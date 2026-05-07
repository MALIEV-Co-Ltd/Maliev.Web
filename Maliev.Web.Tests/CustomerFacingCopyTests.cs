namespace Maliev.Web.Tests;

/// <summary>
/// Guards customer-facing pages against internal implementation wording.
/// </summary>
public sealed class CustomerFacingCopyTests
{
    /// <summary>
    /// Verifies customer-visible client files do not mention internal backend service names.
    /// </summary>
    [Fact]
    public void CustomerFacingClientFilesDoNotExposeBackendImplementationTerms()
    {
        var root = FindRepoRoot();
        var files = Directory.EnumerateFiles(Path.Combine(root, "Maliev.Web.Client"), "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var bannedTerms = new[]
        {
            "Web BFF",
            "catalog backend",
            "configured catalog backend",
            "ProjectNew workflow",
            "Backend contracts",
            "UploadService",
            "GeometryService",
            "PricingService",
            "OrderService",
            "PaymentService",
            "DeliveryService",
            "CustomerService",
            "MaterialService",
            "Shopify handle"
        };

        var violations = files
            .SelectMany(file => bannedTerms
                .Where(term => File.ReadAllText(file).Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(term => $"{Path.GetRelativePath(root, file)} contains {term}"))
            .ToList();

        Assert.Empty(violations);
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
