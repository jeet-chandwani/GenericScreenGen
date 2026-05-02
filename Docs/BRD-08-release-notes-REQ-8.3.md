# Release Notes — BRD-08 Req 8.3: DataStore Config Registry and Folder Segregation

## Overview
Introduces datastore configuration files with dedicated folder structure outside `screen`, and a registry implementation that loads datastore providers from those config files.

## Feature List

### Req 8.3 — Config files and Registry
- Added new folder structure in `GenericScreenGenApp`:
  - `DataStoreConfigs/` for datastore.*.config.json files
  - `DataStoreMappings/` reserved for screen mapping files
- Added config files:
  - `datastore.json-store.config.json`
  - `datastore.sql-primary.config.json`
- Added schema `Schemas/DataStoreConfigSchema.json`.
- Added `CDataStoreConfig` and `CDataStoreRegistry` in `GenericScreenGenImplementationsLib`.
- `CDataStoreRegistry` loads all `datastore.*.config.json` files and instantiates provider implementations by `provider-type`.
- Updated `GenericScreenGenApp.csproj` to copy `DataStoreConfigs/**` and `DataStoreMappings/**` JSON files to output.
- Added shared constants for datastore config/mapping folders.

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Dedicated config folders | Keeps `screen` focused only on screen definitions |
| File-driven registry | Add/adjust datastore backends through config without code edits in API endpoints |
| Multi-provider support | JSON and SQL providers can coexist and be selected by datastore id |
