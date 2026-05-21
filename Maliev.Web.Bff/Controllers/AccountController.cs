using System.Security.Claims;
using System.Text.Json;
using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer account API for the public Web account area.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/account")]
public sealed class AccountController(ICustomerServiceClient customerClient, ICountryServiceClient countryClient) : ControllerBase
{
    /// <summary>Gets the current browser customer session.</summary>
    [HttpGet("session")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CustomerAccountSessionDto), StatusCodes.Status200OK)]
    public ActionResult<CustomerAccountSessionDto> GetSession()
    {
        return Ok(new CustomerAccountSessionDto
        {
            IsAuthenticated = User.Identity?.IsAuthenticated == true,
            PrincipalId = GetClaimGuid("principal_id", ClaimTypes.NameIdentifier),
            CustomerId = GetClaimGuid("customer_id"),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            DisplayName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
        });
    }

    /// <summary>Gets the signed-in customer profile from CustomerService.</summary>
    [HttpGet("profile")]
    [RequirePermission("customer.profile.read")]
    [ProducesResponseType(typeof(CustomerAccountProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var customerId = GetCurrentCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(AccountProblem("Customer session missing", "Sign in again so MALIEV can resolve your customer profile.", StatusCodes.Status401Unauthorized));
        }

        using var response = await customerClient.GetCustomerAsync(customerId.Value, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return DownstreamProblem(response, "Customer profile is temporarily unavailable.");
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        return Ok(MapProfile(document.RootElement, customerId.Value));
    }

    /// <summary>Updates the signed-in customer profile in CustomerService.</summary>
    [HttpPatch("profile")]
    [RequirePermission("customer.profile.write")]
    [ProducesResponseType(typeof(CustomerAccountProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateProfile([FromBody] CustomerAccountProfileUpdateRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(AccountProblem("Customer session missing", "Sign in again so MALIEV can update your profile.", StatusCodes.Status401Unauthorized));
        }

        using var response = await customerClient.UpdateCustomerAsync(customerId.Value, new
        {
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email,
            mobile = request.Mobile,
            preferredLanguage = NormalizeLanguage(request.PreferredLanguage),
            timezone = string.IsNullOrWhiteSpace(request.Timezone) ? "Asia/Bangkok" : request.Timezone,
            xmin = request.Version
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return DownstreamProblem(response, "Customer profile could not be updated.");
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        return Ok(MapProfile(document.RootElement, customerId.Value));
    }

    /// <summary>Gets the signed-in customer address book from CustomerService.</summary>
    [HttpGet("addresses")]
    [RequirePermission("customer.addresses.manage")]
    [ProducesResponseType(typeof(List<CustomerAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var customerId = GetCurrentCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(AccountProblem("Customer session missing", "Sign in again so MALIEV can resolve your address book.", StatusCodes.Status401Unauthorized));
        }

        using var response = await customerClient.GetCustomerAddressesAsync(customerId.Value, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return DownstreamProblem(response, "Customer addresses are temporarily unavailable.");
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        var addresses = document.RootElement.ValueKind == JsonValueKind.Array
            ? document.RootElement.EnumerateArray().Select(MapAddress).ToList()
            : [];
        return Ok(addresses);
    }

    /// <summary>Creates a signed-in customer address in CustomerService.</summary>
    [HttpPost("addresses")]
    [RequirePermission("customer.addresses.manage")]
    [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateAddress([FromBody] CustomerAddressUpsertRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(AccountProblem("Customer session missing", "Sign in again so MALIEV can add an address.", StatusCodes.Status401Unauthorized));
        }

        var countryId = await ResolveCountryIdAsync(request.CountryId, cancellationToken);
        using var response = await customerClient.CreateCustomerAddressAsync(new
        {
            ownerType = "Customer",
            ownerId = customerId.Value,
            type = request.Type,
            isDefault = request.IsDefault,
            placeLabel = request.PlaceLabel,
            placeLabelOther = request.PlaceLabelOther,
            addressLine1 = request.AddressLine1,
            addressLine2 = request.AddressLine2,
            addressLine3 = request.AddressLine3,
            district = request.District,
            city = request.City,
            stateProvince = request.StateProvince,
            postalCode = request.PostalCode,
            countryId,
            recipientName = request.RecipientName,
            recipientPhone = request.RecipientPhone,
            driverNote = request.DriverNote,
            addressSource = string.IsNullOrWhiteSpace(request.AddressSource) ? "Manual" : request.AddressSource,
            googlePlaceId = request.GooglePlaceId,
            formattedAddress = request.FormattedAddress,
            latitude = request.Latitude,
            longitude = request.Longitude
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return DownstreamProblem(response, "Customer address could not be added.");
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        return Ok(MapAddress(document.RootElement));
    }

    /// <summary>Updates a signed-in customer address in CustomerService.</summary>
    [HttpPatch("addresses/{addressId:guid}")]
    [RequirePermission("customer.addresses.manage")]
    [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] CustomerAddressUpsertRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(AccountProblem("Customer session missing", "Sign in again so MALIEV can update an address.", StatusCodes.Status401Unauthorized));
        }

        var ownsAddress = await CustomerOwnsAddressAsync(customerId.Value, addressId, cancellationToken);
        if (!ownsAddress.HasValue)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                AccountProblem(
                    "Customer addresses unavailable",
                    "MALIEV could not verify this address belongs to your account. Please try again later.",
                    StatusCodes.Status503ServiceUnavailable));
        }

        if (!ownsAddress.Value)
        {
            return NotFound(AccountProblem(
                "Address not found",
                "This address is not attached to your customer account.",
                StatusCodes.Status404NotFound));
        }

        using var response = await customerClient.UpdateCustomerAddressAsync(addressId, new
        {
            type = request.Type,
            isDefault = request.IsDefault,
            placeLabel = request.PlaceLabel,
            placeLabelOther = request.PlaceLabelOther,
            addressLine1 = request.AddressLine1,
            addressLine2 = request.AddressLine2,
            addressLine3 = request.AddressLine3,
            district = request.District,
            city = request.City,
            stateProvince = request.StateProvince,
            postalCode = request.PostalCode,
            countryId = request.CountryId == Guid.Empty ? (Guid?)null : request.CountryId,
            recipientName = request.RecipientName,
            recipientPhone = request.RecipientPhone,
            driverNote = request.DriverNote,
            addressSource = request.AddressSource,
            googlePlaceId = request.GooglePlaceId,
            formattedAddress = request.FormattedAddress,
            latitude = request.Latitude,
            longitude = request.Longitude,
            xmin = request.Version
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return DownstreamProblem(response, "Customer address could not be updated.");
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        return Ok(MapAddress(document.RootElement));
    }

    /// <summary>Deletes a signed-in customer address in CustomerService.</summary>
    [HttpDelete("addresses/{addressId:guid}")]
    [RequirePermission("customer.addresses.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteAddress(Guid addressId, [FromBody] CustomerAddressDeleteRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(AccountProblem("Customer session missing", "Sign in again so MALIEV can delete an address.", StatusCodes.Status401Unauthorized));
        }

        var ownsAddress = await CustomerOwnsAddressAsync(customerId.Value, addressId, cancellationToken);
        if (!ownsAddress.HasValue)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                AccountProblem(
                    "Customer addresses unavailable",
                    "MALIEV could not verify this address belongs to your account. Please try again later.",
                    StatusCodes.Status503ServiceUnavailable));
        }

        if (!ownsAddress.Value)
        {
            return NotFound(AccountProblem(
                "Address not found",
                "This address is not attached to your customer account.",
                StatusCodes.Status404NotFound));
        }

        using var response = await customerClient.DeleteCustomerAddressAsync(addressId, new { xmin = request.Version }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return DownstreamProblem(response, "Customer address could not be deleted.");
        }

        return NoContent();
    }

    /// <summary>Gets customer-facing shop order history metadata.</summary>
    [HttpGet("orders")]
    [RequirePermission("order.orders.read")]
    [ProducesResponseType(typeof(CustomerOrdersResponse), StatusCodes.Status200OK)]
    public ActionResult<CustomerOrdersResponse> GetOrders()
    {
        return Ok(new CustomerOrdersResponse
        {
            ShopOrders = [],
            ManufacturingOrdersUrl = SiteContent.QuoteOrdersUrl
        });
    }

    private Guid? GetCurrentCustomerId()
    {
        return GetClaimGuid("customer_id");
    }

    private async Task<bool?> CustomerOwnsAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)
    {
        using var response = await customerClient.GetCustomerAddressesAsync(customerId, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return document.RootElement
            .EnumerateArray()
            .Select(address => GetGuid(address, "id", "Id"))
            .Any(id => id == addressId);
    }

    private Guid? GetClaimGuid(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = User.FindFirstValue(claimType);
            if (Guid.TryParse(value, out var id))
            {
                return id;
            }
        }

        return null;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(content);
    }

    private async Task<Guid> ResolveCountryIdAsync(Guid requestedCountryId, CancellationToken cancellationToken)
    {
        if (requestedCountryId != Guid.Empty)
        {
            return requestedCountryId;
        }

        using var response = await countryClient.GetCountryByIso2Async("TH", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Guid.Empty;
        }

        using var document = await ReadJsonAsync(response, cancellationToken);
        return GetGuid(document.RootElement, "id", "Id") ?? Guid.Empty;
    }

    private CustomerAccountProfileDto MapProfile(JsonElement root, Guid fallbackCustomerId)
    {
        var firstName = GetString(root, "firstName", "FirstName");
        var lastName = GetString(root, "lastName", "LastName");
        var displayName = GetString(root, "name", "Name", "displayName", "DisplayName");
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = $"{firstName} {lastName}".Trim();
        }

        return new CustomerAccountProfileDto
        {
            CustomerId = GetGuid(root, "id", "Id") ?? fallbackCustomerId,
            PrincipalId = GetGuid(root, "principalId", "principal_id", "PrincipalId"),
            FirstName = firstName ?? string.Empty,
            LastName = lastName ?? string.Empty,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? User.FindFirstValue(ClaimTypes.Name) ?? string.Empty : displayName,
            Email = GetString(root, "email", "Email") ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Mobile = GetString(root, "mobile", "Mobile"),
            CompanyName = GetString(root, "companyName", "CompanyName"),
            Status = GetString(root, "status", "Status") ?? string.Empty,
            PreferredLanguage = GetString(root, "preferredLanguage", "PreferredLanguage") ?? string.Empty,
            Timezone = GetString(root, "timezone", "Timezone") ?? string.Empty,
            Version = GetUInt(root, "xmin", "Xmin", "version", "Version")
        };
    }

    private static CustomerAddressDto MapAddress(JsonElement root)
    {
        return new CustomerAddressDto
        {
            Id = GetGuid(root, "id", "Id") ?? Guid.Empty,
            Type = GetString(root, "type", "Type") ?? string.Empty,
            IsDefault = GetBool(root, "isDefault", "IsDefault"),
            PlaceLabel = GetString(root, "placeLabel", "PlaceLabel"),
            PlaceLabelOther = GetString(root, "placeLabelOther", "PlaceLabelOther"),
            AddressLine1 = GetString(root, "addressLine1", "AddressLine1") ?? string.Empty,
            AddressLine2 = GetString(root, "addressLine2", "AddressLine2"),
            AddressLine3 = GetString(root, "addressLine3", "AddressLine3"),
            District = GetString(root, "district", "District"),
            City = GetString(root, "city", "City") ?? string.Empty,
            StateProvince = GetString(root, "stateProvince", "StateProvince") ?? string.Empty,
            PostalCode = GetString(root, "postalCode", "PostalCode") ?? string.Empty,
            CountryId = GetGuid(root, "countryId", "CountryId") ?? Guid.Empty,
            RecipientName = GetString(root, "recipientName", "RecipientName"),
            RecipientPhone = GetString(root, "recipientPhone", "RecipientPhone"),
            DriverNote = GetString(root, "driverNote", "DriverNote"),
            AddressSource = GetString(root, "addressSource", "AddressSource") ?? "Manual",
            GooglePlaceId = GetString(root, "googlePlaceId", "GooglePlaceId"),
            FormattedAddress = GetString(root, "formattedAddress", "FormattedAddress"),
            Latitude = GetDecimal(root, "latitude", "Latitude"),
            Longitude = GetDecimal(root, "longitude", "Longitude"),
            Version = GetUInt(root, "xmin", "Xmin", "version", "Version")
        };
    }

    private ObjectResult DownstreamProblem(HttpResponseMessage response, string detail)
    {
        var status = response.StatusCode == System.Net.HttpStatusCode.NotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status503ServiceUnavailable;
        return StatusCode(status, AccountProblem("Account service unavailable", detail, status));
    }

    private static ProblemDetails AccountProblem(string title, string detail, int status)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status
        };
    }

    private static string NormalizeLanguage(string language)
    {
        return language.StartsWith("th", StringComparison.OrdinalIgnoreCase) ? "th" : "en";
    }

    private static string? GetString(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
        }

        return null;
    }

    private static Guid? GetGuid(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) &&
                value.ValueKind == JsonValueKind.String &&
                Guid.TryParse(value.GetString(), out var id))
            {
                return id;
            }
        }

        return null;
    }

    private static bool GetBool(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return value.GetBoolean();
            }
        }

        return false;
    }

    private static uint GetUInt(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (!root.TryGetProperty(name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && uint.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return 0;
    }

    private static decimal? GetDecimal(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) &&
                value.ValueKind == JsonValueKind.Number &&
                value.TryGetDecimal(out var number))
            {
                return number;
            }
        }

        return null;
    }
}
