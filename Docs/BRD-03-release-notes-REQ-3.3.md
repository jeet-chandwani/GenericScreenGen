# Release Notes — BRD-03 Req 3.3: Lookup Field Enhancements

## Overview
The `lookup` field type has been significantly refined. It now supports mandatory/optional designation, single or multiple selection, a real-time search filter, rich option display (description + image), and a tag-based multi-select UI. These improvements apply to lookup fields across all layout policies — including tabular and record-detail.

---

## Feature List

### Req 3.3 — Mandatory, optional, single and multiple selection
- **TypeInfo format extended**: Lookup fields can now include `mandatory` and `multiple` flags in their TypeInfo string alongside the option list, e.g. `{Option A; Option B; mandatory; multiple}`.
- The backend parses these flags and exposes `IsMandatory` and `IsMultiple` on the field render model.
- The Angular field model gains matching `isMandatory` and `isMultiple` properties.
- When `multiple` is set, the client automatically renders a **checkbox-list** multi-select instead of a `<select>` dropdown.
- **Benefit**: Screen designers can declare the selection behaviour directly in the screen config JSON — no code changes required. A single flag switches between single-select dropdown and multi-select checkbox UI.

---

### Req 3.3.1 — Searchable lookup with partial, case-insensitive match
- Every lookup field (both single and multi-select) now displays a **search text box** above the options.
- As the user types, the displayed options are filtered in real-time using case-insensitive partial matching.
- The search also matches against the option's description text when present.
- **Benefit**: In lookup fields with long option lists, users can instantly narrow down choices without scrolling through every option.

---

### Req 3.3.2 — Description and image per option
- **TypeInfo format**: Options now support an extended format `value::description::imageUrl`. Description and image URL are optional; plain `value` items continue to work unchanged.
- In multi-select checkbox lists, each option row shows:
  - A thumbnail image (24 × 24 px) when an image URL is provided.
  - The option value followed by ` — description` when a description is provided.
- In single-select dropdowns, the `<option>` title attribute and visible text include the description where available.
- **Benefit**: Users get richer context when choosing between options (e.g., product images, status descriptions), reducing selection errors without needing to open external references.

---

### Req 3.3.3 — Multi-select with removable tags
- When `isMultiple` is true, selected values are stored pipe-separated (`value1|value2`) in the field's data slot.
- Below the checkbox list, **selected values are shown as coloured tags**. Each tag has an ×-remove button.
- Clicking a tag's × deselects the value both from the tag list and from the checkbox.
- The tag display works in both the tabular row editor and the record-detail form.
- **Benefit**: Users can see all currently selected values at a glance and remove individual selections without scrolling back through the option list, providing a clear and efficient multi-selection experience.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Mandatory / optional flag | Communicates input requirements directly in the UI |
| Single vs multiple selection | One config flag switches the entire select UX |
| Real-time search filter | Find options instantly in long lists |
| Description + image per option | Richer context for informed selection decisions |
| Tag-based multi-select | Clear overview of chosen values; easy individual removal |
