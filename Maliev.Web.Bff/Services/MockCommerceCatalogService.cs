using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Bff.Services;

internal sealed class MockCommerceCatalogService : ICommerceCatalogService
{
    private static readonly IReadOnlyList<ProductCollectionDto> Collections =
    [
        new()
        {
            Slug = "injection-molding-machines",
            Name = new LocalizedText { En = "Injection molding machines", Th = "เครื่องฉีดพลาสติก" },
            Summary = new LocalizedText { En = "Pneumatic machines and production-ready systems.", Th = "เครื่องฉีดลมและชุดพร้อมผลิต" },
            ExpectedProductCount = 2
        },
        new()
        {
            Slug = "machine-parts",
            Name = new LocalizedText { En = "Machine parts", Th = "อะไหล่เครื่องจักร" },
            Summary = new LocalizedText { En = "Replacement assemblies, brackets, and pneumatic parts.", Th = "ชุดประกอบทดแทน ขายึด และอุปกรณ์ลม" },
            ExpectedProductCount = 3
        },
        new()
        {
            Slug = "molds-tooling",
            Name = new LocalizedText { En = "Molds and tooling", Th = "แม่พิมพ์และทูลลิ่ง" },
            Summary = new LocalizedText { En = "Tooling, fixtures, and production aids.", Th = "แม่พิมพ์ ฟิกซ์เจอร์ และอุปกรณ์ช่วยผลิต" },
            ExpectedProductCount = 2
        },
        new()
        {
            Slug = "3d-printed-products",
            Name = new LocalizedText { En = "3D printed products", Th = "สินค้าพิมพ์สามมิติ" },
            Summary = new LocalizedText { En = "Ready-to-buy printed parts, fixtures, and accessories.", Th = "ชิ้นงานพิมพ์สามมิติ ฟิกซ์เจอร์ และอุปกรณ์เสริมพร้อมซื้อ" },
            ExpectedProductCount = 2
        }
    ];

    private static readonly IReadOnlyList<ProductDetailDto> Products =
    [
        Product(
            "pneumatic-injection-molding-machine",
            "Pneumatic injection molding machine",
            "เครื่องฉีดพลาสติกระบบลม",
            "Bench-top injection molding machine for workshops, product trials, and low-volume production.",
            "เครื่องฉีดพลาสติกตั้งโต๊ะสำหรับเวิร์กช็อป ทดลองผลิตภัณฑ์ และงานผลิตจำนวนน้อย",
            "injection-molding-machines",
            170000m,
            14,
            "https://shop.maliev.com/cdn/shop/files/machine-portrait.21.png?v=1737116109&width=832",
            "Made to order",
            1,
            [
                Variant("MAL-IM-50", "50 g shot capacity", 170000m),
                Variant("MAL-IM-100", "100 g shot capacity", 225000m)
            ]),
        Product(
            "starter-mold-and-training-kit",
            "Starter mold and training kit",
            "ชุดแม่พิมพ์เริ่มต้นพร้อมอบรม",
            "Tooling and hands-on setup package for first injection molding production trials.",
            "ชุดทูลลิ่งและการตั้งค่าเริ่มต้นสำหรับทดลองผลิตด้วยเครื่องฉีดพลาสติก",
            "molds-tooling",
            28000m,
            10,
            "https://shop.maliev.com/cdn/shop/files/spare-part.png?v=1756281837&width=832",
            "Available",
            3,
            [Variant("MOLD-STARTER", "Starter kit", 28000m)]),
        Product(
            "20mm-linear-shaft-set",
            "20 mm linear shaft set",
            "ชุดแกนสไลด์ 20 มม.",
            "Replacement shaft set for MALIEV pneumatic injection molding machine alignment and maintenance.",
            "ชุดแกนสไลด์ทดแทนสำหรับตั้งศูนย์และบำรุงรักษาเครื่องฉีดพลาสติก MALIEV",
            "machine-parts",
            3434.7m,
            3,
            "https://shop.maliev.com/cdn/shop/files/110302574540_001.jpg?v=1759504756&width=832",
            "Available",
            8,
            [Variant("PSFGW20-500-MD12-ND12", "Default set", 3434.7m)]),
        Product(
            "pneumatic-cylinder-unit",
            "Pneumatic cylinder unit",
            "ชุดกระบอกลม",
            "Cylinder assembly for pneumatic machine repair, retrofit, and scheduled service.",
            "ชุดกระบอกลมสำหรับซ่อม อัปเกรด และบำรุงรักษาเครื่องจักร",
            "machine-parts",
            13375m,
            5,
            "https://shop.maliev.com/cdn/shop/files/speedaire-5tev9.png?v=1759505122&width=832",
            "Low stock",
            2,
            [Variant("CYLINDER-UNIT", "Default unit", 13375m)]),
        Product(
            "aluminum-profile-tap-guide",
            "Aluminum profile tap guide fixture",
            "ฟิกซ์เจอร์ไกด์ต๊าปโปรไฟล์อลูมิเนียม",
            "3D printed fixture for fast, repeatable tapping alignment on aluminum extrusion frames.",
            "ฟิกซ์เจอร์พิมพ์สามมิติสำหรับช่วยตั้งแนวต๊าปเกลียวโปรไฟล์อลูมิเนียมให้แม่นและเร็วขึ้น",
            "3d-printed-products",
            192.6m,
            2,
            "https://shop.maliev.com/cdn/shop/files/colors.51.png?v=1740108657&width=832",
            "Available",
            24,
            [
                Variant("TAP-2020-M5", "20 x 20 - M5", 192.6m),
                Variant("TAP-2040-M5", "20 x 40 - M5", 256.8m),
                Variant("TAP-3030-M8", "30 x 30 - M8", 251.45m)
            ]),
        Product(
            "pneumatic-switch-bracket",
            "Pneumatic switch mounting bracket",
            "ขายึดสวิตช์ลม",
            "Machine spare bracket for clean pneumatic switch mounting and service replacement.",
            "ขายึดอะไหล่สำหรับติดตั้งสวิตช์ลมและเปลี่ยนระหว่างงานซ่อมบำรุง",
            "machine-parts",
            2407.5m,
            3,
            "https://shop.maliev.com/cdn/shop/files/spare-part.png?v=1756281837&width=832",
            "Available",
            5,
            [Variant("BRACKET-SWITCH", "Default bracket", 2407.5m)]),
        Product(
            "scan-reference-target-kit",
            "3D scan reference target kit",
            "ชุดเป้าอ้างอิงสำหรับสแกนสามมิติ",
            "Reusable targets and mounting aids for part capture, reverse engineering, and inspection jobs.",
            "เป้าสแกนและอุปกรณ์ช่วยจับยึดสำหรับงานเก็บข้อมูล รีเวิร์สเอนจิเนียริ่ง และตรวจสอบชิ้นงาน",
            "molds-tooling",
            1450m,
            2,
            "https://shop.maliev.com/cdn/shop/files/Screenshot_2025-10-03_223753.png?v=1759505883&width=832",
            "Available",
            12,
            [Variant("SCAN-TARGET-KIT", "Standard kit", 1450m)]),
        Product(
            "fixture-sample-pack",
            "Printed fixture sample pack",
            "ชุดตัวอย่างฟิกซ์เจอร์พิมพ์สามมิติ",
            "Sample pack showing practical 3D printed jigs, caps, guides, and shop-floor accessories.",
            "ชุดตัวอย่างชิ้นงานพิมพ์สามมิติ เช่น จิ๊ก ฝาครอบ ไกด์ และอุปกรณ์ช่วยงานในเวิร์กช็อป",
            "3d-printed-products",
            890m,
            4,
            "https://shop.maliev.com/cdn/shop/files/colors.61.png?v=1740108788&width=832",
            "Available",
            10,
            [Variant("FIXTURE-SAMPLE", "Sample pack", 890m)])
    ];

    public Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Collections);
    }

    public Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collectionSlug, CancellationToken cancellationToken)
    {
        var products = Products.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(collectionSlug))
        {
            products = products.Where(product => product.CollectionSlug.Equals(collectionSlug, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<ProductSummaryDto>>(
            products.OrderBy(product => product.Title.En, StringComparer.OrdinalIgnoreCase)
                .Cast<ProductSummaryDto>()
                .ToList());
    }

    public Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken)
    {
        return Task.FromResult(Products.FirstOrDefault(product =>
            product.Handle.Equals(handle, StringComparison.OrdinalIgnoreCase)));
    }

    private static ProductDetailDto Product(
        string handle,
        string titleEn,
        string titleTh,
        string summaryEn,
        string summaryTh,
        string collection,
        decimal price,
        int leadTime,
        string imageUrl,
        string inventoryStatus,
        int availableQuantity,
        List<ProductVariantDto> variants)
    {
        return new ProductDetailDto
        {
            Handle = handle,
            Title = new LocalizedText { En = titleEn, Th = titleTh },
            Summary = new LocalizedText { En = summaryEn, Th = summaryTh },
            Body = new LocalizedText
            {
                En = $"{summaryEn} Add it to cart for checkout, or contact MALIEV when the item needs a custom configuration.",
                Th = $"{summaryTh} สามารถเพิ่มลงตะกร้าเพื่อสั่งซื้อ หรือสอบถาม MALIEV หากต้องการปรับแต่งเพิ่มเติม"
            },
            CollectionSlug = collection,
            PriceThb = price,
            PriceStartsAt = variants.Select(variant => variant.PriceThb).Distinct().Skip(1).Any(),
            LeadTimeDays = leadTime,
            ImageUrl = imageUrl,
            IsPublished = true,
            AvailableQuantity = availableQuantity,
            InventoryStatus = inventoryStatus,
            Media = [new ProductMediaDto { Url = imageUrl, Alt = titleEn }],
            Variants = variants,
            SeoKeywords = [collection, titleEn]
        };
    }

    private static ProductVariantDto Variant(string sku, string title, decimal price)
    {
        return new ProductVariantDto
        {
            Sku = sku,
            Title = new LocalizedText { En = title, Th = title },
            PriceThb = price,
            Available = true
        };
    }
}
