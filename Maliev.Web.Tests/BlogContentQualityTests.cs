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
        Assert.Equal(posts.Count, imageCredits.Count);
        Assert.Equal(imageCredits.Count, imageCredits.Values.Select(credit => credit.SourceUrl).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(imageCredits.Values, AssertApprovedImageCredit);

        foreach (var post in posts)
        {
            Assert.True(post.Summary.En.Length >= 55, $"{post.Slug} needs a stronger English meta summary.");
            Assert.True(post.Summary.Th.Length >= 30, $"{post.Slug} needs a stronger Thai meta summary.");
            Assert.True(post.Sections.Count >= 4, $"{post.Slug} needs at least four article sections.");
            Assert.True(post.Takeaways.Count >= 3, $"{post.Slug} needs at least three actionable takeaways.");
            Assert.True(CountEnglishWords(post) >= 360, $"{post.Slug} is still too thin for an SEO article.");
            Assert.DoesNotContain(post.Sections, section => section.Title.En == "Why this topic matters");
            Assert.True(imageCredits.ContainsKey(post.Slug), $"{post.Slug} is missing researched image credit metadata.");
            sectionTitleSequences.Add(string.Join(" | ", post.Sections.Select(section => section.Title.En)));

            foreach (var section in post.Sections)
            {
                Assert.True(section.Body.En.Length >= 180, $"{post.Slug}/{section.Title.En} needs a richer English body.");
                Assert.True(section.Body.Th.Length >= 80, $"{post.Slug}/{section.Title.En} needs a richer Thai body.");
            }

            var imageUrl = SiteContent.ResolveBlogImageUrl(post);
            Assert.Equal($"/images/blog/{post.Slug}.jpg", imageUrl);

            var imagePath = Path.Combine(
                root,
                "Maliev.Web.Bff",
                "wwwroot",
                imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(imagePath), $"{post.Slug} is missing image asset {imageUrl}.");
            Assert.False(File.Exists(Path.ChangeExtension(imagePath, ".svg")), $"{post.Slug} still has a legacy SVG blog image.");

            var imageBytes = File.ReadAllBytes(imagePath);
            Assert.True(imageBytes.Length >= 60_000, $"{post.Slug} image asset is too small to be useful.");
            Assert.True(IsJpeg(imageBytes), $"{post.Slug} must use an actual JPEG image, not SVG or HTML.");

            var (width, height) = ReadJpegDimensions(imageBytes);
            Assert.True(width >= 1000, $"{post.Slug} image width is too small.");
            Assert.True(height >= 600, $"{post.Slug} image height is too small.");
            Assert.True(imageHashes.Add(Convert.ToHexString(SHA256.HashData(imageBytes))), $"{post.Slug} reuses another blog image file.");
        }

        Assert.True(sectionTitleSequences.Count >= 8, "Blog posts still reuse too many generic article structures.");
    }

    private static IReadOnlyDictionary<string, BlogImageCredit> ReadImageCredits(string root)
    {
        var creditsPath = Path.Combine(root, "Maliev.Web.Bff", "wwwroot", "images", "blog", "image-credits.json");
        Assert.True(File.Exists(creditsPath), "Blog image source metadata is missing.");

        var credits = JsonSerializer.Deserialize<IReadOnlyList<BlogImageCredit>>(
            File.ReadAllText(creditsPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(credits);

        return credits.ToDictionary(credit => credit.Slug, StringComparer.OrdinalIgnoreCase);
    }

    private static void AssertApprovedImageCredit(BlogImageCredit credit)
    {
        Assert.False(string.IsNullOrWhiteSpace(credit.Slug), "Image credit is missing a slug.");
        Assert.False(string.IsNullOrWhiteSpace(credit.SourceTitle), $"{credit.Slug} is missing a source title.");
        Assert.False(string.IsNullOrWhiteSpace(credit.SourceUrl), $"{credit.Slug} is missing a source URL.");
        Assert.False(string.IsNullOrWhiteSpace(credit.AssetUrl), $"{credit.Slug} is missing an asset URL.");
        Assert.False(string.IsNullOrWhiteSpace(credit.LandingUrl), $"{credit.Slug} is missing a source landing URL.");
        Assert.Contains(credit.License, ApprovedImageLicenses, StringComparer.OrdinalIgnoreCase);
        Assert.StartsWith("https://upload.wikimedia.org/", credit.SourceUrl, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("https://commons.wikimedia.org/wiki/File:", credit.LandingUrl, StringComparison.OrdinalIgnoreCase);

        foreach (var rejectedTerm in RejectedImageSourceTerms)
        {
            Assert.DoesNotContain(rejectedTerm, credit.SourceTitle, StringComparison.OrdinalIgnoreCase);
        }
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

    private static readonly string[] ApprovedImageLicenses = ["CC0", "Public domain"];

    private static readonly string[] RejectedImageSourceTerms =
    [
        "Atlas Van der Hagen",
        "Battlefield",
        "Bicycle Sink",
        "D-Link",
        "Gas Station",
        "Glaubersalz",
        "Government Marble",
        "Locomotive crank",
        "Mercedes SLS",
        "Old dusty",
        "Optical Fabrication Lab"
    ];

    private sealed record BlogImageCredit(
        string Slug,
        string SourceTitle,
        string License,
        string SourceUrl,
        string AssetUrl,
        string LandingUrl);
}
