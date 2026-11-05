# Learning CRUD — Project Plan

**Status**: Approved — Implemented
**Created**: 2026-11-05
**Mode**: Interactive

## 1. Project Overview

- **Project name**: `learning-crud`
- **Project type**: Learning
- **Goal**: Build a small, locally runnable CRUD application to learn ASP.NET Core MVC, Entity Framework Core, and SQLite.
- **Target users**: The learner using the app locally.
- **Deployment boundary**: Local development only. GitHub publication is a future goal; this plan does not authorize publishing or pushing.

## 2. Confirmed Requirements

- Use ASP.NET Core MVC on .NET 10 with SQLite.
- Provide a sample create, read, update, and delete workflow.
- Keep the UI minimal and the solution understandable for learning.
- Make the app runnable locally.
- Keep the project ready for eventual GitHub publication.

### Suggested sample record

Use a `LearningItem` record with an integer ID, required title, optional description, and creation timestamp. This is a small illustrative domain for exercising CRUD; it can be renamed or adjusted during implementation without changing the app's scope.

## 3. Solution Architecture

- **Shape**: One ASP.NET Core MVC modular monolith; no separate API, worker, cloud dependency, or distributed services.
- **Web layer**: MVC controllers handle HTTP requests, bind and validate forms, and select Razor views.
- **Persistence boundary**: An EF Core `DbContext` owns SQLite access and persistence mapping. Keep database operations behind this boundary; avoid adding a repository abstraction unless implementation demonstrates a concrete need.
- **UI**: Server-rendered Razor views provide list, details, create, edit, and delete-confirmation pages.
- **Request flow**: Browser form/request → MVC action and validation → EF Core `DbContext` → local SQLite database; results return through Razor views.
- **Failure handling**: Return validation errors in the form, use not-found responses for missing records, and show a useful generic error page for unexpected failures without exposing exception details.
- **Alternatives considered**: A separate frontend/API would add deployment and integration complexity without supporting the stated learning goal. A remote database would conflict with the local-first requirement.

## 4. Data & Persistence

- **Store**: SQLite, using an application-local database file for development.
- **Entity**: `LearningItem`
  - `Id`: generated integer primary key.
  - `Title`: required, trimmed, with a modest maximum length.
  - `Description`: optional, bounded text.
  - `CreatedAtUtc`: set by the application on creation and retained on update.
- **Schema changes**: Use EF Core migrations, with the migration and database setup commands documented in the README.
- **Deletion**: Physical delete with a confirmation page and POST-only action protected by antiforgery validation.
- **Data handling**: Use synthetic learning data only; no personal or sensitive data is required.

## 5. Functional Scope & Application Flow

1. The index page lists existing learning items and provides an empty state when none exist.
2. The user can create an item; server-side validation rejects a missing or invalid title.
3. The user can view an item's details.
4. The user can edit an item while preserving its original creation timestamp.
5. The user can review a confirmation page and delete an item.
6. Missing or stale item IDs return a not-found response rather than silently succeeding.

**Out of scope**: Authentication, roles, search, pagination, attachments, APIs, multi-user access, cloud hosting, and features beyond the sample CRUD.

## 6. Design System & UI

- **Component Library**: Bootstrap 5, using the standard ASP.NET Core MVC/Razor conventions and existing template assets where suitable.
- **Visual direction**: Minimal, responsive pages with clear headings, readable forms, consistent spacing, and straightforward navigation.
- **Core screens**: Item list, item details, create/edit form, delete confirmation, validation feedback, and not-found/error states.
- **Accessibility**: Use semantic HTML, associated form labels, keyboard-operable controls, visible focus states, and validation messages that are understandable without relying on color alone.
- **UI behavior**: Keep navigation and actions predictable; distinguish destructive delete actions and require explicit confirmation.

## 7. Security & Configuration

- No sign-in is required for this local learning application.
- Use MVC model validation, antiforgery tokens on state-changing forms, and EF Core parameterized queries.
- Do not trust posted IDs or fields; load the persisted item and apply only permitted updates.
- Keep development-only database paths and settings in configuration, not hard-coded secrets. This app should require no credentials.
- Configure production-style exception handling separately from detailed development diagnostics.
- Add a `.gitignore` that excludes build outputs, local database files, IDE state, and user-specific configuration before any eventual publication.

## 8. Testing & Acceptance

- **Unit tests**: Validate any non-trivial input or domain rules introduced for `LearningItem`.
- **Integration tests**: Exercise MVC CRUD actions and persistence against an isolated SQLite test database; verify invalid input, missing IDs, and deletion confirmation behavior.
- **Acceptance checks**:
  - The solution restores and builds with the .NET 10 SDK.
  - A learner can create, list, inspect, edit, and delete a record locally.
  - Invalid form input is redisplayed with actionable validation feedback and does not persist.
  - The database is initialized through documented migration steps.
  - No external service or credential is needed to run tests or the application.
- Record the actual commands and outcomes in project documentation when implementation is completed; do not claim checks passed until run.

## 9. Implementation & Local Run Plan

1. Create the .NET 10 MVC solution and a focused test project.
2. Add the `LearningItem` model, EF Core SQLite configuration, initial migration, and local configuration.
3. Implement the MVC CRUD actions and minimal Razor views with validation and not-found handling.
4. Add isolated automated tests for persistence and CRUD behavior.
5. Add README instructions for prerequisites, restore/build/test, migration/database setup, local run, and future GitHub publication hygiene.
6. Verify with the .NET 10 SDK by restoring, building, applying the migration locally, running tests, and manually checking the CRUD flow.

**Local run target**: `dotnet run` after the documented database migration/setup step. Exact solution and test commands will be recorded once the scaffold is established.

## 10. Risks, Constraints & Open Decisions

- **SDK availability**: .NET 10 SDK availability on the learner's machine must be checked during implementation; installation is not part of this plan.
- **Sample domain**: `LearningItem` is a proposed teaching example, not a separately confirmed business domain.
- **Publication**: Before eventual GitHub publication, review generated files, local database artifacts, repository metadata, and documentation for private information. No publication or push is included here.
- **Cloud**: No Azure resources or deployment are planned; the selected requirements are local-first.

## 11. Implementation Outcome

- Implemented the .NET 10 MVC application in `src/LearningCrud.Web`, with EF Core SQLite, an initial migration, Bootstrap/Razor CRUD pages, server-side validation, antiforgery protection, and isolated in-memory SQLite MVC tests.
- Added `README.md`, `MEMORY.md`, `.gitignore`, a local EF CLI tool manifest, and `LearningCrud.slnx`.
- Validation run with .NET SDK 10.0.403:
  - `dotnet restore LearningCrud.slnx` — passed.
  - `dotnet build LearningCrud.slnx --no-restore` — passed with 0 warnings and 0 errors.
  - `dotnet test LearningCrud.slnx --no-restore` — passed (4 tests).
  - `dotnet format LearningCrud.slnx --verify-no-changes --no-restore` — passed.
  - EF Core `database update` — applied `20261105065055_InitialCreate` to the local SQLite database.
  - EF Core `migrations list` — confirmed the initial migration is applied.
  - `dotnet list LearningCrud.slnx package --vulnerable --include-transitive` — reported no vulnerable packages.
  - Local HTTP smoke check `GET /` — returned HTTP 200 and rendered the empty state.
- No publication, push, or deployment was performed.
