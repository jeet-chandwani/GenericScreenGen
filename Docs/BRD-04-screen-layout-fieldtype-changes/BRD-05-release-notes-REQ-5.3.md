# Release Notes - BRD-05 Req 5.3: Explicit Selection Actions for Click and Double-Click

## Overview
Section-level record selection behavior is now configured through explicit `selection-actions` entries instead of the legacy `detail-screen` property. This enables dedicated action definitions for `click` and `double-click`, each with target child screen and record-id propagation control.

---

## Feature List

### Req 5.3 - Define selection actions with child screen and record-id handling

#### JSON Schema (`ScreenConfigSchema.json`)
- Removed legacy `detail-screen` from section schema.
- Added `selection-actions` array on sections.
- Added new `$defs.selection-action` object:
  - `event` (required): `click` or `double-click`
  - `target-screen` (required): child screen config file
  - `include-record-id` (optional, default true)

#### C# Interfaces and Implementations
- Added `IScreenSelectionActionDefinition` and `CScreenSelectionActionDefinition`.
- Updated `IScreenSectionDefinition` and `CScreenSectionDefinition` to expose `SelectionActions`.
- Added render equivalents:
  - `IScreenRenderSelectionActionModel`
  - `CScreenRenderSelectionActionModel`
- Updated `IScreenRenderSectionModel` and `CScreenRenderSectionModel` to expose `SelectionActions`.

#### C# Config Parsing and Render Mapping
- `CScreenConfigProvider` now parses `selection-actions` and validates:
  - event must be `click` or `double-click`
  - `target-screen` must be non-empty
  - event entries must be unique per section
- Tabular sections now require at least one `selection-action`.
- `CScreenRenderModelFactory` now maps selection actions into render models.

#### Angular Frontend
- Updated section model to use `selectionActions` array.
- Updated tabular row interaction handling:
  - supports explicit `click` and `double-click` actions,
  - uses click-delay disambiguation when both events are configured,
  - navigates using configured `targetScreen`,
  - propagates record-id when `includeRecordId` is enabled.
- Removed dependency on `detailScreen` in the section renderer.

#### Screen Config Migration
- Migrated `Screen-Employee-list.json` from:
  - `detail-screen`
- To:
  - `selection-actions` with both `click` and `double-click` targeting `Screen-Employee-Detail.json` and including record-id.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Explicit click/double-click action config | Selection behavior is clear, declarative, and easier to extend |
| Child screen defined per action | Record navigation target is directly controlled from config |
| Record-id propagation flag on action | Each action can explicitly include record context for updates |
| Legacy detail-screen removed | Single, consistent selection model across backend and frontend |
