# Figma Alignment Review

This repository is a Blazor WebAssembly prototype that already contains the clinically useful functionality for bed management, ward operations, allocation and launchpad workflows. The Figma-coded prototype should therefore be treated as the visual source of truth rather than a replacement for the current C# implementation.

## Merge approach

1. Preserve current Blazor routes, demo data and interactive behaviours.
2. Align the shared shell first because every page inherits the header, navigation, facility context and bottom utility bar.
3. Convert placeholder hub entry points into navigable Blazor pages that link to the existing functional workspaces.
4. Continue page-by-page visual hardening after this baseline, using the reference screenshots in `assets/reference/` as the acceptance target.

## Baseline alignment completed

- Updated the QH header to match the signed-off Figma chrome: compact header height, Queensland Government branding, global search, notifications and clinician profile.
- Reconciled the Bed & Ward secondary navigation labels with the Figma language while keeping existing routes and components.
- Added the Gold Coast University Hospital pressure context strip used by the orchestration screen.
- Replaced the empty Bed & Ward hub placeholder with a functional DD Hub landing page that routes into Bed Management, Ward Operations and Allocation Centre.
- Re-skinned the launchpad and new hub with the same white-card, blue-accent product language used in the Figma reference assets.

## Next recommended page-by-page passes

1. Bed Management orchestration table and right-hand scenario panel.
2. Ward Operations list, map and allocation panel states.
3. Allocation Centre queue cards and destination panel.
4. Dark-mode parity once the light-mode clinical sign-off pass is complete.
