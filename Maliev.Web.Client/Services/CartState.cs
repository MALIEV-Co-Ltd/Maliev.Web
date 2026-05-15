using Maliev.Web.Shared.Commerce;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Maliev.Web.Client.Services;

internal sealed class CartState(IJSRuntime js)
{
    private const string StorageKey = "maliev.cart.v1";
    private readonly List<CartItemDto> _items = [];
    private bool _initialized;

    internal event Action? Changed;

    internal IReadOnlyList<CartItemDto> Items => _items;

    internal int Count => _items.Sum(item => Math.Max(1, item.Quantity));

    internal async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var items = JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? [];
                _items.Clear();
                _items.AddRange(items.Where(item =>
                    !string.IsNullOrWhiteSpace(item.ProductHandle) &&
                    item.Quantity > 0 &&
                    item.UnitPriceThb >= 0));
            }
        }
        catch (JsonException)
        {
            await ClearStoredCartAsync();
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }

        Changed?.Invoke();
    }

    internal async Task AddAsync(ProductSummaryDto product, ProductVariantDto? variant = null)
    {
        var selectedVariant = variant ?? new ProductVariantDto
        {
            Sku = product.Handle,
            Title = product.Title,
            PriceThb = product.PriceThb,
            Available = true
        };
        var sku = string.IsNullOrWhiteSpace(selectedVariant.Sku) ? product.Handle : selectedVariant.Sku;
        var existing = _items.FirstOrDefault(item => item.ProductHandle == product.Handle && item.VariantSku == sku);
        if (existing is null)
        {
            _items.Add(new CartItemDto
            {
                ProductHandle = product.Handle,
                VariantSku = sku,
                Quantity = 1,
                Title = product.Title.En,
                VariantTitle = selectedVariant.Title.En,
                ImageUrl = product.ImageUrl,
                UnitPriceThb = selectedVariant.PriceThb <= 0 ? product.PriceThb : selectedVariant.PriceThb
            });
        }
        else
        {
            existing.Quantity++;
        }

        await PersistAsync();
        Changed?.Invoke();
    }

    internal void UpdateQuantity(CartItemDto item, int quantity)
    {
        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        Persist();
        Changed?.Invoke();
    }

    internal void Remove(CartItemDto item)
    {
        _items.Remove(item);
        Persist();
        Changed?.Invoke();
    }

    internal void Clear()
    {
        _items.Clear();
        Persist();
        Changed?.Invoke();
    }

    private void Persist()
    {
        if (!_initialized)
        {
            return;
        }

        _ = PersistAsync();
    }

    private async Task PersistAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_items);
            await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }

    private async Task ClearStoredCartAsync()
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }
}
