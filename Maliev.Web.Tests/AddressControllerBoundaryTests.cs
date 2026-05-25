using System.Net;
using System.Net.Http.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Controllers;
using Maliev.Web.Shared.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for browser-safe address support endpoints.
/// </summary>
public sealed class AddressControllerBoundaryTests
{
    /// <summary>
    /// Verifies Places autocomplete is global unless configuration explicitly restricts it.
    /// </summary>
    [Fact]
    public void GetGoogleConfig_NoConfiguredRegion_ReturnsUnrestrictedAutocomplete()
    {
        var controller = new AddressController(new ConfigurationBuilder().Build(), new FakeCountryServiceClient(), new FakeRegistryServiceClient());

        var result = Assert.IsType<OkObjectResult>(controller.GetGoogleConfig().Result);
        var config = Assert.IsType<GoogleAddressConfigResponse>(result.Value);

        Assert.Empty(config.IncludedRegionCodes);
    }

    /// <summary>
    /// Verifies country options are read from CountryService and Thailand is kept first.
    /// </summary>
    [Fact]
    public async Task GetCountryOptions_ReturnsCountryServiceOptions()
    {
        var thailandId = Guid.Parse("60f7ba70-8e45-49a5-b8a3-9d78ceda60c9");
        var singaporeId = Guid.Parse("8c088f6c-7144-4c5f-ae30-c6fbb629ab14");
        var controller = new AddressController(new ConfigurationBuilder().Build(), new FakeCountryServiceClient
        {
            CountriesResponse = new
            {
                data = new[]
                {
                    new { id = singaporeId, iso2 = "SG", name = "Singapore" },
                    new { id = thailandId, iso2 = "TH", name = "Thailand" }
                }
            }
        }, new FakeRegistryServiceClient());

        var action = await controller.GetCountryOptions(CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var countries = Assert.IsType<List<AddressCountryOptionDto>>(result.Value);
        Assert.Collection(
            countries,
            thailand =>
            {
                Assert.Equal(thailandId, thailand.Id);
                Assert.Equal("TH", thailand.Iso2);
            },
            singapore =>
            {
                Assert.Equal(singaporeId, singapore.Id);
                Assert.Equal("SG", singapore.Iso2);
            });
    }

    /// <summary>
    /// Verifies Thai address autocomplete maps RegistryService administrative locations for customer entry.
    /// </summary>
    [Fact]
    public async Task SearchThaiLocations_ReturnsRegistryServiceOptions()
    {
        var locationId = Guid.Parse("1f54cb83-cfa2-4e4c-baa8-e902e458019b");
        var controller = new AddressController(
            new ConfigurationBuilder().Build(),
            new FakeCountryServiceClient(),
            new FakeRegistryServiceClient
            {
                LocationsResponse = new
                {
                    data = new[]
                    {
                        new
                        {
                            id = locationId,
                            postalCode = "11120",
                            subDistrictTh = "คลองข่อย",
                            districtTh = "ปากเกร็ด",
                            provinceTh = "นนทบุรี",
                            subDistrictEn = "Khlong Khoi",
                            districtEn = "Pak Kret",
                            provinceEn = "Nonthaburi"
                        }
                    }
                }
            });

        var action = await controller.SearchThaiLocationsAsync("pak", cancellationToken: CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var locations = Assert.IsType<List<ThaiAddressRegistryLocationDto>>(result.Value);
        var location = Assert.Single(locations);
        Assert.Equal(locationId, location.Id);
        Assert.Equal("11120", location.PostalCode);
        Assert.Equal("คลองข่อย", location.SubDistrictTh);
        Assert.Equal("Pak Kret", location.DistrictEn);
    }

    private sealed class FakeCountryServiceClient : ICountryServiceClient
    {
        public object? CountriesResponse { get; init; }

        public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> GetCountriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(CountriesResponse ?? new { data = Array.Empty<object>() })
            });
        }
    }

    private sealed class FakeRegistryServiceClient : IRegistryServiceClient
    {
        public object? LocationsResponse { get; init; }

        public Task<HttpResponseMessage> SearchThaiLocationsAsync(string query, int limit, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(LocationsResponse ?? new { data = Array.Empty<object>() })
            });
        }
    }
}
