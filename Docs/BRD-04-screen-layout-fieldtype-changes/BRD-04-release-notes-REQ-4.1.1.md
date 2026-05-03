# Release Notes — BRD-04 Req 4.1.1: Screen ID and Name with Filename Defaults

## Overview
Each screen definition can now carry an explicit `id` and `name` in its JSON. When these are omitted the system derives sensible defaults from the config filename, so all existing screens continue to work without modification.

---

## Feature List

### Req 4.1.1 — Screen-level `id` and `name` attributes

#### JSON Schema
- Two new optional root-level properties added to `ScreenConfigSchema.json`:
  - `"id"` — free-form string identifier for the screen.
  - `"name"` — human-readable display name for the screen.

#### C# Backend — Config & Utility
- `CScreenConfigProvider.CScreenDocumentDto` — new `[JsonPropertyName("id")] string? Id` and `[JsonPropertyName("name")] string? Name` DTO properties.
- `CScreenConfigProvider` — reads optional `id` and `name` from JSON; falls back to:
  - `id` → `CScreenNameUtility.GetScreenIdFromFileName(strFileName)`
  - `name` → `CScreenNameUtility.GetScreenNameFromFileName(strFileName)` (strips `"Screen-"` prefix and `.json` suffix, preserves original casing).
- `CScreenNameUtility` — new helper `GetScreenNameFromFileName(strFileName)` returns the raw stem without prefix/extension. `GetDisplayNameFromFileName` now delegates to it.

#### API Endpoint
- `GET /api/screens` now calls `TryGetScreenDefinition` per file to resolve `DisplayName` from the JSON-configured `name` field rather than computing it from the filename directly.
- Screens with an explicit `"name"` in JSON show that name in the home screen list; screens without one continue to show the filename-derived name.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Explicit screen `id` | Stable programmatic identifier independent of filename |
| Explicit screen `name` | Override display name shown in the home screen list |
| Filename defaults | Zero migration cost — existing screens are unaffected |
