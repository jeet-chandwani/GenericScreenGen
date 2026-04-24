# GenericScreenGenApp

ASP.NET Core host for the Generic Screen Generator. It exposes screen APIs, schema validation endpoints, and serves the built Angular client.

## Target Framework
- net8.0

## Key Assets
- `screen/` contains runtime screen configuration JSON files
- `Schemas/ScreenConfigSchema.json` defines the screen JSON schema used for validation
- `ClientApp/` contains the Angular client application

## Endpoints
- `GET /api/screens`
- `GET /api/screens/{fileName}`
- `GET /api/screens/{fileName}/render`
- `GET /api/screens/validation`
- `GET /api/schema`