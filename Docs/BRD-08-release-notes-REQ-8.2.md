# Release Notes — BRD-08 Req 8.2: DataStore Provider Implementations

## Overview
Implements concrete datastore provider classes in the provider library so persistence backends are encapsulated in one project.

## Feature List

### Req 8.2 — Provider Implementations in MyDataStoreProviders
- Added `CDbDataStore` in `MyDataStoreProviders` as SQL Server-backed implementation of `IDataStore`.
- `CDbDataStore` uses table `dbo.ScreenData` with key `(ScreenKey, RecordId)` and stores row payload as JSON (`RowData`).
- `CDbDataStore` ensures table creation automatically when accessed.
- Existing `CJsonDataStore` remains in `MyDataStoreProviders` and continues to handle file-backed JSON persistence.
- Added `Microsoft.Data.SqlClient` package to `MyDataStoreProviders.csproj`.

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| SQL Server datastore provider | Enables relational persistence backend for the same screen data contract |
| Providers in one library | Clean separation: API/registry code does not carry backend-specific persistence logic |
| Shared IDataStore contract | Screen data endpoints remain backend-agnostic across provider types |
