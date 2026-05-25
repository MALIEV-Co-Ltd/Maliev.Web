namespace Maliev.Web.Shared.Security;

/// <summary>
/// Authorization policy names shared by the Web BFF and customer-facing components.
/// </summary>
public static class WebAuthorizationPolicies
{
    /// <summary>
    /// Requires an authenticated customer cookie with a valid customer identifier claim.
    /// </summary>
    public const string CustomerAccount = "CustomerAccount";
}
