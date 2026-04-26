# Release Notes — BRD-03 Req 4.3: Full-Screen Layout, Sticky Nav, and About Placement

## Overview
This release restructures the application shell to make full use of the available window width, moves navigation controls into the always-visible sticky title bar, and relocates the About tooltip to be inline with each screen's title.

---

## Feature / Fix List

### Req 4.3.1 — Screen layout fills full window width
- Removed `max-width: 1100px` constraint from `.page-title-panel` and `.screen-view-panel`.
- The rendered screen now stretches to the full available window width, adjusting dynamically as the window is resized.
- `min-width: 360px` is retained on the page shell for very small screens.
- **Benefit**: Wider screens no longer waste space with large empty margins around the content.

---

### Req 4.3.2 — Field controls vertically aligned across rows
- Changed `.field-input` from `flex: 0 0 auto` to `flex: 1 1 auto` so all field input controls (text, date, number, select) stretch to fill the available width in their row.
- This creates a consistent visual column for controls in per-line and record-detail layouts.
- **Benefit**: Fields no longer appear at varying widths; the layout looks clean and aligned.

---

### Req 4.3.3 — Back to Home and Reload moved to sticky title bar
- The text-label **Back to Home** and **Reload screen** buttons have been removed from the `.screen-view-actions` toolbar.
- Two icon buttons are now placed inside the sticky `.page-title-panel`:
  - `¦` — Back to Home (`title="Back to Home"`)
  - `?` — Reload screen (`title="Reload screen"`)
- The buttons appear only when `viewMode === 'screen'` and are hidden on the home view.
- The entire `.screen-view-actions` div has been removed as it is now empty.
- **Benefit**: Navigation controls are always visible even when the user has scrolled down. Icon-only buttons save horizontal space.

---

### Req 4.3.4 — About button placed in screen panel header with hover tooltip
- The `?` About button has been moved from the `.screen-view-actions` toolbar into the `.panel-header`, alongside the screen `<h2>` title.
- The button uses the native `title` attribute: `"Rendered screen\nConfig: {screenFileName}"` — tooltip appears on cursor hover, no click required.
- Removed the `showAboutPanel` signal, `toggleAboutPanel()` method, and the `.about-panel` overlay from App component and CSS.
- `.panel-header` `align-items` changed from `start` to `center` so the About icon is vertically centered with the heading.
- **Benefit**: About information is always co-located with the screen title and accessible without an extra click or overlay.

---

## Summary of User-Facing Benefits

| Req | Change | Benefit |
|-----|--------|---------|
| 4.3.1 | Removed max-width cap | Full window width used for screen layout |
| 4.3.2 | Field inputs fill their row | Clean vertical column alignment across all fields |
| 4.3.3 | Nav icons in sticky title bar | Always accessible regardless of scroll position |
| 4.3.4 | About ? in panel header, hover tooltip | No click needed; co-located with screen title |
