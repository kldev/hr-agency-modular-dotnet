# Ruch i testy wydajnościowe (k6)

Skrypty generują ruch, który widać na dashboardach Grafany (`./infrastructure/start.sh --observability`,
http://localhost:3000, folder **HR Agency**) oraz w logach i śladach w Rootprint (http://localhost:8282).

| skrypt         | co robi                                                                                   | dashboard           | skrót w `start.sh` |
|----------------|-------------------------------------------------------------------------------------------|---------------------|--------------------|
| `panel.js`     | rekruter: logowanie → lista aplikacji → kandydaci (bez filtra / `search` / `source`) → nowy kandydat | HR Agency overview, PostgreSQL | `--traffic`   |
| `emails.js`    | reset hasła → saga → `SendPasswordReset` → `x.emails` → `q.emails.identity` → emails worker → Mailpit | Emails worker, RabbitMQ | `--emails`    |
| `files.js`     | zmiana avatara przez file service; ~10% plików `.txt` udających PNG (odrzucane, 400)       | File service        | `--files`          |
| `job-board.js` | kandydat na publicznej tablicy (:5050): lista, feed, strona oferty                        | HR Agency overview (`hr-web`) | `--job-board` |

```bash
./infrastructure/start.sh --traffic                  # albo: k6 run k6/panel.js
VUS=50 DURATION=5m ./infrastructure/start.sh --traffic
k6 run -e RATE=10 -e DURATION=5m k6/emails.js
k6 run -e REJECT_RATE=0.3 k6/files.js
k6 run -e SLUGS=hr-agency,flex-jobs k6/job-board.js
```

## Dane startowe

`panel.js`, `emails.js` i `files.js` w `setup()` (poza pomiarem) logują właściciela platformy, zakładają
`ORG_COUNT` organizacji z kontem `Admin` i `Recruiter` w każdej i czekają, aż konta da się zalogować
(logowanie czyta `UserProjection` z async daemona). Slugi biorą się z `RUN_ID` (domyślnie czas + losowa
końcówka), więc kolejne przebiegi na tej samej bazie nie zderzają się slugami.

- **Pusta baza**: gdy nie ma właściciela, skrypt woła `GET /api/development/seed` (tylko środowiska
  `Development`/`docker`, trwa kilka minut).
- **Właściciel z `HR_AGENCY_PASSWORD`**: API zakłada go przy starcie z `HrAgencyEmail`/`HrAgencyPassword`
  z `infrastructure/.env` - wtedy podaj to hasło: `-e OWNER_PASSWORD=...` (albo
  `OWNER_PASSWORD=... ./infrastructure/start.sh --traffic`).
- `job-board.js` potrzebuje opublikowanych ogłoszeń i wygenerowanego feedu: seed + działający
  `feeds-worker`.

## Parametry

| zmienna          | domyślnie               | skrypty                     |
|------------------|-------------------------|-----------------------------|
| `BASE_URL`       | `http://localhost:5000` | wszystkie                   |
| `WEB_URL`        | `http://localhost:5050` | `job-board.js`              |
| `ORG_COUNT`      | `5`                     | panel, emails, files        |
| `VUS`            | `20` / `5` / `10`       | panel / files / job-board   |
| `DURATION`       | `3m`                    | wszystkie                   |
| `RATE`           | `3` (resetów/s)         | `emails.js`                 |
| `REJECT_RATE`    | `0.1`                   | `files.js`                  |
| `SLUGS`          | `hr-agency,flex-jobs,tech-jobs` | `job-board.js`      |
| `RUN_ID`         | czas + losowa końcówka  | panel, emails, files        |
| `OWNER_EMAIL`    | `admin@hr-agency.com`   | panel, emails, files        |
| `OWNER_PASSWORD` | `agent999!` (hasło seeda) | panel, emails, files      |

Progi: `http_req_failed < 1%`; p95 < 500 ms dla odczytów, logowania i resetu hasła, < 800 ms dla zapisu
kandydata i stron tablicy, < 1 s dla uploadu.
