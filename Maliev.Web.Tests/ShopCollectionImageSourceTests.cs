namespace Maliev.Web.Tests;

/// <summary>
/// Source-level shop collection image regression tests.
/// </summary>
public sealed class ShopCollectionImageSourceTests
{
    /// <summary>
    /// Verifies the shop collection rail renders storefront collection images.
    /// </summary>
    [Fact]
    public void ShopRendersCollectionImages()
    {
        var shop = ReadRepoFile("Maliev.Web.Client", "Pages", "Shop.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var catalogController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "CatalogController.cs");
        var catalogService = ReadRepoFile("Maliev.Web.Bff", "Services", "CommerceCatalogService.cs");

        Assert.Contains("shop-collection-card", shop, StringComparison.Ordinal);
        Assert.Contains("collection.ImageUrl", shop, StringComparison.Ordinal);
        Assert.Contains("collection.ImageAltText", shop, StringComparison.Ordinal);
        Assert.Contains(".shop-collection-card", styles, StringComparison.Ordinal);
        Assert.Contains(".shop-collection-image", styles, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"collections/{slug}/image\")]", catalogController, StringComparison.Ordinal);
        Assert.Contains("GetCollectionImageRedirectUrlAsync", catalogService, StringComparison.Ordinal);
        Assert.Contains("api/v1/commerce/collections/media/", catalogService, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepositoryRoot()
    {
        foreach (var startDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(startDirectory);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
                {
                    return directory.FullName;
                }

                var siblingCandidate = Path.Combine(directory.FullName, "Maliev.Web");
                if (File.Exists(Path.Combine(siblingCandidate, "Maliev.Web.slnx")))
                {
                    return siblingCandidate;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not find Maliev.Web repository root.");
    }
}
