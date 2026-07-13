using Asp.Versioning;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    /// Starts a public website customer chatbot session.
    /// </summary>
    [HttpPost("sessions")]
    [EnableRateLimiting(WebRateLimiterPolicies.ChatbotSession)]
    [RequestSizeLimit(4_096)]
    [ProducesResponseType(typeof(CustomerChatbotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> StartSession([FromBody] CustomerChatbotStartRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var response = await chatbotService.StartSessionAsync(request, cancellationToken);
            AppendHandoffCookie(response);
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

    /// <summary>
    /// Sends a public website customer message to the MALIEV assistant.
    /// </summary>
    [HttpPost("messages")]
    [EnableRateLimiting(WebRateLimiterPolicies.ChatbotMessage)]
    [RequestSizeLimit(8_192)]
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
            var response = await chatbotService.SendAsync(request, User, cancellationToken);
            AppendHandoffCookie(response);
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

    private void AppendHandoffCookie(CustomerChatbotResponse response)
    {
        if (!response.SessionId.HasValue)
        {
            return;
        }

        var callerContext = CustomerChatbotCallerContext.FromPrincipal(User);
        handoffCookie.Append(
            Request,
            Response,
            response.SessionId.Value,
            callerContext.CustomerId?.ToString("D"),
            response.Language,
            callerContext.IsAuthenticatedCustomer);
    }
}
