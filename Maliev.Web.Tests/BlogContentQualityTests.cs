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

        Assert.True(posts.Count >= 50, $"Expected at least 50 blog posts, found {posts.Count}.");
        Assert.Equal(posts.Count, posts.Select(post => post.Slug).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(posts.Count, imageUrls.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        foreach (var post in posts)
        {
            Assert.True(post.Summary.En.Length >= 55, $"{post.Slug} needs a stronger English meta summary.");
            Assert.True(post.Summary.Th.Length >= 30, $"{post.Slug} needs a stronger Thai meta summary.");
            Assert.True(post.Sections.Count >= 4, $"{post.Slug} needs at least four article sections.");
            Assert.True(post.Takeaways.Count >= 3, $"{post.Slug} needs at least three actionable takeaways.");
            Assert.True(CountEnglishWords(post) >= 220, $"{post.Slug} is still too thin for an SEO article.");

            foreach (var section in post.Sections)
            {
                Assert.True(section.Body.En.Length >= 180, $"{post.Slug}/{section.Title.En} needs a richer English body.");
                Assert.True(section.Body.Th.Length >= 80, $"{post.Slug}/{section.Title.En} needs a richer Thai body.");
            }

            var imageUrl = SiteContent.ResolveBlogImageUrl(post);
            Assert.Equal($"/images/blog/{post.Slug}.svg", imageUrl);

            var imagePath = Path.Combine(
                root,
                "Maliev.Web.Bff",
                "wwwroot",
                imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(imagePath), $"{post.Slug} is missing image asset {imageUrl}.");
            Assert.True(new FileInfo(imagePath).Length >= 2000, $"{post.Slug} image asset is too small to be useful.");
            var imageSource = File.ReadAllText(imagePath);
            Assert.Contains("<linearGradient", imageSource, StringComparison.Ordinal);
            Assert.Contains("MALIEV PRACTICAL NOTE", imageSource, StringComparison.Ordinal);
            Assert.Contains("DFM", imageSource, StringComparison.Ordinal);
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
}
