# SCSS Design System

This folder contains the project's SCSS design system and tokens. A pre-existing architecture is present.

Structure overview:

- `_variables.scss` — color, spacing, typography tokens and maps (primary source of truth).
- `_variables-dark.scss` — dark-theme overrides.
- `app.scss` — project entrypoint that imports partials and builds app stylesheet.
- `components/`, `pages/`, `plugins/`, `structure/` — grouped partials for components and page-level styles.

Next steps performed by this branch/work:

- Documented basic design tokens in `design-tokens.md`.
- If you want to centralize tokens for cross-platform usage (e.g., JS or a style dictionary), we can extract selected values from `_variables.scss` into a JSON/YAML tokens file.

How to compile (existing project):

The project already includes a `gulpfile.js` and `package.json` — use the existing build tasks or we can add a dedicated Gulp pipeline for the new design-system files.
