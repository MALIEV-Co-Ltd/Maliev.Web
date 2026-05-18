namespace Maliev.Web.Bff.Services;

internal sealed class BackendUnavailableException : Exception
{
    internal BackendUnavailableException(string backendName, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        BackendName = backendName;
    }

    internal string BackendName { get; }
}

internal sealed class QuoteNotReadyException : Exception
{
    internal QuoteNotReadyException(string message)
        : base(message)
    {
    }
}

internal sealed class CheckoutRequiresSignInException : Exception
{
    internal CheckoutRequiresSignInException()
        : base("Sign in is required before checkout can continue.")
    {
    }
}

internal sealed class ChatbotRateLimitException : Exception
{
    internal ChatbotRateLimitException(string message)
        : base(message)
    {
    }
}

internal sealed class ChatbotSessionUnavailableException : Exception
{
    internal ChatbotSessionUnavailableException(string message)
        : base(message)
    {
    }
}
