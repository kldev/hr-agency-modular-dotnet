# Specs - done

Thirty-three features, reverse-engineered on 2026-10-02 from the commit history (603 commits, 2026-08-30 → 2026-10-02),
numbered by the date of their first commit. Each spec says why the feature exists, what it promises, what it touches,
what it deliberately leaves out, and **how each promise is proven** - a `./verify` name, an e2e file, or an honest
`unproven`. Gaps found while reconstructing them are collected in [`../../open.md`](../../open.md).

New specs start from [`../TEMPLATE.md`](../TEMPLATE.md) in `.shipit/specs/` and move here when every criterion is proven.

| # | Feature | First commit | Verify |
|---|---|---|---|
| 001 | [Organizations and tenancy](001-organizations-and-tenancy.md) | 2026-08-30 | `./verify organizations` |
| 002 | [Identity and sign-in](002-identity-and-sign-in.md) | 2026-08-31 | `./verify identity` |
| 003 | [Client companies](003-client-companies.md) | 2026-08-30 | `./verify companies` |
| 004 | [Job descriptions](004-job-descriptions.md) | 2026-09-01 | `./verify job-descriptions` |
| 005 | [Job posts and channels](005-job-posts-and-channels.md) | 2026-09-03 | `./verify job-posts` |
| 006 | [Candidates and applications](006-candidates-and-applications.md) | 2026-09-03 | `./verify candidates` |
| 007 | [Job feeds](007-job-feeds.md) | 2026-09-05 | `./verify feeds` |
| 008 | [Public job board](008-public-job-board.md) | 2026-09-06 | `./verify job-board` |
| 009 | [Interviews](009-interviews.md) | 2026-09-07 | `./verify interviews` |
| 010 | [Sales opportunities](010-sales-opportunities.md) | 2026-09-08 | `./verify sales` |
| 011 | [Agency panel foundation](011-agency-panel-foundation.md) | 2026-09-03 | `./verify suggestions` |
| 012 | [Email notifications](012-email-notifications.md) | 2026-09-19 | `./verify emails` |
| 013 | [Password reset and sessions](013-password-reset-and-sessions.md) | 2026-09-19 | `./verify identity` |
| 014 | [Teams](014-teams.md) | 2026-09-19 | `./verify teams` |
| 015 | [File service](015-file-service.md) | 2026-09-20 | `./verify files` |
| 016 | [Projects and compliance](016-projects-and-compliance.md) | 2026-09-20 | `./verify projects` |
| 017 | [My profile](017-my-profile.md) | 2026-09-20 | `./verify identity` |
| 018 | [Legal entities](018-legal-entities.md) | 2026-09-20 | `./verify legal-entities` |
| 019 | [Workers and assignments](019-workers-and-assignments.md) | 2026-09-20 | `./verify workers` |
| 020 | [Positions](020-positions.md) | 2026-09-21 | `./verify projects` and `./verify workers` |
| 021 | [Agency chart and roles](021-agency-chart-and-roles.md) | 2026-09-21 | `./verify agency` |
| 022 | [Time sheets and settlement](022-time-sheets-and-settlement.md) | 2026-09-22 | `./verify agency` (mail bodies: `./verify emails`) |
| 023 | [Impersonation and avatars](023-impersonation-and-avatars.md) | 2026-09-22 | `./verify identity` |
| 024 | [Application knows its worker](024-application-knows-its-worker.md) | 2026-09-22 | `./verify candidates` and `./verify workers` |
| 025 | [Landing page](025-landing-page.md) | 2026-09-22 | frontend only, no automated check |
| 026 | [Candidate and application timeline](026-candidate-and-application-timeline.md) | 2026-09-23 | `./verify timeline` |
| 027 | [Reports](027-reports.md) | 2026-09-23 | `./verify reports` |
| 028 | [Dynamic forms](028-dynamic-forms.md) | 2026-09-23 | `./verify forms` |
| 029 | [Sales workspace and tasks](029-sales-workspace-and-tasks.md) | 2026-09-24 | `./verify tasks` |
| 030 | [Observability](030-observability.md) | 2026-09-25 | `./verify observability` |
| 031 | [Integration tests per area](031-integration-tests-per-area.md) | 2026-10-01 | `./verify all` |
| 032 | [HeroUI v3 migration](032-heroui-v3-migration.md) | 2026-10-01 | frontend only (`yarn build`, `yarn check`, `yarn test`, `yarn e2e:check`) |
| 033 | [Quality gates and startup validation](033-quality-gates-and-startup-validation.md) | 2026-09-18 | `./verify all` |
