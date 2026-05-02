# Release Notes — BRD-07 Req 7.3: Container Verification

## Overview
Confirms that the Docker Compose setup provides exactly one container per executable component in the solution, satisfying the requirement that each deployable service runs in its own isolated container.

---

## Features

### Req 7.3 — One Container per Executable Component

#### Container Inventory

| Container | Service Name | Source Project | Image Base | Role |
|---|---|---|---|---|
| `generic-screen-gen-api` | `api` | `GenericScreenGenApp` | `dotnet/aspnet:8.0` | ASP.NET Core Web API — serves all `/api/*` endpoints |
| `generic-screen-gen-ui` | `ui` | `GenericScreenGenClientApp` | `nginx:alpine` | Angular SPA served as static files via Nginx |

**Total containers: 2** — one per executable/deployable component.

#### Library Projects — Compiled into `api`, No Separate Containers

The following projects are referenced by `GenericScreenGenApp` and compiled into the `api` container image at build time. They are not separately deployable services and do not require their own containers:

| Project | Role |
|---|---|
| `GenericScreenGenFactoryLib` | Factory abstraction for screen render model creation |
| `GenericScreenGenImplementationsLib` | Core implementations: layout policies, field type registry, screen config provider |
| `GenericScreenGenInterfacesLib` | Shared contracts (interfaces) across the solution |
| `GenericScreenGenUtilsLib` | Utility helpers used by backend components |
| `MyDataStoreProviders` | JSON file-based data store implementation |

#### `MyTestConsoleApp` — Excluded from Containers
`MyTestConsoleApp` is a development-time console utility used for local testing and exploration. It is not a deployable service and has no container.

#### Data Persistence — Named Volume
The `DataStore/` folder (JSON file store) is mounted via a Docker named volume (`datastore_data`) rather than baked into the image. This means:
- Data persists across `docker compose down / up` cycles
- The API container image remains stateless (same image, different data per environment)

---

## Verification Checklist

| Check | Expected Result |
|---|---|
| `docker compose build` | Both `api` and `ui` images build without errors |
| `docker compose up` | Exactly 2 containers start; API logs show listening on port 5074 |
| `docker compose ps` | Shows 2 running services: `api`, `ui` |
| Browser → `http://localhost:4200` | Angular app loads successfully |
| Angular app fetches `/api/screens` | Data returns (Nginx proxies to `api` service internally) |
| `docker compose down && docker compose up` | `DataStore/` JSON files persist (named volume) |
| `git status` | `.env` appears as untracked — not committed |

---

## Summary of User-Facing Benefits

| Req | Change | Benefit |
|---|---|---|
| 7.1 | Dockerfile per executable component | Each service is independently buildable and deployable |
| 7.2 | `.env` for runtime config | Secrets and ports configurable per environment without code changes |
| 7.3 | 2 containers (api + ui) | Clean separation — API and UI can be scaled, updated, or replaced independently |
