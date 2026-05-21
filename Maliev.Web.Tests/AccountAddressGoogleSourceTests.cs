namespace Maliev.Web.Tests;

/// <summary>
/// Source-level tests for the public account Google address picker.
/// </summary>
public sealed class AccountAddressGoogleSourceTests
{
    /// <summary>
    /// Verifies the account address page exposes the Google picker and structured address fields.
    /// </summary>
    [Fact]
    public void AccountAddresses_UsesGooglePickerAndStructuredFields()
    {
        var page = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountAddresses.razor");
        var picker = ReadRepoFile("Maliev.Web.Client", "Components", "GoogleAddressPicker.razor");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-google-address-picker.js");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");

        Assert.Contains("GoogleAddressPicker", page, StringComparison.Ordinal);
        Assert.Contains("Address No./Moo/Soi/Road", page, StringComparison.Ordinal);
        Assert.Contains("Place name", page, StringComparison.Ordinal);
        Assert.Contains("Mobile Number", page, StringComparison.Ordinal);
        Assert.Contains("Note to driver", page, StringComparison.Ordinal);
        Assert.Contains("disabled=\"@ProvinceLocked\"", page, StringComparison.Ordinal);
        Assert.Contains("disabled=\"@PostalCodeLocked\"", page, StringComparison.Ordinal);

        Assert.Contains("Search for your location", picker, StringComparison.Ordinal);
        Assert.Contains("web/v1/address/google-config", picker, StringComparison.Ordinal);
        Assert.Contains("PlaceAutocompleteElement", script, StringComparison.Ordinal);
        Assert.Contains("gmp-select", script, StringComparison.Ordinal);
        Assert.Contains("GoogleMapPin", script, StringComparison.Ordinal);
        Assert.Contains("maliev-google-address-picker.js", app, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the Web BFF forwards and maps CustomerService address metadata.
    /// </summary>
    [Fact]
    public void AccountController_ForwardsGoogleAddressMetadata()
    {
        var accountController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AccountController.cs");
        var addressController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AddressController.cs");
        var dtos = ReadRepoFile("Maliev.Web.Shared", "Account", "AccountDtos.cs");

        Assert.Contains("[Route(\"web/v{version:apiVersion}/address\")]", addressController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"google-config\")]", addressController, StringComparison.Ordinal);
        Assert.Contains("GoogleMaps", addressController, StringComparison.Ordinal);

        foreach (var field in new[]
        {
            "placeLabel",
            "placeLabelOther",
            "driverNote",
            "addressSource",
            "googlePlaceId",
            "formattedAddress",
            "latitude",
            "longitude"
        })
        {
            Assert.Contains(field, accountController, StringComparison.Ordinal);
        }

        Assert.Contains("public string? PlaceLabel", dtos, StringComparison.Ordinal);
        Assert.Contains("public string? DriverNote", dtos, StringComparison.Ordinal);
        Assert.Contains("public string AddressSource", dtos, StringComparison.Ordinal);
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
