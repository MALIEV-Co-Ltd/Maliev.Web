# Make Studio Landing Redesign — Design

- **Date:** 2026-06-20
- **Owner:** Natthapol Vanasrivilai
- **Status:** Draft for review
- **Component:** `Maliev.Web` (Blazor WASM `Maliev.Web.Client` + BFF `Maliev.Web.Bff`)

## 1. Goal

Redesign the public landing page (`Home.razor`) end-to-end around **Make Studio** — MALIEV's
agentic manufacturing workspace where a user can ask, sketch, upload, get DFM feedback, get a
quote, and order, all in one conversation. The hero stops showing a passive 3D model and instead
*shows the product working*: a self-running Make Studio window that types a prompt, attaches a
sketch / STEP file / photo, and returns DFM findings, instant pricing, and a reconstructed 3D
model.

The visual direction is already settled by the working prototype at
`.superpowers/brainstorm/landing-hero-1-workbench.html` ("Sample 1 — Make Studio multi-input
animated"). This spec adapts that prototype into the real Blazor + design-system + bilingual app
and extends the Make Studio narrative across the whole page.

## 2. Locked decisions

| Decision | Choice |
| --- | --- |
| Scope | **Full page redesign** of `Home.razor` (all sections) |
| Delivery | **All at once** — one spec, one plan, one coherent drop |
| Make Studio CTA | **Enable site-wide** — flip `QuoteEntryDisabled`, wire `MakeStudioUrl`, restore live entry points everywhere |
| Demo content | **Bilingual EN/TH** via existing `@Text(en, th)` / `.For(Preferences.Culture)` |
| Hero headline | **Keep** the audience-targeted randomized `HeroCopyCatalog` variant system |
| Narrative | **Approach A — "the workspace is the story"**: every section reframed around the Make Studio agentic loop |
| Machine configurator | **Keep, move lower** — relocate below the Make Studio narrative, restyle |

Reversal of note: `PublicQuoteEntryPointsAreDisabledForMvp` and `SiteContent.QuoteEntryDisabled`
were *deliberately* set to disable public quote intake "while we prepare the MVP quote workflow."
The user has confirmed `quote.maliev.com` is public-ready and we are doing a **site-wide launch**.

## 3. Hero design (centerpiece)

### 3.1 Layout

Two-column hero (existing `.landing-hero` grid is retained and re-tuned):

- **Left column (logic unchanged):** keep the dynamic `_heroCopy.HeadlineLead` / `HeadlineAccent`
  / `Body.For(Preferences.Culture)`, the `HeroCopyCatalog` audience targeting + persisted variant
  selection, and the `metric-strip` (count-up metrics). The prototype's fixed H1 ("Describe it,
  sketch it, or upload it…") is **illustrative only** and must NOT be hardcoded — the catalog stays
  the source of the headline. Below the copy: real CTA buttons (primary **Start in Make Studio**
  → live `MakeStudioUrl`; secondary **Talk to MALIEV** → `/contact`). The old static
  `make-studio-cta` description card is removed from the hero (its job is now done by the live
  animation on the right).
- **Right column:** remove `ManufacturingGizmo` (component stays; it is still used by
  `Quote.razor` and `ServicePage.razor`). Replace with the animated **Make Studio window**.

### 3.2 Make Studio window anatomy

Faithful to the prototype:

- **Top bar:** status dot, project name (updates per scene: "Aluminum L-bracket", "Nylon SLS
  enclosure", …), an "Artifacts • N" pill.
- **Thread:** user message bubbles (right, blue) with attachment tiles; agent responses (left)
  with result cards.
- **Composer (bottom):** `+` attach affordance, a text input that **types** the scene prompt
  character-by-character with a blinking caret, a copy affordance, and a send button.
- **Quick-action chips:** `Upload a 3D file`, `Photo of a part`, `Hand sketch` — the relevant chip
  activates per scene.

### 3.3 Scenes (4, auto-cycling, all bilingual)

Each scene = (composer typing text, active quick-action, project name, user bubble, agent card).
Content mirrors the prototype; all strings localized EN/TH.

1. **Hand sketch → DFM blocker.** Attaches `bracket_sketch.png`. Prompt: "Quote this bracket — 50
   pcs, 6061 aluminum." Agent: OCR'd dimensions table (read/assumed source chips) + a DFM
   **blocker** ("hole too close to bend").
2. **3D file → instant quote.** Attaches `enclosure.step`. Prompt: "25 of these in black nylon
   SLS, 7-day lead." Agent: instant price, quantity breaks, line items.
3. **Photo → reconstructed 3D model.** Attaches `part_photo.jpg`. Prompt: "Make 10 like this in
   aluminum." Agent: draft reconstructed 3D model preview with a dimension callout.
4. **Plain text → quote.** No attachment. Prompt: "5 nylon brackets for a test rig by Friday,
   ~80×40mm." Agent: material reasoning + quote with quantity breaks.

Attachment thumbnails are **inline SVG** (sketch, STEP isometric, photo, reconstructed model) —
copied from the prototype, theme-aware via CSS variables.

### 3.4 Animation architecture

- **Content in Blazor, motion in JS.** All scene markup (bubbles, cards, tables, SVGs) is rendered
  by `Home.razor` using `@Text(en, th)` so localization lives in one place. A new self-initializing
  JS module drives **only the timeline** (typing, message reveal, thread scroll, chip activation,
  project-name swap, scene cycling).
- **New module:** `Maliev.Web.Bff/wwwroot/js/maliev-make-studio-hero.js`, registered in
  `Maliev.Web.Bff/Components/App.razor` alongside `js/maliev-countup.js`. It follows the
  `maliev-countup.js` pattern: scans for a `[data-make-studio-hero]` root, self-starts, no Blazor
  lifecycle call required (works with WASM prerender).
- **Localized typing strings** are read from the DOM via `data-ms-composer` (and any per-scene
  data attributes) that Blazor renders with `@Text(...)` — JS never holds copy.
- **Trigger:** `IntersectionObserver` starts the loop when the hero scrolls into view; pauses when
  offscreen (battery/perf).
- **Reduced motion:** `@media (prefers-reduced-motion: reduce)` → no loop; render the first scene
  fully revealed as a static state.
- **Culture change:** the hero window container is `@key`'d to `Preferences.Culture` so it remounts
  and the module re-scans when the user switches EN/TH.
- **Accessibility:** the animated window is `aria-hidden="true"` decorative. The value proposition
  is fully conveyed by the real left-column copy + CTAs, which are the accessible/crawlable source
  of truth.
- **Theming:** light + dark via existing `html[data-theme="dark"]` tokens (`--blue #0a72ef`,
  `--ink`, `--muted`, `--rule`, `--paper`, `--paper-2`, `--font-mono`). The prototype's ad-hoc vars
  map ~1:1 and are replaced with the real tokens.

## 4. Page narrative (Approach A — "the workspace is the story")

| # | Section (current) | New treatment |
| --- | --- | --- |
| 1 | Hero (3D gizmo) | **Animated Make Studio hero** (§3) |
| 2 | Industry sector band | **"Who builds with Make Studio"** — same 4 sectors (Aerospace / Automotive / Consumer electronics / Medical), restyled for cohesion |
| 3 | Workflow carousel "Make Studio to made part" | **"From prompt to part"** — the agentic loop (ask → design → DFM → quote → manufacture → inspect/ship), restyled |
| 4 | Home services tabs | **"What MALIEV makes"** — the production muscle behind the agent (services), restyled |
| 5 | Machine configurator | **Kept, moved lower**, framed as a distinct product line ("shop-floor machines"), restyled |
| 6 | Case studies "What we've made lately" | Kept, restyled |
| 7 | Blog "Practical notes" | Kept, restyled |
| 8 | Final CTA "Bring the idea to Make Studio" | Kept; **live** Make Studio link |

Section ordering after redesign: hero → industries → workflow → services → machine → case studies →
blog → final CTA. (Workflow rises above services so the agentic loop is explained before the
service catalog; this differs from the current order and the ordering assertions in
`HomeUsesFinishedManufacturingAndNewsSections` will be updated to match.)

Restyle = visual cohesion + Make Studio copy reframing; the underlying data sources
(`SiteContent`, `HeroCopyCatalog`, `HeroModelCatalog`, service/case/blog catalogs) and the
machine configurator interactivity are preserved.

## 5. CTA site-wide enablement

- `SiteContent.QuoteEntryDisabled = false`.
- Make Studio CTAs link to `SiteContent.MakeStudioUrl` (`= QuoteEngineUrl` = `https://quote.maliev.com`).
- Restore enabled states for the entry points the MVP gate disabled: `MainLayout` nav (desktop +
  mobile), `Home` hero + final CTA, `Quote.razor`, `Services.razor`, `ServicePage.razor`,
  `Shop.razor`, `Error.razor`, `ProductDetail.razor`, `StaticPage.razor`, and the
  `QuoteDropzone` `Disabled` parameter usages.
- The signed upload→handoff pipeline (`maliev-quote-dropzone.js`, `/web/v1/quote/uploads/*`,
  culture-preserving redirect) already exists and is re-enabled, not rebuilt.
- Keep `QuoteDisabled*` copy/members in `SiteContent` available for any future re-disable, but no
  longer applied on these surfaces.

## 6. Localization

- Hero left-column copy: already bilingual via `HeroCopyCatalog`.
- Demo scene content (prompts, card labels, table headers, DFM text, quantity breaks): bilingual
  via `@Text(en, th)`.
- New section headings / reframed copy: bilingual.
- Thai font switching already handled by `--maliev-font-sans` (`html:lang(th)`).

## 7. Accessibility & motion

- Animated window `aria-hidden`, non-interactive, decorative.
- `prefers-reduced-motion: reduce` → static first scene.
- CTAs and all real content keyboard-focusable and crawlable.
- Color contrast verified in light and dark for new surfaces.

## 8. Files

**Add**
- `Maliev.Web.Bff/wwwroot/js/maliev-make-studio-hero.js` — timeline module.

**Modify**
- `Maliev.Web.Client/Pages/Home.razor` — hero markup + scene markup + section restructure + CTA wiring.
- `Maliev.Web.Bff/wwwroot/app.css` — new hero/window styles, restyled sections, dark mode; remove dead hero rules.
- `Maliev.Web.Bff/Components/App.razor` — register the new JS module.
- `Maliev.Web.Client/Content/SiteContent.cs` — `QuoteEntryDisabled = false`; any new copy.
- `MainLayout.razor`, `Quote.razor`, `Services.razor`, `ServicePage.razor`, `Shop.razor`,
  `Error.razor`, `ProductDetail.razor`, `StaticPage.razor` — restore live Make Studio entry points.
- `HeroCopyCatalog.cs` / catalogs — only if reframed copy requires it.

**Unchanged**
- `ManufacturingGizmo.razor` (still used by Quote/ServicePage).
- Upload/handoff BFF endpoints and `maliev-quote-dropzone.js`.

## 9. Test strategy

The landing page is pinned by large **source-level "change-detector" tests** that assert exact
markup/CSS strings. These will be rewritten to assert the *new* design (per full-ownership rule):

- `HeroLayoutSourceTests.cs` — rewrite hero assertions: gizmo removed from Home, new
  `data-make-studio-hero` window present, scenes carry localized `data-*`, CTAs link to
  `MakeStudioUrl`. Update/replace `HomeHeroUsesRightSideGlbLandingModel`,
  `HomeHeroUsesMakeStudioCta`, `HomeHeroMetricsUseCountUpEnhancement`,
  `HomeUsesFinishedManufacturingAndNewsSections`, and section-ordering assertions.
- `PublicQuoteEntryPointsAreDisabledForMvp` → rewrite as **`PublicQuoteEntryPointsAreLive`**:
  assert `QuoteEntryDisabled = false`, CTAs reference `MakeStudioUrl`, no `QuoteDisabledButtonClass`
  on the live surfaces.
- `WorkflowLayoutSourceTests.cs`, `NavigationScrollSourceTests.cs`,
  `CustomerFacingCopyTests.cs`, `LocalizationTests.cs`, `HeroCopyCatalogTests.cs` — update strings
  that changed; preserve still-true assertions.
- **New:** test asserting `js/maliev-make-studio-hero.js` is registered in `App.razor` and the
  module guards `prefers-reduced-motion` + uses `IntersectionObserver` (mirrors the countup test).
- Keep all non-landing tests (account, checkout, commerce, chatbot, BFF endpoints) green.

`ManufacturingGizmo` GLB-model assertions that are Home-specific move out; Service-page GLB
assertions stay.

## 10. Verification plan

Per `CLAUDE.md` mandatory self-testing:
1. `dotnet build` — **zero warnings/errors**.
2. `dotnet test` — full suite green (rewritten source tests + untouched suites).
3. **Browser preview** via `preview_*` tools — the JS timeline can't be xUnit-tested:
   - hero animation cycles through all 4 scenes (typing → reveal → cards),
   - light + dark,
   - reduced-motion static state,
   - EN + TH,
   - mobile breakpoint stacks correctly,
   - "Start in Make Studio" points at `quote.maliev.com`.
   Capture before/after screenshots.

## 11. Out of scope / risks

- Not rebuilding the QuoteEngine/Make Studio app itself or the upload pipeline.
- Not changing `ManufacturingGizmo` internals or Quote/ServicePage hero behavior.
- **Risk — test-rewrite volume:** thousands of exact-string assertions; mitigate by rewriting
  per-section alongside each markup change, not in one pass.
- **Risk — perf:** keep the animation CSS/SVG-driven and pause offscreen; no heavy libraries.
- **Risk — launch correctness:** enabling site-wide re-opens real public intake; verify the live
  link + handoff in preview before done.

## 12. Implementation milestones (within the single drop)

1. **Hero** — remove gizmo, build window markup + scenes (bilingual), JS module, CSS, CTA wiring; tests.
2. **CTA enablement** — flip flag, restore entry points site-wide; rewrite gate test.
3. **Sections** — industries, workflow, services, machine (moved lower), case studies, blog, final CTA restyle/recopy; tests.
4. **Verify** — build, full test suite, browser preview (all matrices), screenshots.
