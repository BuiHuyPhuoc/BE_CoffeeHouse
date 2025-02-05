using AutoMapper;
using Azure.Core;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoffeeHouseAPI.Controllers
{
    /// <summary>
    /// EOMS Base Controller
    /// </summary>
    public class TCHControllerBase : ControllerBase
    {
        /// <summary>
        /// Get Current User
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        [NonAction]
        public LoginResponse GetLoginResponseFromHttpContext()
        {
            // Lấy đối tượng từ HttpContext.Items và cast thành LoginResponse
            var userInfo = this.HttpContext?.Items["LoginResponse"] as LoginResponse;

            if (userInfo == null)
            {
                return new LoginResponse();
            }
            return userInfo;
        }


        [NonAction]
        public async Task SaveChanges(DbcoffeeHouseContext context)
        {
            var result = await context.SaveChangesAsync();
            if (result < 0)
                throw new Exception("Error!");
        }

        [NonAction]
        public string GetUrlPort()
        {
            //var request = HttpContext.Request;
            //var serverUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            //return serverUrl;
            return "https://localhost:3002";
        }
    }
}
