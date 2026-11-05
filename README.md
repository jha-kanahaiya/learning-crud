# Learning CRUD

## Overview

A locally run learning application for practicing ASP.NET Core MVC, Entity Framework Core, and SQLite. It lets one local user create, list, inspect, edit, and delete synthetic learning items. No account, external service, or credential is required.

Each item has an integer ID, a required title (up to 120 characters), an optional description (up to 2,000 characters), and an application-set UTC creation time. Edits preserve the original creation time. The app does not seed sample records; create one through the UI.

## Architecture

This is a single ASP.NET Core MVC application. `LearningItemsController` handles HTTP actions and validation, `LearningDbContext` owns EF Core persistence, SQLite stores the data locally, and Razor views render the UI. The controller uses an input model rather than binding database entities, so clients cannot set IDs or creation timestamps.

Request flow:

```text
Browser → MVC controller and validation → LearningDbContext → SQLite file
       ← Razor view or redirect ← persisted result
```

Mutating MVC actions validate antiforgery tokens. The app uses parameterized EF Core queries, returns 404 for missing item IDs, and provides a generic production error page. It intentionally has no authentication and is intended for local learning only.

## Folder Structure

```text
.
├── .azure/project-plan.md
├── .config/dotnet-tools.json
├── src/LearningCrud.Web/
│   ├── Controllers/              # MVC actions
│   ├── Data/                     # EF Core context and migrations
│   ├── Models/                   # Entity and validated form model
│   ├── Views/LearningItems/      # List, details, create, edit, delete
│   └── wwwroot/                  # Local Bootstrap and static assets
├── tests/LearningCrud.Tests/     # MVC + SQLite integration tests
├── LearningCrud.slnx
├── MEMORY.md
└── README.md
```

## Setup Instructions

Prerequisite: install the .NET 10 SDK. This project was validated with SDK `10.0.403`.

From the project root:

```sh
dotnet tool restore
dotnet restore
```

No secrets or environment-specific credentials are needed. The default connection string is `ConnectionStrings:LearningCrud` in `src/LearningCrud.Web/appsettings.json`. It points to the local `learning-crud.db` file. Override it, if needed, with the `ConnectionStrings__LearningCrud` environment variable.

## Run Instructions

From the project root, apply the checked-in EF Core migration:

```sh
dotnet tool run dotnet-ef database update \
  --project src/LearningCrud.Web/LearningCrud.Web.csproj \
  --startup-project src/LearningCrud.Web/LearningCrud.Web.csproj
```

Then start the app from its project directory so the relative SQLite path resolves consistently:

```sh
cd src/LearningCrud.Web
dotnet run
```

Open the localhost URL printed by `dotnet run`. If HTTPS development certificates are not trusted, follow `dotnet dev-certs https --trust` for your operating system. The database is local, ignored by Git, and can be backed up or removed while the app is stopped. Removing it deletes the locally entered items; rerun the migration command to recreate an empty database.

To add a schema change, run from the project root:

```sh
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project src/LearningCrud.Web/LearningCrud.Web.csproj \
  --startup-project src/LearningCrud.Web/LearningCrud.Web.csproj \
  --output-dir Data/Migrations
dotnet tool run dotnet-ef database update \
  --project src/LearningCrud.Web/LearningCrud.Web.csproj \
  --startup-project src/LearningCrud.Web/LearningCrud.Web.csproj
```

Review and keep generated migration files with the source change.

## Docker Instructions

Docker was not selected for this local-first learning project, so no Dockerfile or container workflow is included. Run the application directly with the .NET 10 SDK as described above.

## Testing Instructions

From the project root:

```sh
dotnet test LearningCrud.slnx
```

The integration tests host the real MVC application and use an isolated in-memory SQLite connection. They exercise create/list/details/edit/delete, validation without persistence, retained creation timestamps, not-found behavior, and antiforgery enforcement. Tests use synthetic data and do not touch the local development database.

## Deployment Instructions

Deployment is outside the approved scope; this project has not been published or deployed. It has no authentication, so do not expose it to a network or deploy it as a multi-user application without first designing and implementing appropriate identity, authorization, privacy, and operational controls.

## Future Improvements

- Add authentication and authorization only if the app becomes multi-user.
- Consider backup/export guidance if learning records become valuable.
- Revisit accessibility and browser support as the UI grows.


cd /Users/kanahaiyajha/Development/copilot-starter/projects/learning-crud

dotnet tool restore
dotnet restore

dotnet tool run dotnet-ef database update \
  --project src/LearningCrud.Web/LearningCrud.Web.csproj \
  --startup-project src/LearningCrud.Web/LearningCrud.Web.csproj

cd src/LearningCrud.Web
dotnet run
