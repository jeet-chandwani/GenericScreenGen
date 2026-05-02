# Release Notes - BRD-05 Req 5.1: Mandatory Screen Key Attribute

## Overview
Each screen configuration now requires a new root attribute named `key`. The attribute is modeled as an array of field IDs and can be left empty when a unique key is not known yet.

---

## Feature List

### Req 5.1 - Add mandatory screen `key`

#### JSON Schema (`ScreenConfigSchema.json`)
- Added root-level `key` to the required attributes list.
- Defined `key` as an array of kebab-case field IDs.
- Allowed empty arrays to support screens where a record key is not defined yet.
- Added examples for empty, single-field, and composite keys.

#### C# Interfaces and Implementations
- `IScreenDefinition` now exposes `IReadOnlyList<string> Key`.
- `CScreenDefinition` constructor and model updated to carry the parsed key field IDs.

#### C# Config Parsing (`CScreenConfigProvider`)
- `CScreenDocumentDto` now requires a `key` JSON property.
- Added key normalization logic:
  - trims entries,
  - lowercases values,
  - removes duplicates,
  - ignores blank entries.
- Passes normalized key IDs into `CScreenDefinition`.

#### Screen Configuration Migration
- Updated all existing screen config files under `GenericScreenGenApp/screen/` to include:
  - `"key": []`

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Mandatory screen-level key contract | Every screen now has a predictable key definition shape |
| Empty key supported | Teams can onboard screens incrementally without blocking unknown key design |
| Normalized key parsing | Reduces case/whitespace/duplicate inconsistencies in config data |
