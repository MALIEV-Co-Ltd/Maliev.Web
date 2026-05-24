using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

using Maliev.Web.Client.Content;

namespace Maliev.Web.Tests;

/// <summary>
/// Guards public blog posts against thin content and repeated generic images.
/// </summary>
public sealed class BlogContentQualityTests
{
    /// <summary>
    /// Verifies every blog post has useful article depth and a matching local image asset.
    /// </summary>
    [Fact]
    public void BlogPosts_HaveInformativeContentAndUniqueImages()
    {
        var root = FindRepoRoot();
        var posts = SiteContent.BlogPosts;
        var imageUrls = posts.Select(SiteContent.ResolveBlogImageUrl).ToArray();
        var imageCredits = ReadImageCredits(root);
        var imageHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sectionTitleSequences = new HashSet<string>(StringComparer.Ordinal);

        Assert.True(posts.Count >= 50, $"Expected at least 50 blog posts, found {posts.Count}.");
        Assert.Equal(posts.Count, posts.Select(post => post.Slug).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(posts.Count, imageUrls.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(posts.Count * 4, imageCredits.Count);
        Assert.Equal(imageCredits.Count, imageCredits.Select(credit => credit.ImageUrl).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(imageCredits, AssertApprovedImageCredit);
        Assert.Equal(4, imageCredits.Count(credit => credit.SourceKind == "ai-generated-photo"));
        Assert.Equal(posts.Count * 4 - 4, imageCredits.Count(credit => credit.SourceKind == "openverse-photo"));

        var clearResinPost = Assert.Single(posts, post => post.Slug == "clear-resin-expectations");
        Assert.Contains(clearResinPost.Sections, section => section.Image?.Caption.En.Contains("Support marks affect optical surfaces", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Equal(4, imageCredits.Count(credit => credit.Slug == clearResinPost.Slug && credit.SourceKind == "ai-generated-photo"));

        foreach (var post in posts)
        {
            Assert.True(post.Summary.En.Length >= 55, $"{post.Slug} needs a stronger English meta summary.");
            Assert.True(post.Summary.Th.Length >= 30, $"{post.Slug} needs a stronger Thai meta summary.");
            Assert.True(post.Sections.Count >= 4, $"{post.Slug} needs at least four article sections.");
            Assert.True(post.Takeaways.Count >= 3, $"{post.Slug} needs at least three actionable takeaways.");
            Assert.True(CountEnglishWords(post) >= 360, $"{post.Slug} is still too thin for an SEO article.");
            Assert.DoesNotContain(post.Sections, section => section.Title.En == "Why this topic matters");
            Assert.Contains(imageCredits, credit => credit.Slug == post.Slug);
            sectionTitleSequences.Add(string.Join(" | ", post.Sections.Select(section => section.Title.En)));

            foreach (var section in post.Sections)
            {
                Assert.True(section.Body.En.Length >= 180, $"{post.Slug}/{section.Title.En} needs a richer English body.");
                Assert.True(section.Body.Th.Length >= 80, $"{post.Slug}/{section.Title.En} needs a richer Thai body.");
            }

            var imageUrl = SiteContent.ResolveBlogImageUrl(post);
            Assert.Equal($"/images/blog/{post.Slug}.jpg", imageUrl);
            Assert.Contains(imageCredits, credit => credit.Slug == post.Slug && credit.Role == "hero" && credit.ImageUrl == imageUrl);
            AssertImageAsset(root, post.Slug, imageUrl, "hero image", imageHashes);

            var articleImages = post.Sections
                .Where(section => section.Image is not null)
                .Select(section => section.Image!)
                .ToArray();
            Assert.InRange(articleImages.Length, 2, 4);
            Assert.Equal(3, articleImages.Length);

            for (var index = 0; index < articleImages.Length; index++)
            {
                var articleImage = articleImages[index];
                var expectedUrl = $"/images/blog/articles/{post.Slug}-{index + 1:00}.jpg";
                Assert.Equal(expectedUrl, articleImage.Url);
                Assert.True(articleImage.Alt.En.Length >= 30, $"{post.Slug} article image {index + 1} needs stronger English alt text.");
                Assert.True(articleImage.Alt.Th.Length >= 20, $"{post.Slug} article image {index + 1} needs stronger Thai alt text.");
                Assert.True(articleImage.Caption.En.Length >= 30, $"{post.Slug} article image {index + 1} needs a useful English caption.");
                Assert.True(articleImage.Caption.Th.Length >= 20, $"{post.Slug} article image {index + 1} needs a useful Thai caption.");
                Assert.Contains(imageCredits, credit => credit.Slug == post.Slug && credit.Role == $"article-{index + 1:00}" && credit.ImageUrl == expectedUrl);
                AssertImageAsset(root, post.Slug, expectedUrl, $"article image {index + 1}", imageHashes);
            }
        }

        Assert.Equal(posts.Count * 4, imageHashes.Count);
        Assert.True(sectionTitleSequences.Count >= 8, "Blog posts still reuse too many generic article structures.");
    }

    private static IReadOnlyList<BlogImageCredit> ReadImageCredits(string root)
    {
        var creditsPath = Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "blog", "image-credits.json");
        Assert.True(File.Exists(creditsPath), "Blog image source metadata is missing.");

        var credits = JsonSerializer.Deserialize<IReadOnlyList<BlogImageCredit>>(
            File.ReadAllText(creditsPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(credits);

        return credits;
    }

    private static void AssertApprovedImageCredit(BlogImageCredit credit)
    {
        Assert.False(string.IsNullOrWhiteSpace(credit.Slug), "Image credit is missing a slug.");
        Assert.False(string.IsNullOrWhiteSpace(credit.Role), $"{credit.Slug} is missing an image role.");
        Assert.False(string.IsNullOrWhiteSpace(credit.ImageUrl), $"{credit.Slug} is missing a local image URL.");
        Assert.Contains(credit.SourceKind, ApprovedImageSourceKinds, StringComparer.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(credit.SourceTitle), $"{credit.Slug} is missing a source title.");
        Assert.False(string.IsNullOrWhiteSpace(credit.SourceUrl), $"{credit.Slug} is missing a source URL.");
        Assert.False(string.IsNullOrWhiteSpace(credit.AssetUrl), $"{credit.Slug} is missing an asset URL.");
        Assert.False(string.IsNullOrWhiteSpace(credit.LandingUrl), $"{credit.Slug} is missing a source landing URL.");
        Assert.False(string.IsNullOrWhiteSpace(credit.Provider), $"{credit.Slug} is missing an image provider.");
        Assert.False(string.IsNullOrWhiteSpace(credit.License), $"{credit.Slug} is missing image license metadata.");
        Assert.StartsWith("/images/blog/", credit.ImageUrl, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(credit.ImageUrl, credit.AssetUrl);
        Assert.Equal(1200, credit.LocalWidth);
        Assert.Equal(750, credit.LocalHeight);
        Assert.True(credit.SourceWidth.GetValueOrDefault() >= 0, $"{credit.Slug} has invalid source width metadata.");
        Assert.True(credit.SourceHeight.GetValueOrDefault() >= 0, $"{credit.Slug} has invalid source height metadata.");

        foreach (var rejectedTerm in RejectedImageSourceTerms)
        {
            Assert.DoesNotContain(rejectedTerm, credit.SourceTitle, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertImageAsset(
        string root,
        string slug,
        string imageUrl,
        string label,
        ISet<string> imageHashes)
    {
        var imagePath = Path.Combine(
            root,
            "Maliev.Web.Bff",
            "wwwroot",
            imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(imagePath), $"{slug} is missing {label} asset {imageUrl}.");
        Assert.False(File.Exists(Path.ChangeExtension(imagePath, ".svg")), $"{slug} {label} still has a legacy SVG asset.");

        var imageBytes = File.ReadAllBytes(imagePath);
        Assert.True(imageBytes.Length >= 40_000, $"{slug} {label} asset is too small to be useful.");
        Assert.True(IsJpeg(imageBytes), $"{slug} {label} must use an actual JPEG image, not SVG or HTML.");

        var (width, height) = ReadJpegDimensions(imageBytes);
        Assert.True(width >= 1000, $"{slug} {label} image width is too small.");
        Assert.True(height >= 600, $"{slug} {label} image height is too small.");
        Assert.True(imageHashes.Add(Convert.ToHexString(SHA256.HashData(imageBytes))), $"{slug} {label} reuses another blog image file.");
    }

    private static int CountEnglishWords(BlogPostContent post)
    {
        var text = string.Join(
            ' ',
            post.Title.En,
            post.Summary.En,
            post.Category.En,
            string.Join(' ', post.Sections.Select(section => section.Title.En)),
            string.Join(' ', post.Sections.Select(section => section.Body.En)),
            string.Join(' ', post.Sections.SelectMany(section => section.Items).Select(item => item.En)),
            string.Join(' ', post.Takeaways.Select(item => item.En)));

        return Regex.Matches(text, @"[A-Za-z0-9]+(?:[-'][A-Za-z0-9]+)?").Count;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }

    private static bool IsJpeg(byte[] bytes)
    {
        return bytes.Length >= 4
            && bytes[0] == 0xFF
            && bytes[1] == 0xD8
            && bytes[^2] == 0xFF
            && bytes[^1] == 0xD9;
    }

    private static (int Width, int Height) ReadJpegDimensions(byte[] bytes)
    {
        var index = 2;

        while (index + 9 < bytes.Length)
        {
            if (bytes[index] != 0xFF)
            {
                index++;
                continue;
            }

            while (index < bytes.Length && bytes[index] == 0xFF)
            {
                index++;
            }

            if (index >= bytes.Length)
            {
                break;
            }

            var marker = bytes[index++];
            if (marker is 0xD9 or 0xDA)
            {
                break;
            }

            if (index + 2 >= bytes.Length)
            {
                break;
            }

            var segmentLength = (bytes[index] << 8) + bytes[index + 1];
            if (segmentLength < 2 || index + segmentLength > bytes.Length)
            {
                break;
            }

            if (marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC)
            {
                var height = (bytes[index + 3] << 8) + bytes[index + 4];
                var width = (bytes[index + 5] << 8) + bytes[index + 6];
                return (width, height);
            }

            index += segmentLength;
        }

        throw new InvalidDataException("Could not read JPEG dimensions.");
    }

    private static readonly string[] ApprovedImageSourceKinds = ["openverse-photo", "ai-generated-photo"];

    private static readonly string[] RejectedImageSourceTerms =
    [
        "Atlas Van der Hagen",
        "Battlefield",
        "Bicycle Sink",
        "D-Link",
        "Gas Station",
        "Glaubersalz",
        "Government Marble",
        "Hubble",
        "Locomotive crank",
        "Mercedes SLS",
        "NASA",
        "Old dusty",
        "Optical Fabrication Lab",
        "schematic",
        "spacecraft",
        "telescope",
        "Webb"
    ];

    private sealed record BlogImageCredit(
        string Slug,
        string Role,
        string ImageUrl,
        string SourceKind,
        string SourceTitle,
        string Provider,
        string License,
        string SourceUrl,
        string AssetUrl,
        string LandingUrl,
        int? SourceWidth,
        int? SourceHeight,
        int LocalWidth,
        int LocalHeight);
}
