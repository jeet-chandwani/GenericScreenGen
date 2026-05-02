# Release Notes — BRD-07 Req 7.2: Environment Variables via .env File

## Overview
Introduces a `.env` file at the solution root to hold environment-specific variables (container names, ports, runtime environment) consumed by `docker-compose.yml`. A `.env.example` template is committed to source control as a reference, while the actual `.env` is gitignored to prevent leaking configuration values.

---

## Features

### Req 7.2 — .env File for Docker Configuration

#### `.env` (solution root — gitignored)
- Holds all runtime variables read by `docker-compose.yml` at startup:
  - `APP_API_CONTAINER_NAME` — name of the API Docker container
  - `APP_CLIENT_CONTAINER_NAME` — name of the UI Docker container
  - `APP_API_PORT` — host port mapped to the API container (default: `5074`)
  - `APP_CLIENT_PORT` — host port mapped to the UI container (default: `4200`)
  - `ASPNETCORE_ENVIRONMENT` — ASP.NET Core runtime environment (`Production`)
- The `Cors__AllowedOrigins__0` override in `docker-compose.yml` reads `${APP_CLIENT_PORT}` from this file, so CORS is automatically restricted to the configured client port without changing any source code

#### `.env.example` (committed — safe template)
- Documents all required variables with their default values and descriptions
- Copied and renamed to `.env` by any developer or deployment pipeline setting up the project
- Header comment clearly states: copy to `.env`, fill in values, never commit the actual `.env`

#### `.gitignore` update
- Added `.env` entry at the top of the `.gitignore` file with an explanatory comment
- Ensures the actual environment file (which may contain sensitive values in other environments) is never accidentally committed to source control

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| `.env` file for all runtime config | All tunable Docker parameters in one place — no need to edit `docker-compose.yml` |
| Port configuration via `.env` | Host ports for API and UI can be changed without touching any source files |
| `.env` gitignored | Sensitive environment values are safe from accidental commit |
| `.env.example` committed | New developers can get up and running by copying the template — no guessing required |
