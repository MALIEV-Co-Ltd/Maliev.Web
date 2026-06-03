# Maliev.Web UX & Integration Fixes — Design Doc

**Date:** 2026-05-27
**Scope:** 6 UX/integration issues across Maliev.Web

---

## Issue 1: Blog PDF Download — Loading State

**Problem:** The "Download PDF" button is an `<a>` tag with `download` attribute — no visual feedback when clicked.

**Solution:** Convert to `<button>` with `@onclick` handler. Add `_pdfDownloading` boolean that:
- Sets `.is-loading` CSS class (already exists at `app.css:4121`)
- Shows `.button-spinner` + "Downloading…" text
- Calls `JS.InvokeVoidAsync("malievBlog.downloadFile", href, filename)`
- Resets after 2s or on completion

**Files:** `StaticPage.razor:556-561`, `maliev-blog.js`

---

## Issue 2: Chatbot — Degraded Mode + Enter Key Focus

### Part A: ChatbotService not configured
**Root cause:** `IsChatbotServiceConfigured()` returns false because `services__ChatbotService__*` env vars aren't set. ChatbotService isn't registered in the Aspire AppHost.

**Solution:** Register ChatbotService in Aspire AppHost and wire it to the Web project via `WithReference`.

**Files:** `Maliev.Aspire.AppHost/Program.cs`

### Part B: Enter key doesn't refocus textarea
**Root cause:** `sendButton.click()` in `initComposerKeys` doesn't return focus to the textarea.

**Solution:** Add `textarea.focus()` after `sendButton.click()`.

**Files:** `maliev-chatbot.js:207`

---

## Issue 3: Sign-In Page — More Spacious

**Problem:** Current 50/50 split layout constrains the form to 480px max-width in the center of the screen.

**Solution:**
- Widen grid to `1.2fr 1.8fr`
- Increase form inner width to 640px
- Add trust badges and decorative elements to left panel
- Increase padding/gap throughout

**Files:** `AuthSignIn.razor`, `app.css`

---

## Issue 4: Account Profile

### Company search
Add a visible search icon button next to the debounced input field and a clear button.

### Language flags
Replace emoji flags with SVG images (`/images/flags/us.svg`, `/images/flags/th.svg`).

### Save timestamp
- Add "a moment ago" for <60 seconds
- Adaptive timer: tick every 10s for first 2 min, then 30s, then 60s

**Files:** `AccountProfile.razor`, `app.css`, new SVG flag files

---

## Issue 5: Account Addresses

### Dark mode Google Maps
Remove forced `colorScheme: light` from JS; add CSS custom property overrides for `[data-theme="dark"]`.

### Google → Registry enrichment
When a Google Place is selected, auto-query RegistryService with district name to fill Thai-specific sub-district/district/province fields.

### House number extraction
Improve `normalizeSelection` JS to fall back to parsing the formatted address for the house number.

### Address cards with place name
- Add `DisplayName` to `GoogleAddressSelection` DTO
- Add `GooglePlaceName` to `CustomerAddressDto`
- Display the place name on address cards

**Files:** `UnifiedAddressSearch.razor`, `AccountAddresses.razor`, `maliev-google-address-picker.js`, `GoogleAddressDtos.cs`, `AccountDtos.cs`, `app.css`

---

## Issue 6: "Get Part Price" URL

**Problem:** `QuoteNewUrl` points to `{QuoteEngineUrl}/projects/new` instead of the QuoteEngine landing page.

**Solution:**
- Change `QuoteNewUrl` to just `{QuoteEngineUrl}` (landing page)
- Add `QuoteNewProjectUrl = {QuoteEngineUrl}/projects/new` for places that need project creation (QuoteDropzone, "Start project", "Start a similar quote")
- Update 5 references to use `QuoteNewProjectUrl`

**Files:** `SiteContent.cs:25`, `Home.razor`, `ServicePage.razor`, `Quote.razor`, `CustomerChatbot.razor`, `StaticPage.razor`
