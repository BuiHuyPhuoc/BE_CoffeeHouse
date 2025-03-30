using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace CoffeeHouseAPI.Helper
{
    public class MasterAuth : Attribute, IAsyncAuthorizationFilter
    {
        public bool _admin;
        public MasterAuth(bool admin = false)
        {
            _admin = admin;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            string endPoint = context.HttpContext.Request.Path;

            bool isValidateLifetime = true;
            if (endPoint == "api/Auth/GetNewToken")
                isValidateLifetime = false;

            var email = GetMailFromJWTToken(token, isValidateLifetime);

            if (email == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            DbcoffeeHouseContext _context = new DbcoffeeHouseContext();
            var account = await _context.Accounts.Where(x => x.IdNavigation.Email == email && x.IdNavigation.IsValid && (x.BlockExpire < DateTime.UtcNow || x.BlockExpire == null)).FirstOrDefaultAsync();
            if (account == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var customer = _context.Customers.Where(x => x.Id == account.Id && x.IsValid).FirstOrDefault();

            if (customer != null && (_admin == false || customer.IdRole == _admin))
            {
                LoginResponse loginResponse = MappingLoginResponseFromAccountAndCustomer(customer);

                context.HttpContext.Items["LoginResponse"] = loginResponse;

                return;
            }
            else
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }

        public static LoginResponse MappingLoginResponseFromAccountAndCustomer(Customer customer)
        {
            LoginResponse loginResponse = new LoginResponse
            {
                Id = customer.Id,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth,
                Phone = customer.Phone,
                IdRole = customer.IdRole,
                Email = customer.Email,
                AvatarImage = customer.AvatarImageNavigation?.FirebaseImage ?? "https://firebasestorage.googleapis.com/v0/b/shopoes-2e0b8.appspot.com/o/coffee_house%2Fdefault_avatar.png?alt=media&token=d1d661aa-7772-4bda-86ea-3a3b66ca513b"
            };
            return loginResponse;
        }

        private string? GetMailFromJWTToken(string token, bool validateLifetime)
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key cannot be null.");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var issuer = builder.Configuration["Jwt:Issuer"];
                var audience = builder.Configuration["Jwt:Audience"];

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                var claims = principal.Claims;

                return claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            }
            catch (SecurityTokenExpiredException)
            {
                return null;
            }
        }
    }
}
