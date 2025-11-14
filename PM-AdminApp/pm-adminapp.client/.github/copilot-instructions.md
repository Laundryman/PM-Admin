# Copilot project instructions

Purpose: Make AI coding agents immediately productive in this repo by capturing the essential architecture, workflows, and project-specific conventions.

## Big picture
- Stack: Vue 3 + Vite + PrimeVue (Sakai template) + Tailwind CSS, now TypeScript-first, deployed as an SPA.
- Entry: `src/main.ts` mounts `App.vue`, installs Vue Router, PrimeVue with the Aura theme from `@primeuix/themes`, and PrimeVue services (Toast, Confirmation).
- Layout & routing:
  - Router (`src/router/index.js`) uses `createWebHistory()` and wraps most routes in `src/layout/AppLayout.vue` which provides the shell (topbar, sidebar, menu). A d.ts shim (`src/router/index.d.ts`) exposes proper `Router` typing until the router is fully migrated to TS.
  - Pages live under `src/views/**` with feature folders like `uikit/` and `pages/`.
- Theming & dark mode:
  - Tailwind dark mode is driven by the CSS selector `[class*="app-dark"]` (see `tailwind.config.js`).
  - PrimeVue theme is configured with `darkModeSelector: '.app-dark'` in `src/main.js`.
  - Dark mode toggling is centralized in `src/layout/composables/layout.js` by adding/removing the `app-dark` class on `document.documentElement`.

## Dev and build workflows
- Scripts (see `package.json`):
  - `npm run dev` → Vite dev server (uses `vite.config.js`).
  - `npm run build` → production build.
  - `npm run preview` → locally preview a production build.
  - `npm run lint` → eslint with Vue + Prettier config.
  - `npm run type-check` → Vue TSC type-check across `.ts` and `.vue` SFCs (see `tsconfig.json`).
- Vite configuration:
  - Active config: `vite.config.js` (JS). It:
    - Sets alias `@` → `./src`.
    - Generates/uses a local HTTPS cert via `dotnet dev-certs` and starts the dev server on `https` with a fixed port from `DEV_SERVER_PORT` (defaults to 53681).
    - Proxies `^/weatherforecast` to an ASP.NET backend discovered from `ASPNETCORE_HTTPS_PORT` or `ASPNETCORE_URLS` (fallback `https://localhost:7134`).
  - `vite.config.mjs` is a variant focused on PrimeVue auto-imports; prefer `vite.config.js` for this app's dev flow.
- SPA hosting:
  - `vercel.json` rewrites all paths to `/index.html`. Keep this in sync if adding server-side routes.

## TypeScript notes
- Config: `tsconfig.json` sets alias `@/*` → `src/*`, enables strict mode, and includes Vite + Node types.
- Shims: `src/env.d.ts` declares `*.vue` modules and Vite client types; `src/router/index.d.ts` provides a typed `Router` export while the router remains JS.
- Interop: JS files in `src/service/**` are still valid and can be incrementally migrated; prefer adding `.d.ts` shims or JSDoc types during transition.

## Project conventions & patterns
- Imports use `@` for anything under `src`.
- Components are colocated by feature:
  - Global layout components in `src/layout/`.
  - Feature pages in `src/views/` (e.g., `uikit/*`, `pages/*`).
  - Demo/data services in `src/service/*Service.js` return in-memory arrays for UI examples. Swap these for real API calls as needed.
- State and UI shell behavior is handled via the `useLayout()` composable (`src/layout/composables/layout.js`) exposing:
  - `layoutConfig` (preset, primary color, darkTheme, menu mode),
  - `layoutState` (sidebar/menu visibility and active item), and helpers like `toggleMenu()` and `toggleDarkMode()`.
- Styling:
  - Tailwind is enabled with the `tailwindcss-primeui` plugin (`tailwind.config.js`). Content scanning includes `./src/**/*.{vue,js,ts,jsx,tsx}`.
  - Global styles imported via `src/assets/styles.scss`.

## When adding features
- Routes: Add lazy-loaded routes in `src/router/index.js` under the `AppLayout` parent when they should appear in the main shell.
- Theming: Use classes consistent with Tailwind + PrimeVue themes; for dark-aware styles rely on parent `app-dark` class.
- Services: Follow the pattern in `src/service/*Service.js`. For real APIs, prefer using fetch/axios modules colocated under `src/service/` and keep the same method-first export style (e.g., `export const FooService = { list(), get(), ... }`).
 - Services: Now TypeScript-first under `src/service/*.ts` with interfaces in `src/types/models.ts`. Each service returns typed arrays (e.g., `Country[]`, `Product[]`, `Customer[]`). Keep demo data local; replace implementations with API calls returning the same shapes.
- Components: Import from `@/` and keep component folders under `src/components` or `src/views/<feature>`.

## Examples from this repo
- Dark mode toggle: `src/layout/composables/layout.js` toggles `document.documentElement.classList` with `app-dark` and also flips `layoutConfig.darkTheme` to align PrimeVue and Tailwind.
- Proxy to backend during dev: requests to `/weatherforecast` are forwarded to the ASP.NET app; add new backend routes by extending `server.proxy` in `vite.config.js`.

## Gotchas
- Two Vite configs exist; `vite.config.js` is the source of truth for local dev and HTTPS/proxy. Don’t accidentally run with the `.mjs` unless you intend to.
- If history routing 404s in non-Vercel hosting, configure SPA rewrites similar to `vercel.json`.
- Dark mode relies on the `app-dark` class. Don’t introduce a different selector or it will desync Tailwind and PrimeVue.
- Index entry in `index.html` now points at `/src/main.ts`; if you add a parallel JS entry, ensure the `<script type="module">` points to the TS one.
