# Maliev.Web

![CI - Develop](https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/workflows/ci-develop.yml/badge.svg)
![CI - Staging](https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/workflows/ci-staging.yml/badge.svg)
![CI - Main](https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/workflows/ci-main.yml/badge.svg)

The customer-facing MALIEV website for manufacturing services, instant quotations, and MALIEV's owned storefront.

## Architecture

`Maliev.Web` is an interactive server-side Blazor customer website with a Web BFF.

| Project | Purpose |
| --- | --- |
| `Maliev.Web.Bff` | Hosts the interactive server-side Blazor shell, public Web API endpoints, localization middleware, health endpoints, and downstream service clients. |
| `Maliev.Web.Client` | Razor component library for customer UI: landing pages, quote-engine handoff, shop, cart, account links, and localization preference handling. |
| `Maliev.Web.Shared` | DTO contracts shared by the Web BFF and Blazor client. |
| `Maliev.Web.Tests` | Unit and component-level contract tests for quote, catalog, localization, and BFF endpoints. |

## API Endpoints

| Endpoint | Purpose | Auth |
| --- | --- | --- |
| `GET /web/v1/catalog/collections` | Customer-visible shop collections. | Public |
| `GET /web/v1/catalog/products` | Product listing, optionally filtered by collection. | Public |
| `GET /web/v1/catalog/products/{handle}` | Product detail by canonical storefront handle. | Public |
| `GET /web/v1/quote/reference-data` | Customer-visible quote processes, materials, and lead times. | Public |
| `POST /web/v1/quote/estimate` | Quote estimate contract; production pricing must delegate to PricingService. | Public draft |
| `POST /web/v1/quote/uploads/resumable` | Web upload initiation contract matching the ProjectNew resumable flow. | Public draft |
| `GET /web/v1/quote/uploads/{uploadId}/analysis-status` | Watchdog-friendly analysis status. | Public draft |
| `POST /web/v1/checkout/draft` | Cart checkout draft before account sign-in and payment. | Public draft |
| `POST /web/v1/preferences/culture` | Culture/currency preference normalization. | Public |

## Permissions Model

Public marketing, catalog, and anonymous draft quote endpoints are intentionally unauthenticated. Account, saved quotes, checkout finalization, orders, payments, customer file ownership, and persisted preferences must use customer identity through the BFF and downstream MALIEV service permissions before they become write operations.

## Storefront Catalog Source

`/shop` and the public `GET /web/v1/catalog/*` endpoints are backed by `Maliev.CommerceService`, not local static products. The BFF maps CommerceService published storefront endpoints into `Maliev.Web.Shared.Commerce` DTOs:

| Web BFF endpoint | CommerceService endpoint |
| --- | --- |
| `GET /web/v1/catalog/collections` | `GET /commerce/v1/collections` |
| `GET /web/v1/catalog/products?collection={handle}` | `GET /commerce/v1/products?page=1&pageSize=100&collection={handle}` |
| `GET /web/v1/catalog/products/{handle}` | `GET /commerce/v1/products/{handle}` |

CommerceService draft products are intentionally hidden from the customer shop until published.

## Development

```powershell
dotnet restore Maliev.Web.slnx
dotnet build Maliev.Web.slnx --configuration Release
dotnet test Maliev.Web.slnx --configuration Release
dotnet run --project Maliev.Web.Bff/Maliev.Web.Bff.csproj
```

The local site runs from the BFF launch profile at `https://localhost:7236` or `http://localhost:5026`.

### Google Sign-In

`Maliev.Web.Bff` renders Google's official Google Identity Services (GIS) button. The browser receives a one-time AuthService nonce, Google places that nonce in the ID token, and WebBff forwards the raw credential and nonce to AuthService for server-side verification. Web does not need or use a Google client secret for sign-in. For direct local Web runs, store the public Web client ID in the BFF user-secrets store:

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "<google-oauth-client-id>" --project Maliev.Web.Bff/Maliev.Web.Bff.csproj
```

When the site runs under Aspire, store the client in `Maliev.Aspire.AppHost` user-secrets or in the git-ignored `B:\maliev\Maliev.Aspire\Maliev.Aspire.AppHost\sharedsecrets.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "<google-oauth-client-id>"
    }
  }
}
```

Direct Web user-secrets still take priority over the shared Aspire secrets file; `sharedsecrets.json` is only a Development fallback for missing values.

The Google OAuth client must allow these JavaScript origins:

```text
http://localhost:5026
https://localhost:7236
https://www.maliev.com
```

When Web runs under Aspire, `Maliev.Aspire.AppHost` injects the public client ID as `Authentication__Google__ClientId`. Google Drive and Google Maps keep their separate authorization and API-key settings; this sign-in flow does not reuse or remove those credentials.

## Google Ads Landing Routes

The home page rotates Google Ads hero copy from `Maliev.Web.Client/Content/HeroCopyCatalog.cs`. Use `service` as the stable targeting query parameter for ad final URLs. The resolver also accepts `target`, `keyword`, `utm_term`, `utm_content`, and `utm_campaign` so existing campaign tracking can still match hero copy by keyword. The same resolved target promotes the matching service card in the home services grid, keeping the hero and first highlighted capability aligned.

Canonical pattern:

```text
https://www.maliev.com/?service=<ad-target>&utm_campaign=<campaign>&utm_term={keyword}
```

If no target is supplied, or the target is unknown, the page falls back to `3d-printing`. Each target has 50 localized English and Thai hero variants.

| Ad target | Suggested final URL | Campaign intent |
| --- | --- | --- |
| `3d-printing` | `https://www.maliev.com/?service=3d-printing` | General 3D printing, prototype parts, additive manufacturing |
| `fdm-3d-printing` | `https://www.maliev.com/?service=fdm-3d-printing` | FDM, PLA, ABS, PETG, nylon, functional printed parts |
| `resin-3d-printing` | `https://www.maliev.com/?service=resin-3d-printing` | Resin, SLA, DLP, high-detail cosmetic prototypes |
| `cnc-machining` | `https://www.maliev.com/?service=cnc-machining` | General CNC machining, milling, turning, machined prototypes |
| `aluminum-cnc-milling` | `https://www.maliev.com/?service=aluminum-cnc-milling` | Aluminum CNC milling, machined aluminum parts, fixtures |
| `3d-scanning` | `https://www.maliev.com/?service=3d-scanning` | 3D scanning, reverse engineering, inspection-ready scan data |
| `3d-design` | `https://www.maliev.com/?service=3d-design` | 3D CAD design, DFM, product modeling, manufacturable design |
| `silicone-casting` | `https://www.maliev.com/?service=silicone-casting` | Silicone casting, urethane casting, low-volume molded parts |
| `rapid-prototyping` | `https://www.maliev.com/?service=rapid-prototyping` | Rapid prototyping, quick-turn engineering samples |
| `deviation-analysis` | `https://www.maliev.com/?service=deviation-analysis` | Deviation analysis, scan-to-CAD comparison, dimensional reports |

When adding a new ad target, update `HeroCopyCatalog`, `Maliev.Web.Tests/HeroCopyCatalogTests.cs`, this README table, and `AGENTS.md` together.

## Public Policy Routes

Policy pages are rendered by `Maliev.Web.Client/Pages/StaticPage.razor` and listed in the generated sitemap from `Maliev.Web.Bff/Controllers/SeoController.cs`.

| Route | Purpose |
| --- | --- |
| `/terms` | Terms of service for website use, product orders, quote acceptance, CAD uploads, payments, delivery, and liability. |
| `/privacy` | Privacy policy for personal data, CAD files, orders, support records, sharing, retention, and data subject rights. |
| `/cookie-policy` | Cookie policy for essential, preference, analytics, security, and advertising cookies. |
| `/shipping-returns` | Shipping, delivery inspection, return eligibility, and delivery issue handling. |
| `/refund-policy` | Refund, cancellation, credit, rework, and replacement eligibility. |
| `/warranty-policy` | Warranty coverage, workmanship review, exclusions, support process, and available remedies. |
