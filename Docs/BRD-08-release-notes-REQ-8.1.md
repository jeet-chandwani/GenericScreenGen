# Release Notes — BRD-08 Req 8.1: DataStore Foundation Contracts

## Overview
Introduces the contract layer required for pluggable data store management while preserving the existing `IDataStore` persistence API.

## Feature List

### Req 8.1 — Backend Data Store Foundation
- Added `IDataStoreConfig` to represent one configured data store entry.
- Added `IDataStoreRegistry` for resolving an `IDataStore` by configured data store id.
- Added `IScreenDataStoreMappingRegistry` for resolving screen-id to datastore-id associations.
- Kept existing `IDataStore` contract unchanged, so current JSON persistence behavior remains backward compatible.

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Data store registry abstraction | Multiple persistence backends can be introduced without changing API contract |
| Screen-to-datastore mapping contract | Allows optional persistence per screen and future per-screen routing |
| Config contract | Enables file-driven data store setup instead of hardcoded backend selection |
