# Maliev.Web

![CI - Develop](https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/workflows/ci-develop.yml/badge.svg)
![CI - Staging](https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/workflows/ci-staging.yml/badge.svg)
![CI - Main](https://github.com/MALIEV-Co-Ltd/Maliev.Web/actions/workflows/ci-main.yml/badge.svg)

The customer-facing MALIEV website for manufacturing services, instant quotations, and MALIEV's owned storefront.

## Architecture

`Maliev.Web` is a hosted Blazor WebAssembly application with a Web BFF.

| Project | Purpose |
| --- | --- |
| `Maliev.Web.Bff` | Hosts the Blazor shell, public Web API endpoints, localization middleware, health endpoints, and future downstream service clients. |
| `Maliev.Web.Client` | Customer UI for landing pages, instant quotation, shop, cart, account pages, and localization preference handling. |
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

## Development

```powershell
dotnet restore Maliev.Web.slnx
dotnet build Maliev.Web.slnx --configuration Release
dotnet test Maliev.Web.slnx --configuration Release
dotnet run --project Maliev.Web.Bff/Maliev.Web.Bff.csproj
```

The local site runs from the BFF launch profile at `https://localhost:7236` or `http://localhost:5026`.
