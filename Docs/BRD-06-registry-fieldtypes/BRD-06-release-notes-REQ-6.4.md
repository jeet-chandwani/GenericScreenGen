# Release Notes - BRD-06 Req 6.4: Merge Type Defaults with Field Overrides

## Overview
Implemented merge behavior between field type registry defaults and per-field `type-info` values. The effective type-info now starts from registry defaults and applies any field-level overrides.

---

## Feature List

### Req 6.4 - Effective type-info merge

#### Dependency Wiring
- Updated factory and app wiring to pass `IFieldTypeRegistry` into screen config provider creation:
  - `IGenericScreenGenFactory`
  - `CGenericScreenGenFactory`
  - `Program.cs`

#### Screen Config Provider Merge Logic
- `CScreenConfigProvider` now resolves each field type against the registry and computes effective `type-info`.
- Merge behavior:
  - If `type-info` is empty: use registry defaults.
  - If `type-info` is key-value format: merge defaults + overrides (override wins).
  - If `type-info` is non key-value format: preserve existing value as-is for backward compatibility.
- Added special handling for lookup defaults (`values`, `multiple`) when constructing effective lookup type-info.

#### Schema Documentation
- Updated `ScreenConfigSchema.json` `type-info` description to clarify default + override behavior.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Default + override merge | Centralized defaults with per-field customization |
| Registry-backed field type validation | Better safety against undefined field types |
| Backward-compatible non key-value handling | Existing configs continue to work while migration is incremental |
