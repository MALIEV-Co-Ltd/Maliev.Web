using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Maliev.Web.Shared.Account;
using Maliev.Web.Shared.Chatbot;
using Maliev.Web.Shared.Commerce;
using Maliev.Web.Shared.Contact;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Client.Services;

internal sealed class MalievApiClient(HttpClient httpClient)
{
    internal async Task<IReadOnlyList<ProductCollectionDto>> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<List<ProductCollectionDto>>("web/v1/catalog/collections", cancellationToken) ?? [];
    }

    internal async Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? collection = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(collection)
            ? "web/v1/catalog/products"
            : $"web/v1/catalog/products?collection={Uri.EscapeDataString(collection)}";
        return await GetJsonAsync<List<ProductSummaryDto>>(path, cancellationToken) ?? [];
    }

    internal async Task<ProductDetailDto?> GetProductAsync(string handle, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<ProductDetailDto>($"web/v1/catalog/products/{Uri.EscapeDataString(handle)}", cancellationToken);
    }

    internal async Task<QuoteReferenceDataDto> GetQuoteReferenceDataAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<QuoteReferenceDataDto>("web/v1/quote/reference-data", cancellationToken) ?? new QuoteReferenceDataDto();
    }

    internal async Task<QuoteEstimateResponse> EstimateQuoteAsync(QuoteEstimateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/quote/estimate", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<QuoteEstimateResponse>(cancellationToken) ?? new QuoteEstimateResponse();
    }

    internal async Task<WebUploadInitiationResponse> InitiateUploadAsync(WebUploadInitiationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/quote/uploads/resumable", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<WebUploadInitiationResponse>(cancellationToken) ?? new WebUploadInitiationResponse();
    }

    internal async Task<WebUploadCompleteResponse> UploadFileAsync(IBrowserFile file, WebUploadInitiationResponse session, CancellationToken cancellationToken = default)
    {
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        await using var stream = file.OpenReadStream(10L * 1024L * 1024L * 1024L, cancellationToken);
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Headers.ContentLength = file.Size;
        content.Headers.ContentRange = new ContentRangeHeaderValue(0, file.Size - 1, file.Size);

        var response = await httpClient.PutAsync(session.ProxyUploadUrl.TrimStart('/'), content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<WebUploadCompleteResponse>(cancellationToken) ?? new WebUploadCompleteResponse();
    }

    internal async Task<WebAnalysisStatusResponse> GetAnalysisStatusAsync(string uploadId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<WebAnalysisStatusResponse>($"web/v1/quote/uploads/{Uri.EscapeDataString(uploadId)}/analysis-status", cancellationToken) ?? new WebAnalysisStatusResponse();
    }

    internal async Task<CheckoutDraftResponse> CreateCheckoutDraftAsync(CheckoutDraftRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/checkout/draft", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CheckoutDraftResponse>(cancellationToken) ?? new CheckoutDraftResponse();
    }

    internal async Task<CustomerAccountSessionDto> GetAccountSessionAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<CustomerAccountSessionDto>("web/v1/account/session", cancellationToken) ?? new CustomerAccountSessionDto();
    }

    internal async Task<CustomerAccountProfileDto> GetAccountProfileAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<CustomerAccountProfileDto>("web/v1/account/profile", cancellationToken) ?? new CustomerAccountProfileDto();
    }

    internal async Task<CustomerAccountProfileDto> UpdateAccountProfileAsync(CustomerAccountProfileUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync("web/v1/account/profile", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CustomerAccountProfileDto>(cancellationToken) ?? new CustomerAccountProfileDto();
    }

    internal async Task<IReadOnlyList<CustomerAddressDto>> GetAccountAddressesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<List<CustomerAddressDto>>("web/v1/account/addresses", cancellationToken) ?? [];
    }

    internal async Task<CustomerAddressDto> CreateAccountAddressAsync(CustomerAddressUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/account/addresses", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CustomerAddressDto>(cancellationToken) ?? new CustomerAddressDto();
    }

    internal async Task<CustomerAddressDto> UpdateAccountAddressAsync(Guid addressId, CustomerAddressUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync($"web/v1/account/addresses/{addressId}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CustomerAddressDto>(cancellationToken) ?? new CustomerAddressDto();
    }

    internal async Task DeleteAccountAddressAsync(Guid addressId, uint version, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"web/v1/account/addresses/{addressId}")
        {
            Content = JsonContent.Create(new CustomerAddressDeleteRequest { Version = version })
        };
        var response = await httpClient.SendAsync(message, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    internal async Task<CustomerOrdersResponse> GetAccountOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<CustomerOrdersResponse>("web/v1/account/orders", cancellationToken) ?? new CustomerOrdersResponse();
    }

    internal async Task<ContactMessageResponse> SubmitContactMessageAsync(ContactMessageRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/contact/messages", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ContactMessageResponse>(cancellationToken) ?? new ContactMessageResponse(string.Empty, "Received");
    }

    internal async Task<CustomerChatbotResponse> SendChatbotMessageAsync(CustomerChatbotRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/chatbot/messages", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CustomerChatbotResponse>(cancellationToken) ?? new CustomerChatbotResponse();
    }

    internal async Task<CustomerChatbotResponse> StartChatbotSessionAsync(CustomerChatbotStartRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("web/v1/chatbot/sessions", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CustomerChatbotResponse>(cancellationToken) ?? new CustomerChatbotResponse();
    }

    private async Task<T?> GetJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ApiProblemDetails? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken);
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (JsonException)
        {
        }

        throw new MalievApiException(response.StatusCode, problem?.Detail ?? problem?.Title ?? response.ReasonPhrase ?? "Request failed.");
    }
}

internal sealed class MalievApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    internal HttpStatusCode StatusCode { get; } = statusCode;
}

internal sealed class ApiProblemDetails
{
    public string? Title { get; set; }

    public string? Detail { get; set; }
}
