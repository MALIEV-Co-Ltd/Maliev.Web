using System.Security.Claims;
using Asp.Versioning;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// Customer-facing chatbot API.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("web/v{version:apiVersion}/chatbot")]
[AllowAnonymous]
public sealed class ChatbotController(
    ICustomerChatbotService chatbotService,
    CustomerAssistantHandoffCookie handoffCookie) : ControllerBase
{
    /// <summary>
    /// Sends a public website customer message to the MALIEV assistant.
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(CustomerChatbotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Send([FromBody] CustomerChatbotRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var response = await chatbotService.SendAsync(request, cancellationToken);
            if (response.SessionId.HasValue)
            {
                handoffCookie.Append(
                    Request,
                    Response,
                    response.SessionId.Value,
                    ResolveUserKey(),
                    response.Language,
                    User.Identity?.IsAuthenticated == true);
            }

            return Ok(response);
        }
        catch (ChatbotRateLimitException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails
            {
                Title = "MALIEV assistant is busy",
                Detail = ex.Message,
                Status = StatusCodes.Status429TooManyRequests
            });
        }
        catch (BackendUnavailableException ex)
        {
            return Problem(
                title: $"{ex.BackendName} unavailable",
                detail: "The MALIEV assistant is temporarily unavailable. Please try again or contact info@maliev.com.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private string? ResolveUserKey()
    {
        return User.FindFirstValue("customer_id")
            ?? User.FindFirstValue("customerId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name;
    }
}
