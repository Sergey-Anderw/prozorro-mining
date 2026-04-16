# ProzorroMining

ProzorroMining is a .NET 8 monorepo for importing public Prozorro tender data, storing filtered procurement data in PostgreSQL, and exposing analytics through a minimal API and a small React dashboard.

The current scope is focused on:

- manual import of Prozorro tenders for electricity procurement
- idempotent persistence into PostgreSQL
- analytics endpoints for savings, top procurers, and top suppliers
- a simple frontend dashboard
- full local startup through `docker compose`

## What The Project Does

The system imports tenders from the public Prozorro API and keeps only tenders that match the business rules:

- CPV code: `09310000-5`
- tender status: `complete`
- time window: last month
- primary date: `dateModified`
- fallback date: `dateCreated`

For every eligible tender, the application:

- upserts the tender by `prozorro_tender_id`
- refreshes contracts
- refreshes tender-supplier links
- reuses or inserts suppliers
- tracks import progress in `import_runs`
- updates `import_checkpoint` after successful completion

On top of the imported data, the API exposes analytics for:

- total budget savings
- top 5 procurers by total contract value
- top 5 suppliers by total contract value

## Stack

### Backend

- .NET 8
- ASP.NET Core Minimal API
- Immediate.Handlers
- FluentValidation
- Serilog
- PostgreSQL
- Npgsql
- Dapper
- `IHttpClientFactory`
- Microsoft HTTP resilience handlers

### Frontend

- React
- TypeScript
- Vite

### Infrastructure

- `docker compose`
- PostgreSQL 16 container
- backend container
- frontend container
- database migrator container

## Architecture

The backend is a modular monolith with vertical slices.

### Projects

- `src/Backend/ProzorroMining.Api`
  HTTP entry point, endpoint mapping, Swagger, health checks, background import queue registration.

- `src/Backend/ProzorroMining.App`
  Use cases, orchestration, validation, import workflow, analytics handlers, persistence abstractions.

- `src/Backend/ProzorroMining.Domain`
  Core enums and domain-level concepts.

- `src/Backend/ProzorroMining.Infrastructure`
  PostgreSQL access, Dapper repositories, Prozorro HTTP client, parsing, rate limiting, migrations support.

- `src/Backend/ProzorroMining.Contracts`
  Shared result model and cross-layer contracts.

- `src/Backend/ProzorroMining.DbMigrator`
  Applies SQL migrations to PostgreSQL.

- `src/Frontend/prozorro-dashboard`
  React dashboard for analytics and manual import trigger.

### Layering Rules

- API depends on App and Infrastructure wiring only
- App contains business orchestration and abstractions
- Infrastructure contains SQL and external API access
- Domain stays small and independent
- SQL does not leak into API
- business logic does not move into Infrastructure

## Main Backend Approaches

### 1. Minimal API + Thin Endpoints

Endpoints are intentionally thin. They delegate to handlers and return typed results.

### 2. Immediate.Handlers

Application use cases are implemented as vertical-slice handlers instead of controllers plus service classes everywhere.

### 3. PostgreSQL With NpgsqlDataSource

The PostgreSQL integration uses `NpgsqlDataSource` as the primary integration point. Repositories use short-lived opened connections per operation. Dapper is used on top of those connections.

### 4. Explicit Dapper Repositories

There is no EF Core and no generic repository abstraction. Each repository exposes explicit operations that match the application use cases.

### 5. Typed Prozorro API Client

The public Prozorro API is accessed through a typed client registered via `IHttpClientFactory`.

The client includes:

- configured timeout
- retry for transient HTTP failures
- bounded detail request rate limiting
- defensive JSON parsing

### 6. Idempotent Import

Import persistence is idempotent:

- tender upsert by `prozorro_tender_id`
- transaction per tender
- child collections are refreshed, not appended blindly

### 7. Single Active Import

Only one import can run at a time.

- a second `POST /api/v1/import/run` returns `409 Conflict`
- stale `Running` imports are marked `Failed` when the application starts again

## Import Flow

The import is manual and asynchronous from the HTTP client's point of view.

### Start

`POST /api/v1/import/run`

The endpoint:

- creates a new `import_runs` record
- rejects the request if another import is already running
- enqueues background execution
- returns `202 Accepted`

### Execution

The background import process:

1. loads the current checkpoint
2. requests the Prozorro feed starting from:
   - `GET /api/2.5/tenders?descending=1`
3. follows pagination using `next_page.path`
4. stops when feed items are older than the effective filter window
5. fetches details for candidate tenders through:
   - `GET /api/2.5/tenders/{id}`
6. applies business filters
7. persists eligible tenders
8. updates import progress counters
9. finalizes `import_runs`
10. updates `import_checkpoint`

## Analytics Logic

### Budget Savings

Formula:

`sum(expected_amount - total_contract_amount)`

Negative values are possible. That means the total signed contract amount is greater than the expected amount for part of the stored data.

### Top 5 Procurers

Grouped by `procuringEntity.name`, ordered by total contract value descending, limited to 5.

### Top 5 Suppliers

Grouped by supplier name, ordered by total contract value descending, limited to 5.

Note: supplier analytics are limited by the current schema because contracts are linked to tenders, not directly to a supplier-specific contract record. For the current task, this is acceptable, but it is less exact than the procurer aggregation.

## API Endpoints

### Import

- `POST /api/v1/import/run`
  Starts a manual import. Returns `202 Accepted`.

- `GET /api/v1/import/status`
  Returns the current running import if one exists, otherwise the latest import run.

### Analytics

- `GET /api/v1/analytics/savings`
- `GET /api/v1/analytics/top-procurers`
- `GET /api/v1/analytics/top-suppliers`

### Health

- `GET /health/live`

### Swagger

- `GET /swagger`

## Database

The current schema supports:

- `tenders`
- `contracts`
- `suppliers`
- `tender_suppliers`
- `import_runs`
- `import_checkpoint`

The design is intended to support later analytics on large data volumes. The repository layer already uses SQL aggregations for dashboard queries.

## Running Locally

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- PostgreSQL

## Backend

```powershell
dotnet restore
dotnet build
dotnet run --project src/Backend/ProzorroMining.Api
```

## Frontend

```powershell
cd src/Frontend/prozorro-dashboard
npm install
npm run dev
```

Frontend runs through Vite and proxies API requests to the backend.

## Database Migrations

```powershell
dotnet run --project src/Backend/ProzorroMining.DbMigrator
```

## Running With Docker Compose

The whole system can be started with:

```powershell
docker compose up -d
```

Services:

- PostgreSQL: `localhost:5432`
- API: `http://localhost:8080`
- Frontend: `http://localhost:3000`
- Swagger: `http://localhost:8080/swagger`

Compose also includes a one-time `db-restore` step. It restores the committed SQL seed into PostgreSQL only when the database is empty.

Useful commands:

```powershell
docker compose ps -a
docker compose logs -f api
docker compose logs -f db-migrator
docker compose logs -f frontend
```

Important:

- PostgreSQL data is stored in the named Docker volume `prozorromining_postgres_data`
- do not run `docker compose down -v` if you want to keep already imported data
- the committed seed file is located at [database/seed/prozorro_data.sql](database/seed/prozorro_data.sql)
- on a fresh machine the first `docker compose up -d` will restore this seed automatically after migrations

## Current Frontend

The dashboard page shows:

- total budget savings
- top 5 procurers
- top 5 suppliers
- current import status
- button to start import

The frontend polls import status while an import is running.

## Testing

The repository contains:

- unit tests for import orchestration and tender eligibility policy
- integration tests for API endpoints and PostgreSQL persistence behavior

### Unit Tests

These tests validate pure application logic without a real database or HTTP host.

They check that:

- import orchestration completes correctly for eligible and ineligible tenders
- import traversal stops at the configured time window
- a second import cannot start while another one is already running
- the eligibility policy accepts only the required Prozorro tenders

Run unit tests:

```powershell
dotnet test tests/Backend/ProzorroMining.UnitTests/ProzorroMining.UnitTests.csproj
```

### Integration Tests

The integration test project starts the real API with `WebApplicationFactory`, provisions PostgreSQL through `Testcontainers`, applies the committed SQL migration, and then verifies end-to-end behavior against the real schema and repository SQL.

These tests cover:

- `GET /health/live`
  confirms the application host starts and responds through the real HTTP pipeline
- `GET /api/v1/import/status`
  verifies that the API returns `Pending` for an empty database and prefers the currently running import over older completed runs
- `GET /api/v1/analytics/savings`
  checks that budget savings are aggregated from persisted tenders and contracts
- `GET /api/v1/analytics/top-procurers`
  checks grouping, fallback to `Unknown`, descending ordering, and top-5 limiting
- `GET /api/v1/analytics/top-suppliers`
  checks supplier aggregation over `suppliers`, `tender_suppliers`, and `contracts`
- `ImportedTenderPersistence.PersistAsync`
  verifies the tender aggregate is upserted idempotently and that contracts and supplier links are refreshed instead of duplicated

Run integration tests:

```powershell
dotnet test tests/Backend/ProzorroMining.IntegrationTests/ProzorroMining.IntegrationTests.csproj
```

Important:

- Docker must be running because the integration tests use a PostgreSQL container
- the test database schema is recreated from `src/Backend/ProzorroMining.DbMigrator/Migrations/0001__initial_schema.sql`

## Known Limitations

- supplier analytics are approximate for multi-supplier tenders because the schema does not yet link each contract to a specific supplier contract record
- import execution is backgrounded in-process, not distributed
- import is manual, not scheduled

## Repository Layout

```text
src/
  Backend/
    ProzorroMining.Api
    ProzorroMining.App
    ProzorroMining.Contracts
    ProzorroMining.DbMigrator
    ProzorroMining.Domain
    ProzorroMining.Infrastructure
  Frontend/
    prozorro-dashboard
tests/
  Backend/
    ProzorroMining.UnitTests
```

## License

MIT. See [LICENSE](LICENSE).
