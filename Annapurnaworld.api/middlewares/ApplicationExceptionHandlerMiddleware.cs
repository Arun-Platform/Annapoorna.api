using System.Net;

namespace Annapurnaworld.api
{
    /// <summary>
    /// Middleware class to handle exception globally.
    /// </summary>
    public class ApplicationExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApplicationExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">next object</param>
        /// <param name="logger">logger class</param>
        public ApplicationExceptionHandlerMiddleware(RequestDelegate next, ILogger<ApplicationExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invode middleware
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handle exception async
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="exception">exception</param>
        /// <returns>task</returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            var result = "An unexpected error occurred.";

            // Customize the response based on the exception type
            if (exception is ArgumentException)
            {
                code = HttpStatusCode.BadRequest;
                result = "Invalid request.";
            }
            if (exception.Message.Contains("token is expired") || exception is UnauthorizedAccessException)
            {
                code = HttpStatusCode.Unauthorized;
                result = "Invalid request.";
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(new
            {
                StatusCode = (int)code,
                Message = result,
            }.ToString());
        }

    }
}
