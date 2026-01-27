# Design Tokens (initial)

This document records the key design tokens and where they are defined.

Colors (defined in `_variables.scss`):
- Primary / theme colors: `$primary`, `$secondary`, `$success`, `$info`, `$warning`, `$danger`.
- Grays: `$gray-100` .. `$gray-900`.

Typography:
- `$font-family-base`
- `$font-size-base`
- `$line-height-base`

Spacing:
- Scale variables: `$space-1` .. `$space-5` (multiples of 4px)

Usage guidance:
- Prefer using defined variables instead of hard-coded values.
- For layout utilities, create small helper classes in `_utilities.scss` and keep them atomic.

Next actions to fully define tokens:
1. Extract and publish a minimal `tokens.json` for consumption by JS or external tools (optional).
2. Add explicit token names for interactive states (hover, focus) and motion timings.
3. Add accessibility contrast notes for each color token.
