using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Services.Email;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CoffeeHouseAPI.Extensions.MiddleWares
{
    public class ExceptionMiddleWare
    {

        private readonly RequestDelegate _next;
        private string requestBody = string.Empty;
        public ExceptionMiddleWare(
            RequestDelegate next,
            IHostEnvironment hostEnvironment)
        {
            _next = next;
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

                requestBody = await ReadRequestBody(httpContext);

                await _next(httpContext);
            }
            catch (Exception ex)
            {
                string url = httpContext.Request.Path; 
                string method = httpContext.Request.Method;

                try
                {
                    var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
                    var recipients = new List<string> { "buihuyphuoc42.dev@gmail.com", "nldangkhoa0712@gmail.com", "nguyenngocminhkhoa.dev@gmail.com", "dvhtuan.work@gmail.com" };
                    var subject = "🚨 System Error Alert";
                    var message = EMAIL_TEMPLATE.SendMailExceptionTemplate(new string[] { ex.InnerException != null ? ex.InnerException.Message : ex.Message, url, method, requestBody });
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
        private async Task<string> ReadRequestBody(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering();

                using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true);

                string body = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = 0; 

                return body;
            }
            catch
            {
                return "Cannot read request body";
            }
        }

    }
}
