using Maliev.Web.Shared.Commerce;

namespace Maliev.Web.Client.Services;

internal sealed class CartState
{
    private readonly List<CartItemDto> _items = [];

    internal event Action? Changed;

    internal IReadOnlyList<CartItemDto> Items => _items;

    internal int Count => _items.Sum(item => Math.Max(1, item.Quantity));

    internal void Add(ProductSummaryDto product)
    {
        var existing = _items.FirstOrDefault(item => item.ProductHandle == product.Handle);
        if (existing is null)
        {
            _items.Add(new CartItemDto
            {
                ProductHandle = product.Handle,
                VariantSku = product.Handle,
                Quantity = 1
            });
        }
        else
        {
            existing.Quantity++;
        }

        Changed?.Invoke();
    }

    internal void Clear()
    {
        _items.Clear();
        Changed?.Invoke();
    }
}
