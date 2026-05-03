# Release Notes — BRD-03 Req 4.2: All-Screen Improvements

## Overview
This release applies polish and accessibility improvements that affect every screen, regardless of layout policy. The "Rendered screen" header label and config file name are removed from the always-visible header. Field description text is no longer shown as static space-consuming text; it is available via an icon tooltip. All tooltips meet accessibility and contrast requirements.

---

## Feature / Fix List

### Req 4.2.1 — "Rendered screen" label removed; About icon button added
- The `<p class="panel-kicker">Rendered screen</p>` heading and the config filename sidebar (`<div class="screen-meta">`) have been removed from the screen panel header.
- The screen panel header now shows only the display name (`<h2>`).
- A new `ℹ` About icon button has been added to the screen-view-actions toolbar (same line as **Back to Home** and **Reload screen**).
- Clicking the About button toggles a compact dark-background panel that shows:
  - **"Rendered screen"** as a label
  - **Config:** `{screenFileName}`
- The panel uses `role="tooltip"` and `aria-label="Screen information"` for accessibility.
- The `about-icon-btn` is styled as a circular `36 × 36 px` button matching the existing secondary button palette.
- **Benefit**: The header area is cleaner and uncluttered. Users who need the config file name can access it on demand without it being shown permanently.

---

### Req 4.2.2 — Field descriptions shown as tooltip on info icon; no static space consumed
- In **all layout policies** (per-line, record-detail, and the tabular edit screen), the `<small class="field-description">` element that previously faded in on hover is replaced by a small `ℹ` icon button.
- The icon is only rendered when `objField.description` is non-empty.
- The full description text is placed in the `title` attribute of the icon button, making it available as a native browser tooltip on hover or keyboard focus.
- The icon uses `tabindex="-1"` so it does not interrupt keyboard navigation for form fields.
- CSS: `.field-info-btn` — `background: none; border: none; font-size: 12px; border-radius: 50%; cursor: default;` with a subtle hover background.
- **Benefit**: Field descriptions are still discoverable without consuming vertical space, keeping the layout compact and consistent across all rows.

---

### Req 4.2.3 — Tooltip accessibility and contrast
- The About panel (`about-panel`) uses:
  - Dark background `#2e261f` with white text `#fff8f2` — WCAG AA contrast compliant.
  - `role="tooltip"` and `aria-label="Screen information"`.
  - `pointer-events: none` so it does not block interaction with elements behind it.
  - `z-index: 10` to ensure it appears above all other content.
  - `box-shadow` for visual separation.
- The `.field-info-btn` tooltip relies on the native browser `title` tooltip, which is inherently accessible and inherits system contrast settings.
- **Benefit**: All tooltip surfaces meet accessibility standards without requiring an external tooltip library.

---

## Summary of User-Facing Benefits

| Req | Change | Benefit |
|-----|--------|---------|
| 4.2.1 | "Rendered screen" removed from header; About ℹ button added | Cleaner header; info on demand |
| 4.2.2 | Field descriptions → ℹ icon tooltip | No static space consumed; descriptions still discoverable |
| 4.2.3 | Tooltip accessibility + contrast | Meets WCAG AA; no external library required |
