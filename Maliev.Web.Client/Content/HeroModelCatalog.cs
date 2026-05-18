using System.Security.Cryptography;

using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Client.Content;

internal static class HeroModelCatalog
{
    internal const string DefaultServiceSlug = "3d-printing";
    private const string LegacyFixtureServiceSlug = "3d-printing-legacy";

    private static readonly IReadOnlyList<HeroModelAsset> Assets =
    [
        new(
            "3d-printing-part-01",
            LegacyFixtureServiceSlug,
            "/models/hero-3d-printing-part-01.glb",
            SiteContent.Text("3D printed production fixture preview", "ตัวอย่างชิ้นงานฟิกซ์เจอร์จากงานพิมพ์ 3 มิติ"),
            true),
        new(
            "3d-printing-part-02",
            DefaultServiceSlug,
            "/models/hero-3d-printing-part-02.glb",
            SiteContent.Text("3D printed functional part preview", "ตัวอย่างชิ้นงานใช้งานจากงานพิมพ์ 3 มิติ"),
            true,
            1.72),
        new(
            "3d-scanning-part-01",
            "3d-scanning",
            "/models/hero-3d-scanning-part-01.glb",
            SiteContent.Text("3D scanning reference part preview", "ตัวอย่างชิ้นงานอ้างอิงสำหรับงานสแกน 3 มิติ"),
            false,
            1.24)
    ];

    private static readonly IReadOnlyDictionary<string, HeroModelAsset> AssetsByKey = Assets.ToDictionary(
        asset => asset.Key,
        StringComparer.OrdinalIgnoreCase);

    internal static HeroModelAsset Default => ResolveForService(DefaultServiceSlug)[0];

    internal static IReadOnlyList<HeroModelAsset> All => Assets;

    internal static HeroModelAsset Resolve(string? key)
    {
        return !string.IsNullOrWhiteSpace(key) && AssetsByKey.TryGetValue(key, out var asset)
            ? asset
            : Default;
    }

    internal static HeroModelAsset SelectRandomForService(string? serviceSlug)
    {
        var candidates = ResolveForService(serviceSlug);
        return candidates[RandomNumberGenerator.GetInt32(candidates.Count)];
    }

    internal static IReadOnlyList<HeroModelAsset> ResolveForService(string? serviceSlug)
    {
        if (string.IsNullOrWhiteSpace(serviceSlug))
        {
            return Assets;
        }

        var serviceAssets = Assets
            .Where(asset => string.Equals(asset.ServiceSlug, serviceSlug, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return serviceAssets.Length > 0 ? serviceAssets : Assets;
    }
}

internal sealed record HeroModelAsset(
    string Key,
    string ServiceSlug,
    string Url,
    LocalizedText AriaLabel,
    bool UsePlasticMaterial,
    double DisplayScale = 1.0);
