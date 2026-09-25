#!/bin/sh
# Runs on every start of the observability profile, against the stack's own PostgreSQL, and is safe
# to repeat - which is why it is a one-shot container and not a docker-entrypoint-initdb.d script:
# those run only on an empty volume, and this stack's volume usually already holds data.
#   - role + database "rootprint" for Rootprint's metadata,
#   - role "postgres_exporter" with pg_monitor: statistics only, no access to table data,
#   - pg_stat_statements (preloaded by the postgres command) for the top queries panels.
# Passwords are re-applied each time, so changing one in .env takes effect on the next start.
set -eu

until pg_isready -h postgres -U "$PGUSER" -d "$PGDATABASE" >/dev/null 2>&1; do sleep 1; done

psql -v ON_ERROR_STOP=1 -h postgres \
  -v rootprint_password="$ROOTPRINT_DB_PASSWORD" \
  -v exporter_password="$POSTGRES_EXPORTER_PASSWORD" <<'SQL'
SELECT format('CREATE ROLE rootprint LOGIN PASSWORD %L', :'rootprint_password')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'rootprint') \gexec
SELECT format('ALTER ROLE rootprint PASSWORD %L', :'rootprint_password') \gexec

SELECT 'CREATE DATABASE rootprint OWNER rootprint'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'rootprint') \gexec

SELECT format('CREATE ROLE postgres_exporter LOGIN PASSWORD %L', :'exporter_password')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'postgres_exporter') \gexec
SELECT format('ALTER ROLE postgres_exporter PASSWORD %L', :'exporter_password') \gexec
GRANT pg_monitor TO postgres_exporter;

CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
SQL

echo "Observability roles, database and pg_stat_statements are in place"
