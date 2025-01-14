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
        public LoginResponse? GetLoginResponseFromHttpContext(AuthorizationFilterContext context)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            DbcoffeeHouseContext _context = new DbcoffeeHouseContext();
            Account? account = _context.Accounts.Where(x => x.Email == emailClaim).FirstOrDefault();
            if (account == null)
                return null;

            var customer = _context.Customers.Find(account.CustomerId);
            if (customer == null)
                return null;

            return MappingLoginResponseFromAccountAndCustomer(customer, account);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public LoginResponse MappingLoginResponseFromAccountAndCustomer(Customer customer, Account account)
        {
            LoginResponse loginResponse = new LoginResponse
            {
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth,
                Phone = customer.Phone,
                IdRole = customer.IdRole,
                Email = account.Email,
            };
            return loginResponse;
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
            var request = HttpContext.Request;
            var serverUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return serverUrl;
        }
    }
}
