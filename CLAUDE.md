# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Overview

**GenericScreenGenSln** is a JSON-driven screen generator: screens are declared as JSON config files; the API parses them into typed models; the Angular client renders them dynamically without any per-screen code. It targets .NET 8 (C# backend) and Angular 20 (TypeScript frontend).

## Build & Run Commands

### Backend (from solution root)
```powershell
# Build entire solution
dotnet build GenericScreenGenSln.sln

# Run the API (http://localhost:5074)
dotnet run --project GenericScreenGenApp/GenericScreenGenApp.csproj

# Run with hot-reload
dotnet watch --project GenericScreenGenApp/GenericScreenGenApp.csproj
```

### Frontend (from GenericScreenGenClientApp/)
```powershell
# Install dependencies
npm ci

# Dev server with proxy to API on :5074 (http://localhost:4200)
npm start

# Production build
npm run build

# Run tests
npm test
```

### Docker (full stack)
```powershell
# Copy .env.example → .env and set DB_PWD etc., then:
docker compose up --build
```
Docker services: `sqlserver` (SQL Server 2022), `api` (port from `.env` → container :5074), `ui` (Nginx serving Angular build → container :80).

## Architecture

### Project Dependency Graph
```
GenericScreenGenApp (ASP.NET Minimal API)
  ├── GenericScreenGenFactoryLib      ← creates and wires all components
  ├── GenericScreenGenImplementationsLib ← concrete classes (C-prefix)
  ├── GenericScreenGenInterfacesLib   ← all interfaces (I-prefix) and enums
  ├── MyDataStoreProviders            ← CJsonDataStore, CDbDataStore
  └── GenericScreenGenUtilsLib        ← CScreenGeneratorConstants, shared helpers
```

`GenericScreenGenClientApp/` is an independent Angular workspace.

### Key Concepts

**Screen Config Files** live in `GenericScreenGenApp/screen/` and match the glob `Screen-*.json`. Each file declares a screen with `id`, `name`, `features`, `key`, `theme`, and nested `sections`. `CScreenConfigProvider` loads all of them at startup and caches them in memory; `/api/screens/refresh` triggers a hot reload.

**Layout Policies** control how sections render fields. The four policies registered in `Program.cs` are: `per-line`, `flow`, `tabular`, `record-detail`. Tabular sections require at least one `selection-action` (`click` or `double-click`).

**Field Types** are declared in `screen/registry-field-types.json` and resolved by `CFieldTypeRegistry`. Supported `type` values in screen JSONs: `Text`, `Integer`, `Date`, `date-time`, `Lookup`, `Button`. Each field may include a `type-info` string in the format `{key=value;key=value;}` that overrides registry defaults (e.g. `{min-chars=0;max-chars=60;lines=1;}`). Lookup type-info uses semicolon-separated option values with optional `multiple` flag.

**Data Store Stack** (BRD-08):
- `DataStoreConfigs/datastore.*.config.json` — declares data stores by `id` and `provider-type` (`json` or `sql-server`)
- `DataStoreMappings/screen-datastore-mapping.*.json` — maps `screen-ids` arrays to a `datastore-id`
- `CDataStoreRegistry` reads the config files; `CScreenDataStoreMappingRegistry` cross-references the mapping files against the registry at startup
- Data endpoints: `GET/PUT /api/data/{screenFileName}/{recordId}` — rows are keyed by a synthetic `__record-id` field

**Init Pattern** — all components extend `ACanInitBase` (template method). `Init(object, out string)` calls `TryInitCore` then `InitAfterLoad`; it is idempotent (second call returns true immediately). Factory (`CGenericScreenGenFactory`) must be initialized first with a screen folder path.

### API Endpoints (all in `Program.cs`)
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/` | API root / route listing |
| GET | `/health` | Health check |
| GET | `/api/screens` | List all loaded screens |
| GET | `/api/screens/{fileName}` | Get screen definition |
| GET | `/api/screens/{fileName}/render` | Get render model for Angular client |
| POST | `/api/screens/refresh` | Hot-reload all screen configs |
| GET | `/api/screens/validation` | Validate all screens against JSON schema |
| GET | `/api/schema` | Serve `Schemas/ScreenConfigSchema.json` |
| GET | `/api/data/{fileName}/{recordId}` | Load a record |
| PUT | `/api/data/{fileName}/{recordId}` | Upsert a record |

CORS is configured via `Cors:AllowedOrigins` in `appsettings.json` / environment variable (`Cors__AllowedOrigins__0`).

### Angular Client (`GenericScreenGenClientApp/src/`)
- `ScreenApiService` — all HTTP calls to the backend
- `SectionRendererComponent` — single standalone component that handles all four layout policies; uses Angular signals for state
- `screen.models.ts` — TypeScript mirror of the backend render model types
- `proxy.conf.json` — dev proxy routes `/api/*` to `:5074`
- The `__record-id` and `__source-screen` synthetic field names are internal constants shared between the Angular component and the API

### Naming Conventions (C# only)
- Classes: `C` prefix (e.g. `CScreenConfigProvider`)
- Interfaces: `I` prefix (e.g. `IScreenConfigProvider`)
- Abstract base classes: `A` prefix (e.g. `ACanInitBase`)
- Enums: `E` prefix (e.g. `EFieldType`)
- Local variables: `str` / `i` / `f` / `lst` / `dict` / `arr` / `obj` Hungarian-style prefixes
- Member fields: `m_` prefix

### JSON Config File Conventions
- Screen files: `Screen-*.json` in `GenericScreenGenApp/screen/`
- DataStore configs: `datastore.*.config.json` in `GenericScreenGenApp/DataStoreConfigs/`
- DataStore mappings: `screen-datastore-mapping.*.json` in `GenericScreenGenApp/DataStoreMappings/`
- Field type registry: `registry-field-types.json` in `GenericScreenGenApp/screen/`
- JSON schemas for all config formats live in `GenericScreenGenApp/Schemas/`

### Schemas
All config files have a corresponding JSON schema in `GenericScreenGenApp/Schemas/`:
- `ScreenConfigSchema.json` — screen definition files
- `DataStoreConfigSchema.json` — datastore config files
- `ScreenDataStoreMappingSchema.json` — mapping files
- `FieldTypesRegistrySchema.json` — field type registry

### Static Fallback UI
`GenericScreenGenApp/wwwroot/index.html` is a self-contained vanilla-JS screen viewer that calls the same API — useful for quick local testing without running the Angular app.
