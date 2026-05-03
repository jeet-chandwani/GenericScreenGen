# Release Notes — BRD-04 Req 4.1.0: Field Max-Width Property

## Overview
Every field type now supports an optional `max-width` JSON attribute that caps the horizontal space a field control can occupy, independent of the `width` property. This is particularly useful for integer or date fields whose rendered control would otherwise expand to fill available space on wide screens.

---

## Feature List

### Req 4.1.0 — Field-level `max-width` attribute (full-stack)

#### JSON Schema
- A new optional field property `"max-width"` has been added to `ScreenConfigSchema.json`.
- Accepted value pattern: a positive integer followed by `px` or `%` (e.g. `"120px"`, `"50%"`).
- The property is optional; omitting it means no max-width constraint is applied.

#### C# Backend
- `IScreenFieldDefinition` — new `string MaxWidth { get; }` property.
- `CScreenFieldDefinition` — new constructor parameter `string strMaxWidth = ""` (optional, after `strWidth`); `MaxWidth` property.
- `IScreenRenderFieldModel` — new `string MaxWidth { get; }` property (between `Width` and `ControlType`).
- `CScreenRenderFieldModel` — new constructor parameter `string strMaxWidth` (after `strWidth`); `MaxWidth` property.
- `CScreenConfigProvider.CScreenFieldDto` — new `[JsonPropertyName("max-width")] string? MaxWidth` DTO property.
- `CScreenConfigProvider` — passes `objField.MaxWidth ?? string.Empty` to `CScreenFieldDefinition`.
- `CScreenRenderModelFactory` — passes `itfFieldDefinition.MaxWidth` to `CScreenRenderFieldModel`.

#### Angular Frontend
- `ScreenRenderFieldModel` model — new `maxWidth: string` field.
- `SectionRendererComponent` template — `[style.max-width]="objField.maxWidth"` applied to every field control (`<input>`, `<textarea>`, `<select>`, lookup wrapper `<div>`) across all three rendering branches: tabular-edit, record-detail, and per-line/flow.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Per-field `max-width` in JSON | Prevent controls from stretching too wide on large screens |
| Works for all field types | Consistent constraint mechanism regardless of control type |
| No required change | Fields without `max-width` are unaffected |
