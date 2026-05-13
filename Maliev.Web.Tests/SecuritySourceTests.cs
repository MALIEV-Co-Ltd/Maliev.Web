namespace Maliev.Web.Tests;

/// <summary>
/// Source-level regression tests for public Web BFF security boundaries.
/// </summary>
public sealed class SecuritySourceTests
{
    /// <summary>
    /// Verifies customer address mutation endpoints check signed-in ownership before forwarding by address id.
    /// </summary>
    [Fact]
    public void AccountAddressMutationsVerifySignedInCustomerOwnsAddressBeforeForwarding()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AccountController.cs");

        Assert.Contains("private async Task<bool?> CustomerOwnsAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)", source);
        Assert.Contains("await customerClient.GetCustomerAddressesAsync(customerId, cancellationToken)", source);
        Assert.Contains("GetGuid(address, \"id\", \"Id\")", source);
        Assert.Contains("This address is not attached to your customer account.", source);

        AssertOwnershipCheckPrecedesForward(source, "UpdateAddress", "UpdateCustomerAddressAsync");
        AssertOwnershipCheckPrecedesForward(source, "DeleteAddress", "DeleteCustomerAddressAsync");
    }

    private static void AssertOwnershipCheckPrecedesForward(string source, string actionName, string clientCall)
    {
        var actionStart = source.IndexOf($"public async Task<IActionResult> {actionName}", StringComparison.Ordinal);
        Assert.True(actionStart >= 0, $"{actionName} action was not found.");

        var ownershipCheck = source.IndexOf("await CustomerOwnsAddressAsync(customerId.Value, addressId, cancellationToken)", actionStart, StringComparison.Ordinal);
        Assert.True(ownershipCheck > actionStart, $"{actionName} does not verify address ownership.");

        var forwardCall = source.IndexOf(clientCall, actionStart, StringComparison.Ordinal);
        Assert.True(forwardCall > actionStart, $"{actionName} does not forward to {clientCall}.");
        Assert.True(ownershipCheck < forwardCall, $"{actionName} forwards address id before verifying ownership.");
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
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
