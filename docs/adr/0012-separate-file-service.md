# ADR-0012: Private documents live behind a separate file service; the API calls it with a short-lived service token and the domain knows only a `FileId`

- **Status:** Accepted
- **Date:** 2026-09-20
- **Evidence:** `a3f09189` feat(files): add the file service contract shared by the api and the service, `3ff75a9d` feat(files): add the file service with tenant scoped uploads and downloads, `07be0105` test(files): cover storage keys, upload rules and service token validation, `af2569e0` feat(api): call the file service through a typed client with a short lived service token, `0fd76bd3` chore(files): ship the file service in the stack and surface it in health and errors, `a592e12b` feat(projects): attach, describe and remove project documents through the file service
- **Specs:** [015](../../.shipit/specs/done/015-file-service.md), [016](../../.shipit/specs/done/016-projects-and-compliance.md), [017](../../.shipit/specs/done/017-my-profile.md), [019](../../.shipit/specs/done/019-workers-and-assignments.md)

## Context
Projects, workers, assignments and user avatars need documents: contracts, A1 certificates,
identity papers. These are private, belong to one agency, and several modules need them. The
existing `HrAgencySystem.Files` library (`IObjectStorage`) knew only keys and buckets - no tenant,
no owner, no delete or metadata.

## Decision
- `src/services/HrAgencySystem.FileService` is its own HTTP host (port 5100, `Dockerfile.file-service`)
  and the only process that touches the `documents` bucket and the `files` schema
  (`StoredFile`: owner, organization, key, size, sha256). It uses Marten as a document store only,
  without Wolverine or the daemon.
- The service builds the key `{organizationId}/{ownerKind}/{ownerId}/{fileId}{ext}`; no endpoint
  accepts a key. A domain stores a `FileId`; owners are opaque pairs `FileOwnerRef(kind, id)`
  (`project`, `user`, `worker`, `assignment`).
- The API calls it through `IFileServiceClient` (in `FileService.Contracts`) with an HMAC JWT
  minted per call by `FileServiceTokenFactory`: `iss: hr-api`, `aud: file-service`, claim `org`,
  lifetime two minutes, secret `FileService:Secret` that must differ from `Jwt:SecretKey`.
- The organization comes only from the signed claim. A file of another organization answers 404,
  like an unknown one; 403 would confirm that the id exists.
- The API decides whether this user may touch this project; the service decides whether this
  file belongs to this organization and owner. Two checks, two questions.
- `FileServiceException` -> 503 in the API; a file the service refuses -> 400 with its reason.
- Upload endpoints resolve the multipart body into a `FileId` before building the command
  ([ADR-0008](0008-endpoint-per-file-thin-api.md)). Public feed files keep using `Files` directly
  ([ADR-0009](0009-feeds-own-read-model-and-worker.md)).

## Alternatives considered
From backend plan 010 (local, not in git):
- **A `Files` module inside the API with a proper contract** - the author's recommendation,
  in line with [ADR-0001](0001-modular-monolith.md). The product owner chose a separate process:
  many modules will need files, and a process boundary cannot be bypassed by a shortcut.
- **mTLS between API and service** - rejected: no certificates or CA anywhere in the stack; the
  token factory is the single place to swap later.
- **A separate database** - deferred; own schema and credentials on the shared instance.
- **Download tickets / pre-signed URLs for the browser** - designed, not built; the browser never
  talks to the service in v1.

## Consequences
- The API is no longer self-sufficient: document endpoints fail without the service.
  `/healthz` reports `fileService`, and `/health/ready` reports it as `Degraded`.
- Integration tests use a fake client that enforces organization isolation, so the real network
  path is checked only in compose and by the k6 `--files` script.
- Not built yet: antivirus scan (the seam is `IUploadInspector`), content sniffing, file versions,
  orphan clean-up, per-agency quotas.
- The plan proposed a one-minute token; the code uses two minutes (`FileServiceToken.Lifetime`).
- Every Dockerfile must copy the contracts project from its real path (`f51d8a56`).

## Revisit when
1. The browser must download large files directly - build the ticket endpoint.
2. Orphaned `StoredFile` rows become visible in practice - add the clean-up job (own plan).
3. The service needs its own database or a second consumer outside the API.
