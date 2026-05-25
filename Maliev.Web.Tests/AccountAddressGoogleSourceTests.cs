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
        Assert.Contains("Country / region", page, StringComparison.Ordinal);
        Assert.Contains("GetAddressCountriesAsync", page, StringComparison.Ordinal);
        Assert.Contains("disabled=\"@ProvinceLocked\"", page, StringComparison.Ordinal);
        Assert.Contains("disabled=\"@PostalCodeLocked\"", page, StringComparison.Ordinal);

        Assert.Contains("Search for your location", picker, StringComparison.Ordinal);
        Assert.Contains("web/v1/address/google-config", picker, StringComparison.Ordinal);
        Assert.Contains("PlaceAutocompleteElement", script, StringComparison.Ordinal);
        Assert.Contains("gmp-select", script, StringComparison.Ordinal);
        Assert.Contains("options.includedRegionCodes = config.includedRegionCodes", script, StringComparison.Ordinal);
        Assert.DoesNotContain("[\"th\"]", script, StringComparison.Ordinal);
        Assert.DoesNotContain("region: \"th\"", script, StringComparison.Ordinal);
        Assert.Contains("GoogleMapPin", script, StringComparison.Ordinal);
        Assert.Contains("maliev-google-address-picker.js", app, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the driver note is treated as shipping-only delivery instructions after the address fields.
    /// </summary>
    [Fact]
    public void AccountAddresses_ShowsDriverNoteOnlyForShippingAfterAddressFields()
    {
        var page = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountAddresses.razor");

        Assert.Contains("@bind-Value=\"FormAddressType\"", page, StringComparison.Ordinal);
        Assert.Contains("@if (IsShippingAddress)", page, StringComparison.Ordinal);
        Assert.Contains("private bool IsShippingAddress", page, StringComparison.Ordinal);
        Assert.Contains("private string FormAddressType", page, StringComparison.Ordinal);
        Assert.Contains("_form.DriverNote = null;", page, StringComparison.Ordinal);

        var typeIndex = page.IndexOf("@bind-Value=\"FormAddressType\"", StringComparison.Ordinal);
        var contactIndex = page.IndexOf("Recipient Name", StringComparison.Ordinal);
        var countryIndex = page.IndexOf("Country / region", StringComparison.Ordinal);
        var postalIndex = page.IndexOf("Postal code", StringComparison.Ordinal);
        var noteIndex = page.IndexOf("Note to driver", StringComparison.Ordinal);
        var actionsIndex = page.IndexOf("account-form-actions", StringComparison.Ordinal);

        Assert.True(typeIndex >= 0);
        Assert.True(contactIndex > typeIndex);
        Assert.True(countryIndex > contactIndex);
        Assert.True(postalIndex > countryIndex);
        Assert.True(noteIndex > postalIndex);
        Assert.True(actionsIndex > noteIndex);
    }

    /// <summary>
    /// Verifies the default-address checkbox aligns with the address type select control, not the whole label row.
    /// </summary>
    [Fact]
    public void AccountAddresses_AlignsDefaultAddressWithTypeSelect()
    {
        var page = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountAddresses.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("class=\"checkbox-label address-default-checkbox\"", page, StringComparison.Ordinal);
        Assert.Contains(".address-default-checkbox", styles, StringComparison.Ordinal);
        Assert.Contains("align-self: end;", styles, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the Web BFF forwards and maps CustomerService address metadata.
    /// </summary>
    [Fact]
    public void AccountController_ForwardsGoogleAddressMetadata()
    {
        var accountController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AccountController.cs");
        var addressController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AddressController.cs");
        var boundaryClients = ReadRepoFile("Maliev.Web.Bff", "Clients", "CheckoutBoundaryClients.cs");
        var dtos = ReadRepoFile("Maliev.Web.Shared", "Account", "AccountDtos.cs");

        Assert.Contains("[Route(\"web/v{version:apiVersion}/address\")]", addressController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"google-config\")]", addressController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"countries\")]", addressController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"thai-locations\")]", addressController, StringComparison.Ordinal);
        Assert.Contains("GetCountriesAsync", addressController, StringComparison.Ordinal);
        Assert.Contains("SearchThaiLocationsAsync", addressController, StringComparison.Ordinal);
        Assert.Contains("GoogleMaps", addressController, StringComparison.Ordinal);
        Assert.Contains("IRegistryServiceClient", boundaryClients, StringComparison.Ordinal);
        Assert.Contains("/registry/v1/thai/addresses/autocomplete", boundaryClients, StringComparison.Ordinal);

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

        var googleDtos = ReadRepoFile("Maliev.Web.Shared", "Account", "GoogleAddressDtos.cs");
        var css = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("public string? CountryIso2", googleDtos, StringComparison.Ordinal);
        Assert.Contains("AddressCountryOptionDto", googleDtos, StringComparison.Ordinal);
        Assert.Contains("color-scheme: light", css, StringComparison.Ordinal);
        Assert.Contains("maliev-google-place-autocomplete", css, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the customer address form uses RegistryService Thai-location suggestions in addition to Google Maps.
    /// </summary>
    [Fact]
    public void AccountAddresses_UsesRegistryServiceThaiLocationSuggestions()
    {
        var page = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountAddresses.razor");
        var apiClient = ReadRepoFile("Maliev.Web.Client", "Services", "MalievApiClient.cs");
        var googleDtos = ReadRepoFile("Maliev.Web.Shared", "Account", "GoogleAddressDtos.cs");
        var css = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("Thai address registry", page, StringComparison.Ordinal);
        Assert.Contains("SearchRegistryLocationsAsync", page, StringComparison.Ordinal);
        Assert.Contains("ApplyRegistryLocation", page, StringComparison.Ordinal);
        Assert.Contains("AddressSource = \"RegistryThaiLocation\"", page, StringComparison.Ordinal);
        Assert.Contains("GetThaiAddressLocationsAsync", apiClient, StringComparison.Ordinal);
        Assert.Contains("web/v1/address/thai-locations", apiClient, StringComparison.Ordinal);
        Assert.Contains("ThaiAddressRegistryLocationDto", googleDtos, StringComparison.Ordinal);
        Assert.Contains("account-registry-suggestions", css, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies the map dialog handles missing map IDs and Google provider failures without exposing Google's raw degraded-map overlay.
    /// </summary>
    [Fact]
    public void GoogleAddressMap_HandlesMissingMapIdAndProviderFailure()
    {
        var picker = ReadRepoFile("Maliev.Web.Client", "Components", "GoogleAddressPicker.razor");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-google-address-picker.js");
        var css = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("NotifyGoogleAddressPickerStatus", picker, StringComparison.Ordinal);
        Assert.Contains("MapUnavailable", picker, StringComparison.Ordinal);
        Assert.Contains("createAddressMarker", script, StringComparison.Ordinal);
        Assert.Contains("google.maps.Marker", script, StringComparison.Ordinal);
        Assert.Contains("config.mapId", script, StringComparison.Ordinal);
        Assert.Contains("gm_authFailure", script, StringComparison.Ordinal);
        Assert.Contains("renderMapUnavailable", script, StringComparison.Ordinal);
        Assert.Contains("document.documentElement.lang", script, StringComparison.Ordinal);
        Assert.Contains("account-google-map-unavailable", css, StringComparison.Ordinal);
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
