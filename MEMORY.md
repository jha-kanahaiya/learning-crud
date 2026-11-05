# Project Memory

## Project Summary

- `learning-crud` is a `Learning` project: a locally runnable .NET 10 ASP.NET Core MVC CRUD sample.
- Intended user: one learner running the app locally.
- Delivery status: implemented; local only. No publishing or deployment was performed.

## Requirements

- Use ASP.NET Core MVC, EF Core, and SQLite; keep UI and architecture understandable for learning.
- Support create, list, details, edit, and confirmed delete for `LearningItem`.
- Validate title and description on the server, protect state changes with antiforgery, preserve creation time on edit, and return not-found for missing IDs.
- Initialize the schema using EF Core migrations; require no external services, credentials, or seed data.
- Include automated tests, local run instructions, `.gitignore`, README, and this memory record.

## Architectural Decisions

- Use one MVC modular monolith; no separate API, repository layer, cloud dependency, or authentication.
- `LearningItemsController` owns HTTP/form flow and delegates persistence to `LearningDbContext`.
- Use a separate input model for create/edit so database-generated ID and creation timestamp are not client-bound.
- Store data in a local SQLite file; keep generated database and build artifacts out of Git.
- Use server-rendered Razor and the MVC Bootstrap assets from the .NET template.

## Features Implemented

- Learning item list and empty state, details, create, edit, and delete-confirmation pages.
- Title trimming, required/max-length validation and optional bounded description.
- Application-set UTC creation time retained through updates.
- Antiforgery validation for unsafe MVC actions; missing records return 404.
- Initial EF Core migration and an in-memory SQLite MVC integration test suite.

## Decisions Log

| Date | Decision | Rationale and alternatives | Status |
|---|---|---|---|
| 2026-11-05 | Keep the sample as a single MVC application with SQLite | Matches the approved local learning scope; a separate API/frontend or remote database would add unnecessary complexity | Accepted |
| 2026-11-05 | Require explicit EF migration application; do not auto-migrate or seed | Keeps schema changes visible and prevents hidden startup writes or sample-data assumptions | Accepted |

## Technical Debt

- No reproducible debt is known after the documented build and integration tests.

## Future Roadmap

1. Reassess authentication, authorization, and data management only if multi-user or network exposure becomes an approved requirement.
2. Consider backup/export guidance if locally entered learning records become valuable.

## Known Issues

- The application has no authentication by design and is suitable only for local use.

## Open Tasks

- None for the approved local MVP. Deployment and GitHub publication remain outside scope.

## Implementation Updates

| Date | Change summary | Verification |
|---|---|---|
| 2026-11-05 | Implemented the .NET 10 MVC + EF Core SQLite CRUD app, migration, tests, and local documentation. | Restore and build passed; all 4 integration tests passed; format verification passed; initial migration applied and listed; package audit found no vulnerable packages; local `GET /` returned HTTP 200. |
