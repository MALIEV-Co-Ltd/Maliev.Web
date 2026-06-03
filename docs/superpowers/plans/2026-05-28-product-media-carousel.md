# Product Detail Media Carousel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the horizontal thumbnail strip below the main image on `ProductDetail` with a vertical left-side carousel, hover-aware opacity on thumbnails, and a CSS cross-fade when switching images.

**Architecture:** Two files only — `app.css` for all visual changes and `ProductDetail.razor` for template restructuring and async cross-fade state. Tests are source-level (read file content, assert strings) matching the project's existing xunit pattern.

**Tech Stack:** Blazor WASM (.NET 10), CSS (no JS), xunit source tests

---

## File Map

| File | Role |
|---|---|
| `Maliev.Web.Client/Pages/ProductDetail.razor` | Template + C# state/logic |
| `Maliev.Web.Bff/wwwroot/app.css` | All CSS changes |
| `Maliev.Web.Tests/ProductDetailMediaSourceTests.cs` | New — source-level regression tests |

---

## Task 1: Write failing tests

**Files:**
- Create: `Maliev.Web.Tests/ProductDetailMediaSourceTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
namespace Maliev.Web.Tests;

/// <summary>
/// Source-level regression tests for the product detail left thumbnail carousel
/// and cross-fade image transition.
/// </summary>
public sealed class ProductDetailMediaSourceTests
{
    private static string Razor => ReadRepoFile("Maliev.Web.Client", "Pages", "ProductDetail.razor");
    private static string Css   => ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

    // ── CSS: layout ─────────────────────────────────────────────────────────

    [Fact]
    public void Css_MediaGrid_HasLeftThumbColumn()
        => Assert.Contains("grid-template-columns: 72px", Css, StringComparison.Ordinal);

    [Fact]
    public void Css_MediaStrip_IsVerticalFlex()
        => Assert.Contains("flex-direction: column", Css, StringComparison.Ordinal);

    // ── CSS: thumbnail opacity ───────────────────────────────────────────────

    [Fact]
    public void Css_MediaThumbImg_HasHalfOpacityAtRest()
        => Assert.Contains("opacity: 0.5", Css, StringComparison.Ordinal);

    [Fact]
    public void Css_MediaThumbHover_BringsToFullOpacity()
        => Assert.Contains(".media-thumb:hover img", Css, StringComparison.Ordinal);

    [Fact]
    public void Css_MediaThumbActiveImg_HasOutlineOffset()
        => Assert.Contains("outline-offset: 2px", Css, StringComparison.Ordinal);

    // ── CSS: cross-fade ──────────────────────────────────────────────────────

    [Fact]
    public void Css_HasCrossfadeInKeyframe()
        => Assert.Contains("img-crossfade-in", Css, StringComparison.Ordinal);

    [Fact]
    public void Css_HasCrossfadeOutKeyframe()
        => Assert.Contains("img-crossfade-out", Css, StringComparison.Ordinal);

    [Fact]
    public void Css_HasProductDetailImgInClass()
        => Assert.Contains(".product-detail-img--in", Css, StringComparison.Ordinal);

    [Fact]
    public void Css_HasProductDetailImgOutClass()
        => Assert.Contains(".product-detail-img--out", Css, StringComparison.Ordinal);

    // ── CSS: mobile override ─────────────────────────────────────────────────

    [Fact]
    public void Css_MobileOverride_MediaStripIsHorizontal()
        => Assert.Contains("flex-direction: row", Css, StringComparison.Ordinal);

    // ── Razor: state ─────────────────────────────────────────────────────────

    [Fact]
    public void Razor_DoesNotContainSelectedImageField()
        => Assert.DoesNotContain("_selectedImage", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasDisplayedImageField()
        => Assert.Contains("_displayedImage", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasFadingOutImageField()
        => Assert.Contains("_fadingOutImage", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasIsTransitioningField()
        => Assert.Contains("_isTransitioning", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasSelectImageMethod()
        => Assert.Contains("SelectImage", Razor, StringComparison.Ordinal);

    // ── Razor: template ──────────────────────────────────────────────────────

    [Fact]
    public void Razor_HasMediaMainContainer()
        => Assert.Contains("product-detail-media-main", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasImgInClass()
        => Assert.Contains("product-detail-img--in", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasImgOutClass()
        => Assert.Contains("product-detail-img--out", Razor, StringComparison.Ordinal);

    [Fact]
    public void Razor_HasKeyOnDisplayedImageUrl()
        => Assert.Contains("@key=\"@_displayedImage.Url\"", Razor, StringComparison.Ordinal);

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepositoryRoot()
    {
        foreach (var startDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(startDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
                    return directory.FullName;
                var siblingCandidate = Path.Combine(directory.FullName, "Maliev.Web");
                if (File.Exists(Path.Combine(siblingCandidate, "Maliev.Web.slnx")))
                    return siblingCandidate;
                directory = directory.Parent;
            }
        }
        throw new DirectoryNotFoundException("Could not find Maliev.Web repository root.");
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```
dotnet test Maliev.Web.Tests/Maliev.Web.Tests.csproj --filter "ProductDetailMedia" -v minimal
```

Expected: most tests fail (the CSS and Razor assertions don't match the current source). A few may pass if the old code happens to contain a matching string — that's fine.

---

## Task 2: CSS — media grid and thumbnail strip

**Files:**
- Modify: `Maliev.Web.Bff/wwwroot/app.css:6459-6495`

- [ ] **Step 1: Update `.product-detail-media` — add the two-column grid**

Find this block (lines 6459–6462):
```css
.product-detail-media {
  display: grid;
  gap: 10px;
}
```

Replace with:
```css
.product-detail-media {
  display: grid;
  grid-template-columns: 72px minmax(0, 1fr);
  gap: 10px;
  align-items: start;
}
```

- [ ] **Step 2: Replace the combined `.product-detail-media > img, .product-detail-media-main` rule**

Find this block (lines 6464–6471):
```css
.product-detail-media > img,
.product-detail-media-main {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: 16px;
  box-shadow: var(--shadow-card);
}
```

Replace with (remove `> img` selector and `object-fit`; `position: relative`, `overflow: hidden`, and `background` are already provided by the shared group selector at line 1680):
```css
.product-detail-media-main {
  width: 100%;
  aspect-ratio: 1 / 1;
  border-radius: 16px;
  box-shadow: var(--shadow-card);
}
```

- [ ] **Step 3: Update `.media-strip` — vertical flex column**

Find this block (lines 6473–6477):
```css
.media-strip {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 8px;
}
```

Replace with:
```css
.media-strip {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
```

- [ ] **Step 4: Update `.media-thumb img` — add hover-aware opacity**

Find this block (lines 6485–6491):
```css
.media-thumb img {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: var(--radius);
  box-shadow: var(--shadow-border);
}
```

Replace with:
```css
.media-thumb img {
  width: 100%;
  aspect-ratio: 1 / 1;
  object-fit: cover;
  border-radius: var(--radius);
  box-shadow: var(--shadow-border);
  opacity: 0.5;
  transition: opacity 0.18s ease;
}

.media-thumb:hover img {
  opacity: 1;
}
```

- [ ] **Step 5: Update `.media-thumb.active img` — add opacity and outline-offset**

Find this block (lines 6493–6495):
```css
.media-thumb.active img {
  outline: 2px solid var(--blue);
}
```

Replace with:
```css
.media-thumb.active img {
  opacity: 1;
  outline: 2px solid var(--blue);
  outline-offset: 2px;
}
```

---

## Task 3: CSS — cross-fade rules and mobile override

**Files:**
- Modify: `Maliev.Web.Bff/wwwroot/app.css` (two locations)

- [ ] **Step 1: Add `.product-detail-img` rules and keyframes**

Find this block (around line 6504):
```css
.product-description {
  color: var(--muted);
  font-size: 1rem;
  line-height: 1.6;
}
```

Insert the following **after** that block:
```css
.product-detail-img {
  position: absolute;
  inset: 0;
}

.product-detail-img--in {
  z-index: 2;
  animation: img-crossfade-in 0.22s ease both;
}

.product-detail-img--out {
  z-index: 1;
  animation: img-crossfade-out 0.22s ease both;
}

@keyframes img-crossfade-in {
  from { opacity: 0; }
  to   { opacity: 1; }
}

@keyframes img-crossfade-out {
  from { opacity: 1; }
  to   { opacity: 0; }
}
```

(`.product-detail-media img` at line 1707 already provides `width: 100%; height: 100%; object-fit: cover` to these elements via cascade, so `.product-detail-img` only needs to add `position: absolute; inset: 0`.)

- [ ] **Step 2: Add mobile overrides inside the `@media (max-width: 960px)` block**

Find this block inside `@media (max-width: 960px)` (around line 8557):
```css
  .product-detail-copy,
  .cart-summary,
  .account-nav-card {
    position: static;
  }
```

Insert the following **after** that block (still inside the same `@media` rule, before the closing `}`):
```css
  .product-detail-media {
    grid-template-columns: 1fr;
  }

  .media-strip {
    flex-direction: row;
    overflow-x: auto;
    overscroll-behavior-x: contain;
  }
```

---

## Task 4: Razor — state fields, SelectImage method, template

**Files:**
- Modify: `Maliev.Web.Client/Pages/ProductDetail.razor`

- [ ] **Step 1: Replace the entire file**

The full updated file (every change marked inline):

```razor
@page "/shop/{Handle}"
@inherits PreferenceAwareComponentBase
@inject MalievApiClient Api
@inject CartState Cart

<PageTitle>@(_product?.Title.For(Preferences.Culture) ?? "Product") - MALIEV</PageTitle>

@if (_product is null)
{
    <section class="page-hero compact">
        <h1>Product not found</h1>
        <p>@(_loadError ?? "This product is not available right now.")</p>
        <a class="button secondary" href="/shop">@Text("Back to shop", "กลับไปหน้าร้าน")</a>
    </section>
}
else
{
    <section class="product-detail">
        <div class="product-detail-media">
            <div class="media-strip">
                @if (_product.Media.Count > 1)
                {
                    @foreach (var media in _product.Media)
                    {
                        <button type="button" class="@MediaButtonClass(media)" @onclick="async () => await SelectImage(media)">
                            <img src="@media.Url" alt="" aria-hidden="true" />
                        </button>
                    }
                }
            </div>
            <div class="product-detail-media-main">
                @if (_fadingOutImage is not null)
                {
                    <img class="product-detail-img product-detail-img--out"
                         src="@_fadingOutImage.Url"
                         alt=""
                         aria-hidden="true" />
                }
                @if (_displayedImage is not null)
                {
                    <img @key="@_displayedImage.Url"
                         class="product-detail-img product-detail-img--in"
                         src="@_displayedImage.Url"
                         alt="@_displayedImage.Alt" />
                }
            </div>
        </div>
        <div class="product-detail-copy">
            <h1>@_product.Title.For(Preferences.Culture)</h1>
            <div class="product-description">@((MarkupString)_product.Body.For(Preferences.Culture))</div>
            <div class="product-price">@Price(_product)</div>
            @if (_product.Variants.Count > 0)
            {
                <label class="variant-select">
                    <span>@Text("Variant", "ตัวเลือกสินค้า")</span>
                    <select @bind="_selectedVariantSku">
                        @foreach (var variant in _product.Variants)
                        {
                            <option value="@variant.Sku" disabled="@(!variant.Available)">
                                @variant.Title.For(Preferences.Culture) · @Price(variant)
                            </option>
                        }
                    </select>
                </label>
            }
            <dl class="spec-list">
                <div><dt>@Text("Lead time", "ระยะเวลา")</dt><dd>@Text($"{_product.LeadTimeDays} days", $"{_product.LeadTimeDays} วัน")</dd></div>
                <div><dt>@Text("Availability", "สถานะสินค้า")</dt><dd>@InventoryStatus(_product.InventoryStatus)</dd></div>
                @if (_product.AvailableQuantity is not null)
                {
                    <div><dt>@Text("Quantity", "จำนวน")</dt><dd>@Text($"{_product.AvailableQuantity} available", $"มีสินค้า {_product.AvailableQuantity}")</dd></div>
                }
            </dl>
            <div class="quote-actions">
                <button type="button" class="button primary" @onclick="AddToCart" disabled="@(!CanBuy)">@Text("Add to cart", "เพิ่มลงตะกร้า")</button>
                <a class="button secondary" href="@SiteContent.QuoteNewUrl">@Text("Request custom part", "ขอผลิตชิ้นงานเฉพาะแบบ")</a>
            </div>
        </div>
    </section>
}

@code {
    [Parameter]
    public string Handle { get; set; } = string.Empty;

    private ProductDetailDto? _product;
    private ProductMediaDto? _displayedImage;
    private ProductMediaDto? _fadingOutImage;
    private bool _isTransitioning;
    private string _selectedVariantSku = string.Empty;
    private string? _loadError;
    private ProductVariantDto? SelectedVariant => _product?.Variants.FirstOrDefault(variant => variant.Sku == _selectedVariantSku) ?? _product?.Variants.FirstOrDefault();
    private bool CanBuy => _product is not null &&
        _product.PriceThb > 0 &&
        _product.IsPublished &&
        _product.AvailableQuantity.GetValueOrDefault(1) > 0 &&
        (SelectedVariant?.Available ?? true);

    protected override async Task OnParametersSetAsync()
    {
        await Preferences.InitializeAsync();
        await Cart.InitializeAsync();
        _fadingOutImage = null;
        _isTransitioning = false;
        try
        {
            _loadError = null;
            _product = await Api.GetProductAsync(Handle);
            _displayedImage = _product?.Media.FirstOrDefault() ??
                (!string.IsNullOrWhiteSpace(_product?.ImageUrl) ? new ProductMediaDto { Url = _product.ImageUrl, Alt = _product.Title.For(Preferences.Culture) } : null);
            _selectedVariantSku = _product?.Variants.FirstOrDefault(variant => variant.Available)?.Sku ??
                _product?.Variants.FirstOrDefault()?.Sku ??
                string.Empty;
        }
        catch (MalievApiException ex)
        {
            _loadError = ex.Message;
        }
    }

    private async Task SelectImage(ProductMediaDto media)
    {
        if (_isTransitioning || media.Url == _displayedImage?.Url) return;
        _fadingOutImage = _displayedImage;
        _displayedImage = media;
        _isTransitioning = true;
        StateHasChanged();
        await Task.Delay(240);
        _fadingOutImage = null;
        _isTransitioning = false;
        StateHasChanged();
    }

    private async Task AddToCart()
    {
        if (_product is not null)
        {
            await Cart.AddAsync(_product, SelectedVariant);
        }
    }

    private string MediaButtonClass(ProductMediaDto media)
    {
        return media.Url == _displayedImage?.Url ? "media-thumb active" : "media-thumb";
    }

    private string Price(ProductSummaryDto product)
    {
        var prefix = product.PriceStartsAt ? Text("From ", "เริ่มต้น ") : string.Empty;
        return product.PriceThb <= 0 ? Text("Request quote", "ขอใบเสนอราคา") : string.Create(CultureInfo.GetCultureInfo("th-TH"), $"{prefix}฿{product.PriceThb:N2}");
    }

    private string Price(ProductVariantDto variant)
    {
        return variant.PriceThb <= 0 ? Text("Request quote", "ขอใบเสนอราคา") : string.Create(CultureInfo.GetCultureInfo("th-TH"), $"฿{variant.PriceThb:N2}");
    }

    private string Text(string en, string th)
    {
        return Preferences.Culture.StartsWith("th", StringComparison.OrdinalIgnoreCase) ? th : en;
    }

    private string InventoryStatus(string status)
    {
        return status switch
        {
            "Available" => Text("Available", "พร้อมจำหน่าย"),
            "Low stock" => Text("Low stock", "สินค้าเหลือน้อย"),
            "Made to order" => Text("Made to order", "ผลิตตามสั่ง"),
            _ => status
        };
    }
}
```

Key changes from the original:
- `_selectedImage` removed; `_displayedImage`, `_fadingOutImage`, `_isTransitioning` added
- `AddToCart` was `private async Task` — kept as-is
- `.media-strip` div always rendered (even when `Media.Count <= 1`) so grid column 1 is always occupied
- `product-detail-media-main` wraps both the outgoing and incoming `<img>` elements
- `@key="@_displayedImage.Url"` on the incoming `<img>` forces a new DOM element per image switch
- `_fadingOutImage = null; _isTransitioning = false;` reset at the top of `OnParametersSetAsync` clears any in-flight transition on page navigation

---

## Task 5: Build, run tests, commit

**Files:**
- No new changes — verify and commit

- [ ] **Step 1: Build**

```
dotnet build Maliev.Web.slnx
```

Expected: Build succeeded, 0 error(s), 0 warning(s).

- [ ] **Step 2: Run all tests**

```
dotnet test Maliev.Web.Tests/Maliev.Web.Tests.csproj -v minimal
```

Expected: All tests pass. The new `ProductDetailMediaSourceTests` (18 tests) should all be green. The pre-existing test suite should have no regressions.

- [ ] **Step 3: Commit**

```bash
git add Maliev.Web.Tests/ProductDetailMediaSourceTests.cs \
        Maliev.Web.Client/Pages/ProductDetail.razor \
        Maliev.Web.Bff/wwwroot/app.css
git commit -m "feat: left thumbnail carousel with cross-fade on product detail

- Vertical 72px thumbnail strip left of main image (desktop)
- Hover-aware opacity: inactive thumbs 50%, full on hover/active
- CSS cross-fade (0.22s) via two stacked images with @keyframes
- Mobile: thumbnail strip reverts to horizontal scroll row

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```
