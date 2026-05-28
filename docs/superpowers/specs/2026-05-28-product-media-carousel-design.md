# Product Detail — Left Thumbnail Carousel + Cross-fade

**Date:** 2026-05-28  
**Status:** Approved  
**Scope:** `Maliev.Web.Client` — `ProductDetail.razor` + `app.css`

---

## Overview

The `ProductDetail` page currently shows a horizontal thumbnail strip below the main image. This spec replaces it with a vertical left-side thumbnail carousel matching the style of high-quality product photography experiences (reference: screenshot provided by user). Key behaviours:

- Thumbnails arranged vertically to the left of the main image.
- Inactive thumbnails at 50% opacity, rising to 100% on hover and when active.
- Active thumbnail highlighted with a blue outline ring.
- Switching thumbnails cross-fades the main image (outgoing fades out while incoming fades in, ~220ms).
- Mobile: layout collapses to the existing horizontal bottom strip.

---

## Layout

### Desktop

`.product-detail-media` changes from a single-column vertical stack to a two-column horizontal grid:

```
grid-template-columns: 72px minmax(0, 1fr)
gap: 10px
align-items: start
```

Left column (`72px`): `.media-strip` — vertical flex column of thumbnails.  
Right column: `.product-detail-media-main` — the stacked cross-fade image container.

### Mobile (< 768px)

`.product-detail-media` reverts to a single column. `.media-strip` reverts to the existing horizontal row (`flex-direction: row`, `overflow-x: auto`). This matches the current mobile behaviour exactly.

---

## Thumbnail Strip

### CSS changes

`.media-strip` changes from:
```css
display: grid;
grid-template-columns: repeat(5, minmax(0, 1fr));
gap: 8px;
```
to:
```css
display: flex;
flex-direction: column;
gap: 8px;
```

### Thumbnail opacity (hover-aware)

```css
.media-thumb img {
  opacity: 0.5;
  transition: opacity 0.18s ease;
}
.media-thumb:hover img {
  opacity: 1;
}
.media-thumb.active img {
  opacity: 1;
  outline: 2px solid var(--blue);
  outline-offset: 2px;
}
```

Thumbnail images remain `width: 100%; aspect-ratio: 1/1; object-fit: cover` as today. The `border-radius` and `box-shadow` on `.media-thumb img` are retained.

---

## Main Image — Cross-fade (Approach B)

### State changes in `ProductDetail.razor`

| Old field | New field | Purpose |
|---|---|---|
| `_selectedImage` | `_displayedImage` | Currently fully visible image |
| _(new)_ | `_fadingOutImage` | Previous image, fading out |
| _(new)_ | `_isTransitioning` | Guard against double-clicks during animation |

`_selectedImage` is removed entirely. All existing references to `_selectedImage` in `OnParametersSetAsync` and `MediaButtonClass` are updated to use `_displayedImage`.

### `SelectImage()` method

Replaces the inline `@onclick="() => _selectedImage = media"` lambda on `.media-thumb`:

```csharp
private async Task SelectImage(ProductMediaDto media)
{
    if (_isTransitioning || media.Url == _displayedImage?.Url) return;
    _fadingOutImage = _displayedImage;
    _displayedImage = media;
    _isTransitioning = true;
    StateHasChanged();
    await Task.Delay(240);   // slightly longer than CSS duration to avoid flicker
    _fadingOutImage = null;
    _isTransitioning = false;
    StateHasChanged();
}
```

### Template

The single `<img src="@_selectedImage.Url" />` becomes:

```razor
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
```

`@key="@_displayedImage.Url"` forces Blazor to create a new DOM element for each incoming image (instead of patching `src` in place), ensuring the CSS animation fires on every swap.

### CSS for cross-fade

```css
.product-detail-media-main {
  position: relative;
  width: 100%;
  aspect-ratio: 1 / 1;
  border-radius: 16px;
  overflow: hidden;
  background: var(--paper-2);
  box-shadow: var(--shadow-card);
}

.product-detail-img {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
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

The existing `.product-detail-media > img` rule (which targeted the old bare `<img>`) is removed. `border-radius` and `box-shadow` move to `.product-detail-media-main`.

---

## Single-image fallback

When `_product.Media.Count <= 1`, no `.media-strip` is rendered (same as today). `.product-detail-media` renders only the right column — the grid still works because the left column simply has no content. No special-casing needed.

---

## Files Changed

| File | Change |
|---|---|
| `Maliev.Web.Client/Pages/ProductDetail.razor` | Template restructure; rename `_selectedImage`→`_displayedImage`; add `_fadingOutImage`, `_isTransitioning`; add `SelectImage()` async method |
| `Maliev.Web.Bff/wwwroot/app.css` | Update `.product-detail-media` grid; update `.media-strip`; update `.media-thumb img` opacity; add `.product-detail-media-main`, `.product-detail-img`, `--in`/`--out` variants, `@keyframes` |

No new files. No new dependencies.

---

## Out of Scope

- Zoom / lightbox on main image click
- Video media support
- Swipe gesture for mobile carousel (beyond existing scroll)
