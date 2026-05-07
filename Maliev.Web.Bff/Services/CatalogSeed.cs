using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Bff.Services;

internal static class CatalogSeed
{
    private const string MachineImage = "https://shop.maliev.com/cdn/shop/files/machine-portrait.21.png?v=1737116109&width=832";
    private const string MarkerImage = "https://shop.maliev.com/cdn/shop/files/Magnetic_base_1.5x3_white.1.png?v=1743005723&width=832";

    internal static IReadOnlyList<ProductCollectionDto> Collections { get; } =
    [
        new()
        {
            Slug = "injection-molding-machines",
            Name = Text("Injection molding machines", "เครื่องฉีดพลาสติก"),
            Summary = Text("Desktop pneumatic machines for low-volume plastic production.", "เครื่องฉีดพลาสติกแบบตั้งโต๊ะสำหรับการผลิตจำนวนน้อย"),
            ExpectedProductCount = 2
        },
        new()
        {
            Slug = "machine-parts-spares",
            Name = Text("Machine parts and spares", "อะไหล่เครื่องจักร"),
            Summary = Text("Pneumatic, heating, frame, and injection tube parts migrated from Shopify.", "อะไหล่ระบบลม ชุดทำความร้อน โครง และท่อฉีดจาก Shopify"),
            ExpectedProductCount = 44
        },
        new()
        {
            Slug = "3d-printed-products",
            Name = Text("3D printed products", "สินค้าพิมพ์สามมิติ"),
            Summary = Text("Functional printed accessories, fixtures, and SimMount products.", "อุปกรณ์ ฟิกซ์เจอร์ และสินค้า SimMount ที่ผลิตด้วยการพิมพ์สามมิติ"),
            ExpectedProductCount = 16
        },
        new()
        {
            Slug = "manufacturing-services",
            Name = Text("Manufacturing services", "บริการผลิต"),
            Summary = Text("3D printing, CNC machining, 3D scanning, molds, and injection services.", "บริการพิมพ์สามมิติ CNC สแกนสามมิติ แม่พิมพ์ และฉีดพลาสติก"),
            ExpectedProductCount = 4
        }
    ];

    internal static IReadOnlyList<ProductDetailDto> Products { get; } =
    [
        Product(
            "pneumatic-injection-molding-machine",
            "Pneumatic Injection Molding Machine (30g)",
            "เครื่องฉีดพลาสติกระบบลม 30 กรัม",
            "Compact pneumatic machine for prototypes, education, and low-volume plastic parts.",
            "เครื่องฉีดพลาสติกขนาดกะทัดรัดสำหรับต้นแบบ การศึกษา และผลิตชิ้นงานจำนวนน้อย",
            "injection-molding-machines",
            99000m,
            115000m,
            true,
            30,
            MachineImage,
            "https://shop.maliev.com/products/pneumatic-injection-molding-machine",
            ["machine", "injection molding", "pneumatic", "30g"]),
        Product(
            "injection-molding-machine-50g",
            "Injection Molding Machine 50g",
            "เครื่องฉีดพลาสติก 50 กรัม",
            "Larger shot-size desktop injection platform for small production runs.",
            "เครื่องฉีดพลาสติกขนาดช็อตใหญ่ขึ้นสำหรับงานผลิตจำนวนน้อย",
            "injection-molding-machines",
            170000m,
            199000m,
            true,
            30,
            MachineImage,
            "https://shop.maliev.com/products/injection-molding-machine-50g",
            ["machine", "injection molding", "50g"]),
        Product(
            "magnetic-marker-base-for-3d-scanning",
            "Magnetic Marker Base for 3D Scanning",
            "ฐานมาร์กเกอร์แม่เหล็กสำหรับสแกนสามมิติ",
            "Ready-to-use magnetic marker bases for repeatable 3D scanning alignment.",
            "ฐานมาร์กเกอร์แม่เหล็กพร้อมใช้สำหรับจัดแนวงานสแกนสามมิติให้แม่นยำ",
            "3d-printed-products",
            83.46m,
            null,
            true,
            1,
            MarkerImage,
            "https://shop.maliev.com/products/magnetic-marker-base-for-3d-scanning",
            ["3d scanning", "marker", "3d printed"]),
        Product(
            "custom-made-mold",
            "Custom-Made Mold",
            "แม่พิมพ์สั่งทำ",
            "Quote-request product for custom injection mold design and production.",
            "สินค้าสำหรับขอใบเสนอราคาแม่พิมพ์ฉีดพลาสติกสั่งทำ",
            "manufacturing-services",
            0m,
            null,
            false,
            14,
            MachineImage,
            "https://shop.maliev.com/products/custom-made-mold",
            ["mold", "tooling", "injection service"]),
        Product(
            "3d-printing-service-individual-order",
            "3D Printing Service - Individual Order",
            "บริการพิมพ์สามมิติแบบรายงาน",
            "Manual-order service entry for customer-specific 3D printing jobs.",
            "บริการพิมพ์สามมิติสำหรับคำสั่งซื้อเฉพาะลูกค้า",
            "manufacturing-services",
            0m,
            null,
            false,
            3,
            MarkerImage,
            "https://shop.maliev.com/products/3d-printing-service-individual-order",
            ["3d printing", "service"]),
        Product(
            "20mm-linear-shafts-for-injection-molding-machine",
            "20mm Linear Shafts for Injection Molding Machine",
            "เพลาเชิงเส้น 20 มม. สำหรับเครื่องฉีดพลาสติก",
            "Replacement linear shafts for MALIEV injection molding machines.",
            "เพลาอะไหล่สำหรับเครื่องฉีดพลาสติกของ MALIEV",
            "machine-parts-spares",
            3434.70m,
            null,
            false,
            14,
            MachineImage,
            "https://shop.maliev.com/products/20mm-linear-shafts-for-injection-molding-machine",
            ["spare parts", "machine"]),
        Product(
            "electronic-control-unit",
            "Electronic Control Unit",
            "ชุดควบคุมอิเล็กทรอนิกส์",
            "Replacement control electronics for injection molding machines.",
            "ชุดควบคุมอิเล็กทรอนิกส์อะไหล่สำหรับเครื่องฉีดพลาสติก",
            "machine-parts-spares",
            20319.30m,
            null,
            false,
            14,
            MachineImage,
            "https://shop.maliev.com/products/electronic-control-unit",
            ["spare parts", "controller"]),
        Product(
            "ecoqueue-desk-paper-recycle-bin",
            "EcoQueue Desk Paper Recycle Bin for Office & Queue Counters",
            "ถังรีไซเคิลกระดาษตั้งโต๊ะ EcoQueue",
            "A compact 3D printed desk bin for service counters and offices.",
            "ถังตั้งโต๊ะพิมพ์สามมิติสำหรับเคาน์เตอร์บริการและสำนักงาน",
            "3d-printed-products",
            369.15m,
            547.84m,
            false,
            3,
            MarkerImage,
            "https://shop.maliev.com/products/ecoqueue-desk-paper-recycle-bin-for-office-queue-counters",
            ["3d printed", "office"])
    ];

    internal static ShopifyImportPreviewDto ShopifyPreview()
    {
        return new ShopifyImportPreviewDto
        {
            SourceStorefrontUrl = "https://shop.maliev.com/collections/all",
            ExpectedProductCount = 57,
            HighestObservedPriceThb = 165887.85m,
            SeededProducts = Products.Select(p => (ProductSummaryDto)p).ToList(),
            Notes =
            [
                "Public products.json is protected by Cloudflare; use Shopify Admin GraphQL or CSV export for the authoritative import.",
                "Preserve Shopify handles as redirects to avoid breaking existing product links.",
                "Initial storefront filters expose MALIEV Co., Ltd. products, lead-time facets, and machine-heavy categories."
            ]
        };
    }

    private static ProductDetailDto Product(
        string handle,
        string titleEn,
        string titleTh,
        string summaryEn,
        string summaryTh,
        string collection,
        decimal price,
        decimal? compareAt,
        bool startsAt,
        int leadTime,
        string image,
        string sourceUrl,
        List<string> keywords)
    {
        return new ProductDetailDto
        {
            Handle = handle,
            Title = Text(titleEn, titleTh),
            Summary = Text(summaryEn, summaryTh),
            Body = Text(summaryEn, summaryTh),
            CollectionSlug = collection,
            PriceThb = price,
            CompareAtPriceThb = compareAt,
            PriceStartsAt = startsAt,
            LeadTimeDays = leadTime,
            ImageUrl = image,
            SourceUrl = sourceUrl,
            SeoKeywords = keywords,
            Media = [new ProductMediaDto { Url = image, Alt = titleEn }],
            Variants =
            [
                new ProductVariantDto
                {
                    Sku = $"MALIEV-{handle.ToUpperInvariant()[..Math.Min(handle.Length, 18)]}",
                    Title = Text("Default", "ค่าเริ่มต้น"),
                    PriceThb = price,
                    Available = true
                }
            ]
        };
    }

    private static LocalizedText Text(string en, string th)
    {
        return new LocalizedText { En = en, Th = th };
    }
}
