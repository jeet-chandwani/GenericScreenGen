# Release Notes - BRD-05 Req 5.4: Selection Action Target Uses Screen ID

## Overview
Selection action configuration now uses `target-screen-id` (screen identifier) instead of `target-screen` (screen file name). This standardizes selection navigation contracts around screen IDs and removes file-name coupling from action metadata.

---

## Feature List

### Req 5.4 - Rename action target attribute and migrate values to screen ID

#### JSON Schema (`ScreenConfigSchema.json`)
- Renamed selection-action property from `target-screen` to `target-screen-id`.
- Updated required list to include `target-screen-id`.
- Added kebab-case pattern validation for target screen ID values.
- Updated examples to use IDs such as `screen-employee-detail`.

#### C# Parsing and Models
- `CScreenConfigProvider` DTO now reads `target-screen-id`.
- Validation messages updated to reference `target-screen-id`.
- Selection action contracts and implementations now use:
  - `TargetScreenId`
  - (replacing `TargetScreen`)
- Render model mapping updated to pass `TargetScreenId` through to frontend payloads.

#### API and Angular Integration
- `/api/screens` response now includes `screenId` for each screen list item.
- Angular screen list model now includes `screenId`.
- Navigation handling resolves `navigate:{target}` as screen ID to the corresponding file name before opening the screen.
- Backward-compatible fallback retained: if a token is not found as a screen ID, treat it as a file name token.

#### Screen Config Migration
- Migrated all selection-action definitions in screen config files from:
  - `target-screen`: `Screen-Employee-Detail.json`
- to:
  - `target-screen-id`: `screen-employee-detail`

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Selection target based on screen ID | Navigation config is stable even if screen file names change |
| Consistent ID-based contracts across backend/frontend | Lower risk of mismatched routing metadata |
| Config migration to new attribute | Existing configured selection flows remain functional under the new contract |
