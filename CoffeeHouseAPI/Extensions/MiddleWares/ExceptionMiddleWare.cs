using CoffeeHouseLib.Models;
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
            var dbContext = serviceProvider.GetService<DbcoffeeHouseContext>();

            if (httpContext.Request.Method == HttpMethods.Get)
            {
                await _next(httpContext);
                return;
            }

            if (dbContext == null)
            {
                await _next(httpContext);
                return;
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await _next(httpContext);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {

                /*
IEmailTemplateService _emailTemplateService = new EmailTemplateService(new UnitOfWorkEOMS(new eOMSContext()));

var emailTemplate = _emailTemplateService.findByTemplate(TemplateEmailEnum.SYSTEM_ERROR.ToString(),
                new string[]
                {
                        GetUserInfoFromHttpContext().Value?.UserName,string.Empty,
                     ex.ToString(),
                     "1"

                },
                new string[]
                {
                    DateTime.Now.ToString("ddMMyyyy HH:mm:ss"),
                }
                );

List<string> toEmails = new List<string>();
if (emailTemplate.RecipientEmail != null && emailTemplate.RecipientEmail.Length > 0)
{
    toEmails.AddRange(emailTemplate.RecipientEmail.Split(";").ToList());
}

//await SendMail.Send(emailTemplate.Subject, emailTemplate.Body, toEmails, null, (emailTemplate.CcEmails ?? string.Empty).Split(";").ToList(), (emailTemplate.BccEmails ?? string.Empty).Split(";").ToList());



log.Error(ex);
_logger.LogError(ex, ex.Message);
httpContext.Response.ContentType = "application/json";
httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//new APIException((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace.ToString())
var response = _hostEnvironment.IsDevelopment() ?
    new APIResponseBase
    {
        code = (int)HttpStatusCode.InternalServerError,
        message = ex.Message,
        status = (int)HttpStatusCode.InternalServerError,
    }
    : new APIResponseBase
    {
        code = (int)HttpStatusCode.InternalServerError,
        status = (int)HttpStatusCode.InternalServerError,
    };

var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var json = JsonSerializer.Serialize(response, options);
log.Info(httpContext.Response.WriteAsync(json));

await httpContext.Response.WriteAsync(json);
*/

                await transaction.RollbackAsync();

                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình xử lý request.");

                if (!httpContext.Response.HasStarted)
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

                throw;
            }

        }

    }
}
