# Release Notes — BRD-07 Req 7.1: Dockerize the Application

## Overview
Introduces Docker containerization for the GenericScreenGen solution. Each executable component (API and Angular UI) gets its own multi-stage Dockerfile, and a root-level `docker-compose.yml` orchestrates the entire application with a single command.

---

## Features

### Req 7.1 — Docker Containerization

#### `GenericScreenGenApp/Dockerfile`
- Multi-stage build using official Microsoft .NET images
- **Stage 1 (build)**: `mcr.microsoft.com/dotnet/sdk:8.0`
  - Copies all `.csproj` files first to leverage Docker layer caching on `dotnet restore`
  - Restores and publishes `GenericScreenGenApp` in Release configuration, including all referenced library projects (`GenericScreenGenFactoryLib`, `GenericScreenGenImplementationsLib`, `GenericScreenGenInterfacesLib`, `GenericScreenGenUtilsLib`, `MyDataStoreProviders`)
  - `screen/**` and `Schemas/**` JSON files are included automatically (already configured with `CopyToOutputDirectory` in the `.csproj`)
- **Stage 2 (runtime)**: `mcr.microsoft.com/dotnet/aspnet:8.0`
  - Copies only the published output — no SDK or source code in the final image
  - Exposes port `5074`; entrypoint: `dotnet GenericScreenGenApp.dll`
- `DataStore/` is intentionally excluded from the image — persisted at runtime via a Docker named volume

#### `GenericScreenGenClientApp/Dockerfile`
- Multi-stage build using Node.js and Nginx
- **Stage 1 (build)**: `node:20-alpine`
  - Copies `package.json` / `package-lock.json` first for install layer caching
  - Runs `npm ci` (clean, reproducible install) then `npm run build` (Angular production build)
  - Output lands at `dist/clientapp` (configured in `angular.json`)
- **Stage 2 (serve)**: `nginx:alpine`
  - Copies Angular build output to `/usr/share/nginx/html`
  - Copies `nginx.conf` as the active site config
  - Exposes port `80`

#### `GenericScreenGenClientApp/nginx.conf`
- Serves Angular SPA static files from `/usr/share/nginx/html`
- SPA fallback: all unmatched routes return `index.html` (supports Angular client-side routing)
- `location /api/` proxied to `http://api:5074` using Docker Compose's internal DNS — replaces the dev-only `proxy.conf.json`

#### `docker-compose.yml` (solution root)
- Modeled on the project sample, adapted for this solution's file-based data store (no database service)
- **`api` service**: builds from `GenericScreenGenApp/Dockerfile` with solution root as context; maps `${APP_API_PORT}:5074`; mounts `datastore_data` named volume to `/app/DataStore` for persistent JSON file storage; reads `.env` for environment configuration
- **`ui` service**: builds from `GenericScreenGenClientApp/Dockerfile`; maps `${APP_CLIENT_PORT}:80`; depends on `api`
- **`datastore_data` named volume**: ensures `DataStore/` JSON files persist across `docker compose down / up` cycles
- Both services restart `unless-stopped`
- Single command to run the full stack: `docker compose up --build`

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Multi-stage Dockerfiles | Lean production images — no SDK or build tools in runtime containers |
| `docker compose up --build` | Entire application (API + UI) starts with one command |
| Named volume for DataStore | JSON data files persist across container restarts and redeployments |
| Nginx API proxy in UI container | Angular app communicates with the API without CORS issues or manual port wiring in production |
| Layer-cached `restore`/`npm install` | Subsequent builds are faster when source changes but dependencies don't |
