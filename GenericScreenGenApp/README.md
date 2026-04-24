# GenericScreenGenApp

ASP.NET Core middle tier for the Generic Screen Generator. It exposes screen APIs and schema validation endpoints. The Angular frontend is designed to run as a separate deployable application and connect to this API over HTTP.

## Target Framework
- net8.0

## Key Assets
- `screen/` contains runtime screen configuration JSON files
- `Schemas/ScreenConfigSchema.json` defines the screen JSON schema used for validation
- `../GenericScreenGenClientApp/` contains the Angular client application (separate deployable UI)

## Frontend/Backend Separation
- Angular runtime API endpoint is configured in `../GenericScreenGenClientApp/public/runtime-config.js` using `window.__GSG_CONFIG__.apiBaseUrl`
- Default value is `/api` (works with local Angular proxy)
- For a separate middle tier (for example Java/Spring, Node.js, or another .NET service), set `apiBaseUrl` to the full API root URL (for example `https://api.example.com/api`)
- CORS is configured through `Cors:AllowedOrigins` in appsettings
- This .NET project serves API endpoints only; frontend hosting is independent

## Endpoints
- `GET /api/screens`
- `GET /api/screens/{fileName}`
- `GET /api/screens/{fileName}/render`
- `GET /api/screens/validation`
- `GET /api/schema`