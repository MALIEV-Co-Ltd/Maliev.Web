using System.ComponentModel.DataAnnotations;

namespace Maliev.Web.Shared.Account;

/// <summary>
/// Request to initiate an email address change for the signed-in customer.
/// </summary>
public sealed class EmailChangeRequest
{
    /// <summary>
    /// Gets or sets the new email address to verify.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;
}
