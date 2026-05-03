# Release Notes — BRD-03 Req 3.2: Record-Detail Layout Policy

## Overview
A new `record-detail` layout policy is now available. It renders a single record in a per-line, form-style layout with built-in edit controls. Screens declare `"layout-policy": "record-detail"` in their configuration to opt in. This policy is the natural companion to the tabular policy: the tabular view links to a record-detail screen when a user clicks a row or adds a new entry.

---

## Feature List

### Req 3.2 — Record-detail layout policy registration
- The backend exposes a `record-detail` policy ID through the `ILayoutPolicy`/`ILayoutPolicyRegistry` system.
- The Angular client maps `record-detail` to the `layout-record-detail` CSS class.
- **Benefit**: Any screen definition can render a single-record edit form without any extra front-end code.

---

### Req 3.2.1 — Per-line field rendering (similar to per-line policy)
- Each field is displayed on its own row with the label on the left and the editable control on the right.
- The form responds to viewport changes: label width adjusts at tablet and mobile breakpoints.
- All control types (text input, textarea, select dropdown) are supported.
- **Benefit**: Users get a clean, focused editing experience for one record at a time, with no distracting table rows around them.

---

### Req 3.2.2 — Save and Cancel buttons
- A **Save** button commits the current field values as the new baseline for the record and emits a `save` action to the parent.
- A **Cancel** button reverts all unsaved changes back to the last-saved baseline and emits a `cancel` action.
- Both buttons are clearly styled and placed together in an action bar below the fields.
- **Benefit**: Users have an explicit, risk-free editing workflow. Accidental edits are fully reversible until Save is pressed.

---

### Req 3.2.3 — Show Original Values (side-by-side comparison)
- A **Show Original Values** toggle button appears alongside Save and Cancel.
- When active, each field row displays the saved baseline value directly beneath the editable control, styled with a warm amber indicator.
- Clicking the button again hides the original values.
- **Benefit**: Users can compare what a field currently says against what it originally said before making a final save decision, reducing data-entry errors and enabling informed change management.

---

## Summary of User-Facing Benefits

| Capability | User Benefit |
|---|---|
| Record-detail layout | Focused single-record form, free of tabular clutter |
| Per-line field rendering | Clean label + input alignment across all field types |
| Save / Cancel | Explicit, reversible edit lifecycle — no accidental data loss |
| Show Original Values | Side-by-side comparison before committing changes |
