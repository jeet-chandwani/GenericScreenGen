# Release Notes — BRD-04 Req 4.1.2: Centralized Screen Features (Save, Cancel, Show Original Values)

## Overview
A new `features` array attribute on the screen JSON root gives screen authors declarative control over which action features are available: `save`, `cancel`, and `show-original-values`. When the array is omitted, all three features are enabled by default to preserve backward compatibility.

---

## Feature List

### Req 4.1.2 — Screen `features` attribute

#### JSON Schema
- New optional root-level `"features"` array in `ScreenConfigSchema.json`.
- Allowed enum values: `"save"`, `"cancel"`, `"show-original-values"`.
- Duplicate values are silently deduplicated; unknown values are rejected by schema validation.

#### C# Backend
- `IScreenDefinition` — new `IReadOnlyList<string> Features { get; }` property.
- `CScreenDefinition` — new constructor parameter `IReadOnlyList<string> lstFeatures`; `Features` property.
- `IScreenRenderModel` — new `IReadOnlyList<string> Features { get; }` property (before `Sections`).
- `CScreenRenderModel` — new constructor parameter `IReadOnlyList<string> lstFeatures` (after `strDisplayName`); `Features` property.
- `CScreenConfigProvider.CScreenDocumentDto` — new `[JsonPropertyName("features")] List<string>? Features` DTO property.
- `CScreenConfigProvider` — `NormalizeFeatures(List<string>?)` private static method: validates tokens against the allowed set, deduplicates, and defaults to all three features when the JSON array is absent.
- `s_arrDefaultScreenFeatures = ["save", "cancel", "show-original-values"]` static field.
- `CScreenRenderModelFactory.TryCreateRenderModel` — passes `itfScreenDefinition.Features` to `CScreenRenderModel`.

#### Angular Frontend
- `ScreenRenderModel` model — new `features: string[]` field.
- `App` template (`app.html`) — `[screenFeatures]="objRenderModel.features"` passed to `app-section-renderer`.
- `SectionRendererComponent` — new `@Input() screenFeatures: string[] = []`.
  - `canSaveFeature()`, `canCancelFeature()`, `canShowOriginalValuesFeature()` computed methods.
  - `hasFeature(strFeatureId)` private helper — returns `true` when `screenFeatures` is empty (backward-compat default) or the feature id is present in the array.
  - Save, Cancel, and Show-Original-Values buttons in tabular-edit and record-detail sections are wrapped in `@if (canXFeature())`.
  - Nested `app-section-renderer` elements also receive `[screenFeatures]="screenFeatures"` to propagate the setting.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Per-screen `features` array | Author can expose only the actions relevant to a given screen |
| Omit array = all features on | No breaking change for existing screen definitions |
| Consistent propagation | Feature gates apply uniformly across tabular, record-detail, and nested sections |
