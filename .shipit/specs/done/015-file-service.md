# 015 · File service

**Status:** done · **Built:** 2026-09-20 → 2026-09-25 · **Verify:** `./verify files` · **Plan:** backend 010 (local, not in git)

## Why
Projects, workers, assignments and profiles all carry private documents: contracts, permits, A1
certificates, pictures. They must never leak across agencies, and no business module should know
where a file physically lives. One place has to own storage, ownership and tenant isolation, so that
every module that needs a document asks the same guarded door.

## What it promises
- Private documents are stored only by the file service, a separate HTTP host; it is the only
  process that talks to the `documents` bucket. Metadata (owner, organization, key, size, SHA-256)
  lives in the Postgres schema `files`.
- A domain knows a `FileId` and nothing else. The storage key
  `{organizationId}/{ownerKind}/{ownerId}/{fileId}{ext}` is built inside the service and never
  from the uploader's file name; no endpoint accepts a key.
- Every call carries the organization as a claim of a signed service token (`iss: hr-api`,
  `aud: file-service`, two minute lifetime, own secret `FileService:Secret`). A token with another
  secret, issuer or audience, an expired one, or one without a valid organization claim is refused.
- A file of another organization answers 404, never 403.
- Uploads are checked before they reach the bucket or the database: empty files, files over the size
  limit (25 MB by default), types outside the allow list and an extension that contradicts the
  content type are refused with the reason. The service ships with an allow list, so a missing config
  section is not an open door.
- File names are sanitised (last path segment, no control characters, truncated keeping the extension).
- In the API, a refusal from the file service is a 400 with its reason; the service being down is a
  503 (`FileServiceException`), and the API's health reports it as degraded.

## Surface
- Backend: `src/services/HrAgencySystem.FileService` (`Endpoints`, `Application/FileStore`,
  `UploadInspector`, `Domain/StorageKey`, `Auth`), `src/services/HrAgencySystem.FileService.Contracts`
  (`IFileServiceClient`, `FileDescriptor`, `FileOwnerRef`, `FileServiceToken`),
  `src/HrAgencySystem.Files` (`IObjectStorage`, shared S3/RustFS library), `Dockerfile.file-service`.
- Service endpoints: `POST /files`, `GET /files/{id}`, `GET /files/{id}/content`, `DELETE /files/{id}`, `GET /healthz`.
- Consumers: document endpoints of projects, workers, assignments and the avatar endpoints.
- Tests: `tests/HrAgencySystem.FileService.UnitTests`, `tests/HrAgencySystem.UnitTests/Files`;
  integration tests use `FakeFileServiceClient`.

## Out of scope for this feature
- Download tickets / pre-signed URLs for the browser - designed, not implemented; the browser only talks to the HR API (not yet).
- Virus scanning and content sniffing (magic bytes) - `IUploadInspector` is the seam (not yet).
- File versioning, per-organization quotas, orphan cleanup, mTLS (not yet).
- Feeds stay on `IObjectStorage` directly: a feed file is public and has a deterministic key (not ever).

## Acceptance criteria
- [x] Service token: valid accepted; wrong secret, issuer, audience, expired, missing or unparsable organization refused — `./verify files`
- [x] Storage key built from organization and owner, ignores the uploader's name, owner kind cannot escape the prefix — `./verify files`
- [x] Upload rules (empty, too large, type, extension mismatch, charset, jpeg alias) — `./verify files`
- [x] A refused upload never reaches the bucket or the database; an accepted one is hashed — `./verify files`
- [x] API maps a refusal to the uploader's mistake and an outage to unavailable — `./verify files`
- [x] Another organization cannot read a project document through the API (404) — `./verify projects`
- [ ] The file service itself answers 404 for a file of another organization — unproven (no test found; integration tests use a fake client)

## Decisions
- Private documents behind a separate host, service JWT, `FileId` only, 404 across tenants:
  [ADR-0012](../../../docs/adr/0012-separate-file-service.md).
- An exception to the single deployable: [ADR-0001](../../../docs/adr/0001-modular-monolith.md).
- The endpoint turns uploaded bytes into a `FileId` before building the command, because a Wolverine
  command goes through the outbox and cannot carry a `Stream` - the one sanctioned exception to thin endpoints.
- The service secret must differ from `Jwt:SecretKey`; one key for both would make every user token a service token.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-20 | `c889602b` | S3 adapter turned into the shared `Files` library |
| 2026-09-20 | `a3f09189` | File service contract shared by API and service |
| 2026-09-20 | `3ff75a9d` | File service with tenant scoped uploads and downloads |
| 2026-09-20 | `07be0105` | Tests for keys, upload rules, service token |
| 2026-09-20 | `af2569e0` | API calls the service through a typed client with a short-lived token |
| 2026-09-20 | `a592e12b` | Project documents through the file service |
| 2026-09-25 | `4064aedd` | Upload metrics |
| 2026-10-02 | `d5b186f6` | Options validated at startup |

## Notes from reconstruction
- Plan 010 records that a module inside the monolith was recommended and the separate service was
  the owner's explicit choice ("the process boundary is one nobody can shortcut").
- Since this feature the API is no longer self-sufficient: document endpoints fail without the
  service, and no integration test notices, because they all use a fake.
