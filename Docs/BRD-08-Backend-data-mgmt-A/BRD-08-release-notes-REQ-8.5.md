# Release Notes — BRD-08 Req 8.5: Screen-DataStore Mapping Registry Files

## Overview
Adds explicit screen-to-datastore mapping artifacts and schema so datastore routing is file-driven and separated from screen definitions.

## Feature List

### Req 8.5 — Mapping registry per datastore
- Added schema: `Schemas/ScreenDataStoreMappingSchema.json`.
- Added mapping file: `DataStoreMappings/screen-datastore-mapping.json-store.json`.
- Mapped existing persisted screens:
  - `screen-employee-list`
  - `screen-employee-detail`
- Mapping registry implementation validates:
  - duplicate screen-id mappings across files
  - unknown datastore-id references

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Dedicated mapping files | Clear and auditable control of which screens persist and where |
| Schema-backed mapping format | Reduces configuration mistakes and improves consistency |
| Duplicate/unknown mapping validation | Fail-fast startup behavior for invalid datastore wiring |
