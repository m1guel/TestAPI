using System.Security.Claims;
using TestAPI.Domain;

namespace TestAPI.Application.Middleware
{
    /// <summary>
    /// Middleware to populate RequestContext with authenticated user information from JWT token
    /// </summary>
    public class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if user is authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Extract UserId from NameIdentifier claim
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    RequestContext.UserId = userId;
                }

                // Extract Email from Email claim
                RequestContext.Email = context.User.FindFirst(ClaimTypes.Email)?.Value;

                // Set authenticated flag
                RequestContext.IsAuthenticated = true;
            }
            else
            {
                // Clear context for unauthenticated requests
                RequestContext.UserId = null;
                RequestContext.Email = null;
                RequestContext.IsAuthenticated = false;
            }

            try
            {
                // Continue to next middleware
                await _next(context);
            }
            finally
            {
                // Clear context after request completes to prevent leaking to other requests
                RequestContext.UserId = null;
                RequestContext.Email = null;
                RequestContext.IsAuthenticated = false;
            }
        }
    }

    /// <summary>
    /// Extension method for registering RequestContextMiddleware
    /// </summary>
    public static class RequestContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestContextMiddleware>();
        }
    }
}
