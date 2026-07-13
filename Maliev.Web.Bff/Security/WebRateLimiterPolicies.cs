namespace Maliev.Web.Bff.Security;

/// <summary>Named ingress budgets for anonymous, cost-bearing Web endpoints.</summary>
internal static class WebRateLimiterPolicies
{
    internal const string ChatbotSession = "chatbot-session";
    internal const string ChatbotMessage = "chatbot-message";
    internal const string Contact = "contact";
    internal const string QuoteEstimate = "quote-estimate";
    internal const string QuoteReference = "quote-reference";
    internal const string UploadInitiate = "upload-initiate";
    internal const string UploadStream = "upload-stream";
    internal const string UploadFinalize = "upload-finalize";
    internal const string UploadHandoff = "upload-handoff";
    internal const string UploadStatus = "upload-status";
    internal const string Checkout = "checkout";
    internal const string ShippingRate = "shipping-rate";
    internal const string ShippingRead = "shipping-read";
}
