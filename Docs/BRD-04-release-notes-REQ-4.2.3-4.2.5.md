# Release Notes — BRD-04 Req 4.2.3, 4.2.4, 4.2.5: Mandatory Schema Attributes

## Overview
Screen configuration files now enforce mandatory attributes at three levels — screen root, section, and field — via both the JSON schema and the C# parsing layer. Missing required attributes are rejected with a clear error rather than silently defaulted.

---

## Feature List

### Req 4.2.3 — Screen `id` and `name` are mandatory

#### JSON Schema (`ScreenConfigSchema.json`)
- Root-level `"required"` array updated from `["sections", "features"]` to `["id", "name", "sections", "features"]`.
- `id` and `name` property descriptions updated to reflect mandatory status (removed "optional / defaults to file name" wording).

#### C# Backend (`CScreenConfigProvider`)
- `CScreenDocumentDto.Id` — changed from `string?` to `string`; added `[JsonRequired]`. Deserialization throws `JsonException` if `id` is absent in the JSON file.
- `CScreenDocumentDto.Name` — changed from `string?` to `string`; added `[JsonRequired]`. Same enforcement as above.
- `TryLoadScreenDefinition` — removed filename-derived fallback logic for `id` and `name`; values are now taken directly from the DTO (trimmed).

---

### Req 4.2.4 — Section `name`, `layout-policy`, and `is-collapsible` are mandatory; fields array must have at least one entry

#### JSON Schema (`ScreenConfigSchema.json`)
- Section `"required"` array updated from `["name"]` to `["name", "layout-policy", "is-collapsible"]`.
- `layout-policy` and `is-collapsible` property descriptions updated to reflect mandatory status (removed `"default"` keywords).
- `fields` array in section: added `"minItems": 1` and a description clarifying that when present it must contain at least one field definition.

#### C# Backend (`CScreenConfigProvider`)
- `CScreenSectionDto.Name` — changed from `string?` to `string`; added `[JsonRequired]`.
- `CScreenSectionDto.LayoutPolicy` — changed from `string?` to `string`; added `[JsonRequired]`.
- `CScreenSectionDto.IsCollapsible` — changed from `bool?` to `bool`; added `[JsonRequired]`.
- `TryCreateSectionDefinition`:
  - Removed conditional fallbacks for `name` and `layout-policy`; values read directly from DTO.
  - `IsCollapsible` is now a non-nullable `bool`; `?? true` default removed.
  - Added early-exit guard: when `objSection.Fields` is not null but is empty, returns an error — `"Section '{name}' has a fields array but it is empty. At least one field is required when the fields array is present."`.

---

### Req 4.2.5 — Field `id`, `name`, `description`, `type`, and `width` are mandatory

#### JSON Schema
- Already correct from prior work: `"required": ["id", "name", "description", "type", "width"]` on the `field` definition.

#### C# Backend (`CScreenConfigProvider`)
- `CScreenFieldDto.Description` — changed from `string?` to `string`; added `[JsonRequired]`.
- `CScreenFieldDto.Width` — changed from `string?` to `string`; added `[JsonRequired]`.
- `TryCreateFieldDefinition`:
  - Validation guard extended to also check `description` and `width` for blank/empty values: `"Each field must define id, name, description, and width values."`.
  - `CScreenFieldDefinition` constructor call: `Description` and `Width` passed directly (no `?? string.Empty` or `"300px"` fallback needed).

---

## Summary of User-Facing Benefits

| Req | Change | Benefit |
|-----|--------|---------|
| 4.2.3 | `id` and `name` mandatory at screen root | Screen files with missing identifiers are rejected early with a clear error |
| 4.2.4 | `layout-policy` and `is-collapsible` mandatory per section | Sections are always fully specified; no hidden defaults that could cause rendering surprises |
| 4.2.4 | `fields` array requires at least 1 item | Sections that declare a fields array cannot be left empty |
| 4.2.5 | `description` and `width` enforced in C# layer | All five field attributes are uniformly validated both by schema and at runtime |
