# Testy wydajnościowe (k6)

> **Uwaga: skrypt działa na pustej bazie.** Zakłada świeżą bazę bez seeda i bez poprzednich
> przebiegów: sam woła `GET /api/development/seed` (tylko tak powstaje właściciel platformy), a potem
> zakłada organizacje `perf-org-1..N` ze stałymi slugami i domenami. Drugi przebieg na tej samej
> bazie zatrzyma się w `setup()` na zajętym slugu. Na bazie, która nie jest pusta, podaj `RUN_ID`
> (np. `-e RUN_ID=$(date +%s)`) — slugi i domeny będą wtedy nowe, a seed zostanie pominięty, bo
> właściciel już istnieje.

API musi działać w środowisku `Development` albo `docker` (tylko tam jest endpoint seeda).

```bash
k6 run k6/test.js
k6 run -e BASE_URL=http://localhost:5000 -e ORG_COUNT=20 -e VUS=50 -e DURATION=2m k6/test.js
k6 run -e RUN_ID=$(date +%s) k6/test.js     # baza nie jest pusta
```

## Co robi

`setup()` (poza pomiarem):

1. loguje właściciela platformy (`admin@hr-agency.com`), a jeśli go nie ma — odpala seed,
2. zakłada `ORG_COUNT` organizacji, w każdej konto `Admin` i `Recruiter`,
3. czeka, aż każde konto da się zalogować — logowanie czyta `UserProjection` z async daemona
   i do tego czasu odpowiada 500.

Każdy VU w pętli: logowanie losowym kontem → lista aplikacji → lista kandydatów (bez filtra /
`search` / `source`) → utworzenie kandydata.

## Parametry

| zmienna          | domyślnie               |
|------------------|-------------------------|
| `BASE_URL`       | `http://localhost:5000` |
| `ORG_COUNT`      | `10`                    |
| `VUS`            | `30`                    |
| `DURATION`       | `1m`                    |
| `RUN_ID`         | puste                   |
| `OWNER_EMAIL`    | `admin@hr-agency.com`   |
| `OWNER_PASSWORD` | `agent999!`             |

Progi: `http_req_failed < 1%`, p95 < 500 ms dla odczytów i logowania, p95 < 800 ms dla zapisu
kandydata.
