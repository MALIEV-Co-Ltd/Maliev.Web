using Maliev.Web.Client.Content;

namespace Maliev.Web.Tests;

/// <summary>
/// Tests the ad-targeted landing hero copy catalog.
/// </summary>
public sealed class HeroCopyCatalogTests
{
    private const int MaxHeadlineLineLength = 30;
    private const int MaxDefaultHeadlineLineLength = 20;
    private const int MaxBodyLength = 90;
    private const int MaxMetaDescriptionLength = 155;

    /// <summary>
    /// Verifies every ad target has enough localized copy for visit-level rotation.
    /// </summary>
    [Fact]
    public void AdTargets_HaveAtLeastFiftyCompleteLocalizedHeroVariants()
    {
        Assert.Contains(HeroCopyCatalog.AdTargets, target => target.Key == "fdm-3d-printing");
        Assert.Contains(HeroCopyCatalog.AdTargets, target => target.Key == "aluminum-cnc-milling");

        foreach (var target in HeroCopyCatalog.AdTargets)
        {
            var variants = HeroCopyCatalog.GetVariants(target.Key);

            Assert.True(variants.Count >= 50, $"{target.Key} only has {variants.Count} variants.");
            Assert.All(variants, variant =>
            {
                Assert.False(string.IsNullOrWhiteSpace(variant.HeadlineLead.En), $"{target.Key} has an empty English lead.");
                Assert.False(string.IsNullOrWhiteSpace(variant.HeadlineLead.Th), $"{target.Key} has an empty Thai lead.");
                Assert.False(string.IsNullOrWhiteSpace(variant.HeadlineAccent.En), $"{target.Key} has an empty English accent.");
                Assert.False(string.IsNullOrWhiteSpace(variant.HeadlineAccent.Th), $"{target.Key} has an empty Thai accent.");
                Assert.False(string.IsNullOrWhiteSpace(variant.Body.En), $"{target.Key} has an empty English body.");
                Assert.False(string.IsNullOrWhiteSpace(variant.Body.Th), $"{target.Key} has an empty Thai body.");
                Assert.False(string.IsNullOrWhiteSpace(variant.MetaDescription.En), $"{target.Key} has an empty English meta description.");
                Assert.False(string.IsNullOrWhiteSpace(variant.MetaDescription.Th), $"{target.Key} has an empty Thai meta description.");
            });
        }
    }

    /// <summary>
    /// Verifies generated hero text stays concise enough for the first viewport and metadata remains search-ready.
    /// </summary>
    [Fact]
    public void AdTargets_HeroCopyStaysConciseAndSeoReady()
    {
        foreach (var target in HeroCopyCatalog.AdTargets)
        {
            var variants = HeroCopyCatalog.GetVariants(target.Key);

            Assert.All(variants, variant =>
            {
                Assert.InRange(variant.HeadlineLead.En.Length, 1, MaxHeadlineLineLength);
                Assert.InRange(variant.HeadlineLead.Th.Length, 1, MaxHeadlineLineLength);
                Assert.InRange(variant.HeadlineAccent.En.Length, 1, MaxHeadlineLineLength);
                Assert.InRange(variant.HeadlineAccent.Th.Length, 1, MaxHeadlineLineLength);
                if (target.Key == HeroCopyCatalog.DefaultTargetKey)
                {
                    Assert.InRange(variant.HeadlineLead.En.Length, 1, MaxDefaultHeadlineLineLength);
                    Assert.InRange(variant.HeadlineLead.Th.Length, 1, MaxDefaultHeadlineLineLength);
                    Assert.InRange(variant.HeadlineAccent.En.Length, 1, MaxDefaultHeadlineLineLength);
                    Assert.InRange(variant.HeadlineAccent.Th.Length, 1, MaxDefaultHeadlineLineLength);
                }

                Assert.InRange(variant.Body.En.Length, 1, MaxBodyLength);
                Assert.InRange(variant.Body.Th.Length, 1, MaxBodyLength);
                Assert.InRange(variant.MetaDescription.En.Length, 1, MaxMetaDescriptionLength);
                Assert.InRange(variant.MetaDescription.Th.Length, 1, MaxMetaDescriptionLength);
                Assert.Contains("MALIEV", variant.MetaDescription.En, StringComparison.Ordinal);
                Assert.Contains("CAD", variant.MetaDescription.En, StringComparison.Ordinal);
                Assert.Contains("MALIEV", variant.MetaDescription.Th, StringComparison.Ordinal);
                Assert.Contains("CAD", variant.MetaDescription.Th, StringComparison.Ordinal);
            });
        }
    }

    /// <summary>
    /// Verifies Google Ads final URLs can select the matching hero-copy category.
    /// </summary>
    [Theory]
    [InlineData("https://www.maliev.com/?service=fdm-3d-printing", "fdm-3d-printing")]
    [InlineData("https://www.maliev.com/?service=aluminum-cnc-milling", "aluminum-cnc-milling")]
    [InlineData("https://www.maliev.com/?utm_term=FDM%203D%20printing%20service", "fdm-3d-printing")]
    [InlineData("https://www.maliev.com/?utm_term=3D%20printing%20near%20me", "3d-printing")]
    [InlineData("https://www.maliev.com/?utm_term=Resin%203D%20printing", "resin-3d-printing")]
    [InlineData("https://www.maliev.com/?utm_term=Aluminum%20CNC%20Milling", "aluminum-cnc-milling")]
    [InlineData("https://www.maliev.com/?utm_term=CNC%20shop%20near%20me", "cnc-machining")]
    [InlineData("https://www.maliev.com/?keyword=รับพิมพ์%203%20มิติ%20FDM", "fdm-3d-printing")]
    [InlineData("https://www.maliev.com/?keyword=กัดอลูมิเนียม%20CNC", "aluminum-cnc-milling")]
    [InlineData("https://www.maliev.com/?utm_term=ร้าน%20cnc%20ใกล้ฉัน", "cnc-machining")]
    public void ResolveTargetKey_MapsAdUrlToMatchingHeroCategory(string url, string expected)
    {
        Assert.Equal(expected, HeroCopyCatalog.ResolveTargetKey(url));
    }
}
