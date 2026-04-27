# MyDataStoreProviders

JSON-backed data persistence project for GenericScreenGen.

## Purpose
- Provides `CJsonDataStore`, the first implementation of `IDataStore`.
- Persists each screen's row-data payload as a separate JSON file.
- Keeps storage implementation swappable for future database-backed stores.

## Target Framework
- net8.0
