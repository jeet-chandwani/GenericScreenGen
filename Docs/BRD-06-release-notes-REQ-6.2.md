# Release Notes - BRD-06 Req 6.2: Enforce Unique Field Type ID and Name

## Overview
Field type registry loading now validates uniqueness for both field type `id` and `name`. Duplicate definitions are rejected early with clear startup errors.

---

## Feature List

### Req 6.2 - Unique id and name per field type

#### Backend Validation (`CFieldTypeRegistry`)
- Added duplicate `id` detection during registry initialization.
- Added duplicate `name` detection during registry initialization.
- Registry initialization now fails fast with explicit error messages when duplicates are found.

#### Validation Behavior
- `id` checks are case-insensitive after normalization.
- `name` checks are case-insensitive for consistency and safety.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Duplicate id protection | Prevents ambiguous field type lookups |
| Duplicate name protection | Keeps registry definitions predictable and maintainable |
| Fail-fast startup validation | Configuration mistakes are detected immediately |
