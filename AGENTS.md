# Maliev.Web Agent Notes

## Crawling And Sitemaps

Any change that adds, removes, renames, or materially changes public marketing routes, ad landing URLs, localized pages, catalog pages, blog content, or other crawlable customer-facing content must create or update crawler files in the same commit.

Required crawlability work:

- Create or update `robots.txt` so crawlers can discover the current sitemap entry points.
- Create or update sitemap files for public site URLs, including service landing routes used by Google Ads.
- Keep sitemap URLs canonical and production-facing, using `https://www.maliev.com/` unless the deployment domain changes.
- Include localized public routes when they have distinct crawlable URLs.
- Do not include private account, checkout, cart, upload, quote draft, health, API, or internal BFF endpoints in sitemaps.
- Add or update tests/build checks when sitemap or robots generation is code-driven.

## Google Ads Landing Routes

Do not rediscover the Google Ads route list manually. The ad landing page is the home route `/` with targeting supplied by query string. Use `service` as the stable ad final URL parameter.

Canonical pattern:

```text
https://www.maliev.com/?service=<ad-target>&utm_campaign=<campaign>&utm_term={keyword}
```

The resolver also accepts `target`, `keyword`, `utm_term`, `utm_content`, and `utm_campaign`. If no target is supplied, or the target is unknown, it falls back to `3d-printing`. The source of truth is `Maliev.Web.Client/Content/HeroCopyCatalog.cs`; tests live in `Maliev.Web.Tests/HeroCopyCatalogTests.cs`.

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

When adding or renaming an ad target, update the catalog, the route tests, `README.md`, and this file in the same commit.
