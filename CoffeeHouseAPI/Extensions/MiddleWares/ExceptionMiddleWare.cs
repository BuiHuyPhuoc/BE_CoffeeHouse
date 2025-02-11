using CoffeeHouseAPI.Services.Email;
using System.Net;

namespace CoffeeHouseAPI.Extensions.MiddleWares
{
    public class ExceptionMiddleWare
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleWare> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public ExceptionMiddleWare(
            RequestDelegate next,
            ILogger<ExceptionMiddleWare> logger,
            IHostEnvironment hostEnvironment)
        {
            _next = next;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public async Task InvokeAsync(HttpContext httpContext, IServiceProvider serviceProvider)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                try
                {
                    var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
                    var recipients = new List<string> { "buihuyphuoc42@gmail.com", "nldangkhoa0712@gmail.com" };
                    var subject = "🚨 System Error Alert";
                    var message = EMAIL_TEMPLATE.SendMailExceptionTemplate(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    await emailSender.SendEmailAsync(recipients, subject, message, null, null);
                }
                finally
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsJsonAsync(new
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Message = "Internal Error.",
                        Detail = "An unexpected error occurred.",
                    });
                }
            }
        }

    }
}
