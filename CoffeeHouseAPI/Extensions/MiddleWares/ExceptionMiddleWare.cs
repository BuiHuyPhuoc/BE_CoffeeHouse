using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Services.Email;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text.Json;

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
                var modelState = httpContext.Features.Get<ModelStateDictionary>();

                if (modelState != null && !modelState.IsValid)
                {
                    var errors = modelState
                        .Where(ms => ms.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                        );

                    var responseObj = new APIResponseBase
                    {
                        Status = 422,
                        Message = "Format Error",
                        IsSuccess = false,
                        Error = errors
                    };

                    httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    httpContext.Response.ContentType = "application/json";

                    var jsonResponse = JsonSerializer.Serialize(responseObj);
                    await httpContext.Response.WriteAsync(jsonResponse);

                    return;
                }

                await _next(httpContext);
            }
            catch (Exception ex)
            {
                try
                {
                    var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
                    var recipients = new List<string> { "buihuyphuoc42@gmail.com", "nldangkhoa0712@gmail.com", "nguyenngocminhkhoa.dev@gmail.com" };
                    var subject = "🚨 System Error Alert";
                    var message = EMAIL_TEMPLATE.SendMailExceptionTemplate(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    _ = emailSender.SendEmailAsync(recipients, subject, message, null, null);
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
