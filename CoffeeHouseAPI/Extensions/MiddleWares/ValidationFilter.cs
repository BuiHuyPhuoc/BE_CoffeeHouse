using CoffeeHouseAPI.DTOs.APIPayload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoffeeHouseAPI.Extensions.MiddleWares
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
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

                context.Result = new ObjectResult(responseObj)
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }
        }
    }
}
