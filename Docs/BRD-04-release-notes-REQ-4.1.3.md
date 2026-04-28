# Release Notes — BRD-04 Req 4.1.3: Mandatory Field Validation Gated on Save Feature

## Overview
Mandatory-field enforcement is now conditional on whether the `save` feature is enabled for the screen. Screens that do not expose a Save button (e.g. read-only or display-only screens) no longer block the user with mandatory-field alerts, and the mandatory indicator (`*`) is only shown when saving is possible.

---

## Feature List

### Req 4.1.3 — Mandatory validation only when save is enabled

#### Angular Frontend (`SectionRendererComponent`)
- `validateMandatoryFields(dictValues)` private method added:
  - Iterates over all non-action fields in the section.
  - Skips validation entirely when `canSaveFeature()` returns `false`.
  - Collects fields whose value is empty (via `hasFieldValue()` helper) and shows a browser `alert` listing the missing field names if any are found.
  - Returns `true` (proceed) or `false` (block) to the calling save handler.
- `hasFieldValue(strValue)` private helper — treats blank/whitespace as absent.
- `saveRecordDetail()`, `saveEditRow()`, `saveNewRow()` — all call `validateMandatoryFields(...)` before proceeding with the save action.
- Mandatory indicator (`*`) in field name spans rendered with:
  - `@if (objField.isMandatory && canSaveFeature())` — suppressed when Save is not available.
- `.mandatory-indicator` CSS rule added: red, bold marker styled to sit inline after the field name without disrupting layout.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Validation gated on `save` feature | Read-only or display screens are never blocked by validation alerts |
| `*` indicator only shown when save is active | UI stays clean for non-editable contexts |
| Alert lists all missing fields at once | User can see all problems in one pass before re-attempting to save |
