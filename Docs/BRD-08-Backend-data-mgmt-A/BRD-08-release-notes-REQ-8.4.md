# Release Notes — BRD-08 Req 8.4: Optional Screen-DataStore Association

## Overview
Updates API data endpoints so persistence is now conditional on screen-to-datastore association instead of a single hardcoded global datastore.

## Feature List

### Req 8.4 — Not every screen is persisted
- Replaced hardcoded `IDataStore` usage in `Program.cs` with registry-based resolution:
  - `IDataStoreRegistry`
  - `IScreenDataStoreMappingRegistry`
- Added startup wiring methods to initialize datastore registry and mapping registry from content root.
- `/api/data/{screenFileName}/{recordId}` GET and PUT now:
  - resolve screen definition
  - resolve datastore-id for screen-id
  - resolve datastore instance from registry
  - return not found when screen is not mapped to any datastore

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Optional persistence by screen | Screens can exist without backend persistence configuration |
| Registry-based datastore resolution | Data APIs route to the correct backend per screen association |
| Clear not-found behavior for unmapped screens | Safer behavior and better diagnostics for incomplete mapping setup |
