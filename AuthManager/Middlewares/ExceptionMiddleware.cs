using AuthManager.Exceptions;

namespace AuthManager.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private static async Task HandleException(HttpContext context, Exception exception)
        {
            int status = StatusCodes.Status500InternalServerError;
            object response;

            switch (exception)
            {
                case ApiException apiException:
                    status = apiException.StatusCode;
                    response = new
                    {
                        type = exception.GetType().Name,
                        message = apiException.Message
                    };
                    break;

                case InvalidCredentialsException:
                    status = StatusCodes.Status401Unauthorized;
                    response = new
                    {
                        type = "Unauthorized",
                        message = "Invalid credentials"
                    };
                    break;

                default:
                    response = new
                    {
                        type = "ServerError",
                        message = "Internal server error"
                    };
                    break;
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
