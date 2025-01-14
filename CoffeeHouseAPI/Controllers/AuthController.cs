using AutoMapper;
using Azure.Core;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Auth;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.Email;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using ForgotPasswordRequest = CoffeeHouseAPI.DTOs.Auth.ForgotPasswordRequest;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : TCHControllerBase
    {
        private readonly DbcoffeeHouseContext _context;
        private readonly IEmailSender _email;
        private readonly IMapper _mapper;
        public AuthController(DbcoffeeHouseContext context, IEmailSender email, IMapper mapper)
        {
            _mapper = mapper;
            _email = email;
            _context = context;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginResquest request)
        {
            var account = await _context.Accounts.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

            if (account == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Email is not existed",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (account.BlockExpire != null && account.BlockExpire > DateTime.UtcNow)
            {

                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = $"Your account is blocked. Please try after {((DateTime)account.BlockExpire).ToString("HH:mm:ss")}.",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }


            if (account.Password != request.Password)
            {
                account.LoginFailed += 1;
                if (account.LoginFailed % 5 == 0)
                {
                    account.BlockExpire = DateTime.UtcNow.AddMinutes(account.LoginFailed);
                }
                await this.SaveChanges(_context);
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Login failed, please try again",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (account.VerifyTime == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Account is not verified.",
                    Status = (int)StatusCodes.Status403Forbidden,
                });
            }

            var customer = _context.Customers.Find(account.CustomerId);
            if (customer == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Login failed, please try again",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }
            LoginResponse loginResponse = this.MappingLoginResponseFromAccountAndCustomer(customer, account);

            string stringToken = CreateToken(customer, account);

            if (account.RefreshToken != null)
            {
                var oldRfsToken = _context.RefreshTokens.Find(account.RefreshToken);
                if (oldRfsToken != null)
                {
                     oldRfsToken.Revoke = DateTime.UtcNow;
                    await this.SaveChanges(_context);
                }
            }

            RefreshTokenDTO refreshTokenDTO = GenerateRefreshToken();
            RefreshToken refreshToken = _mapper.Map<RefreshToken>(refreshTokenDTO);
            _context.RefreshTokens.Add(refreshToken);
            await this.SaveChanges(_context);
            account.RefreshToken = refreshToken.RefreshToken1;
            account.LoginFailed = 0;
            await this.SaveChanges(_context);

            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expire
            };

            Response.Cookies.Append("refreshToken", refreshToken.RefreshToken1, options);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Login success",
                Status = (int)StatusCodes.Status200OK,
                Value = new
                {
                    Token = stringToken,
                    UserAccount = loginResponse
                }
            });
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserAccountRequest request)
        {
            Account? account = await _context.Accounts.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

            if (account != null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Account with this email is existed",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            Customer customer = new Customer
            {
                FullName = request.FullName,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                IdRole = request.IdRole,
            };

            await _context.Customers.AddAsync(customer);
            await this.SaveChanges(_context);

            string newOtp = GENERATE_DATA.GenerateString(64);

            var builder = WebApplication.CreateBuilder();
            string frontEndDomain = builder.Configuration["FrontendDomain"] ?? string.Empty;
            string urlVerify = frontEndDomain + "/verify?query=" + newOtp;
            
            account = new Account
            {
                Email = request.Email,
                Password = request.Password,
                VerifyToken = newOtp,
                LoginFailed = 0,
                CustomerId = customer.Id,
            };

            await _context.Accounts.AddAsync(account);
            await this.SaveChanges(_context);

            string subject = "Xác nhận tài khoản";
            await _email.SendEmailAsync(account.Email, subject, EMAIL_TEMPLATE.SendOtpTemplate(urlVerify));

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Register account success. Please check your email to get OTP.",
                Status = (int)StatusCodes.Status200OK,
            });
        }

        [HttpPost]
        [Route("VerifyAccount")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequest request)
        {
            var account = await _context.Accounts.Where(x => x.VerifyToken == request.Otp).FirstOrDefaultAsync();
            if (account == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Verify link is not existed.",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (account.VerifyTime != null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Account was verified.",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (request.Otp != account.VerifyToken)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Wrong OTP",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            account.VerifyTime = DateTime.UtcNow;
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Verify account success.",
                Status = (int)StatusCodes.Status200OK,
                Value = account.Email
            });
        }

        [HttpPost]
        [Route("ResendOtp")]
        public async Task<IActionResult> ResendOtp([FromBody] string email)
        {
            var account = await _context.Accounts.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (account == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Account with this email is not existed.",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (account.VerifyTime != null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Account is verified",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            string newOtp = GENERATE_DATA.GenerateString(64);

            var builder = WebApplication.CreateBuilder();
            string frontEndDomain = builder.Configuration["FrontendDomain"] ?? string.Empty;
            string urlVerify = frontEndDomain + "/Verify?token=" + newOtp;
            
            account.VerifyToken = newOtp;
            await this.SaveChanges(_context);

            string subject = "Xác nhận tài khoản";
            await _email.SendEmailAsync(account.Email, subject, EMAIL_TEMPLATE.SendOtpTemplate(urlVerify));

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Your OTP was sent to your email.",
                Status = (int)StatusCodes.Status200OK,
            });
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var account = await _context.Accounts.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (account == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = $"Account with {email} is not existed.",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            string otp = GENERATE_DATA.GenerateNumber(6);
            DateTime expire = DateTime.UtcNow.AddMinutes(5);

            account.ResetPasswordExpired = expire;
            account.ResetPasswordToken = otp;
            await this.SaveChanges(_context);

            string subject = "Xác nhận đổi mật khẩu";
            string message = EMAIL_TEMPLATE.SendOtpForgotPasswordTemplate(otp, expire);
            await _email.SendEmailAsync(email, subject, message);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = $"Your OTP was sent to your {email}.",
                Status = (int)StatusCodes.Status200OK,
            });
        }

        [HttpPost]
        [Route("SetNewPassword")]
        public async Task<IActionResult> SetNewPassword([FromBody] ForgotPasswordRequest request)
        {
            var account = await _context.Accounts.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
            if (account == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Account with this email is not existed.",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (account.ResetPasswordToken != request.Otp)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Wrong OTP",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            if (account.ResetPasswordExpired < DateTime.UtcNow)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "OTP is expired",
                    Status = (int)StatusCodes.Status400BadRequest,
                });
            }

            account.Password = request.NewPassword;
            account.BlockExpire = null;
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Your password was change success",
                Status = (int)StatusCodes.Status200OK,
            });

        }


        [HttpPost]
        [Route("GetNewToken")]
        public async Task<IActionResult> GetNewToken(string token)
        {
            Account? account;
            Customer? customer;

            var rfsTokenFromHttp = HttpContext.Request.Cookies["refreshToken"];

            if (rfsTokenFromHttp == null) return UnauthorizedResponse();

            var refreshToken = await _context.RefreshTokens.Where(x => x.RefreshToken1 == rfsTokenFromHttp).FirstOrDefaultAsync();

            if (refreshToken == null) return UnauthorizedResponse();

            if (refreshToken.Revoke != null) return UnauthorizedResponse();

            var accountFromRefreshToken = _context.Accounts.Where(x => x.RefreshToken == refreshToken.RefreshToken1).FirstOrDefault();

            if (accountFromRefreshToken == null) return UnauthorizedResponse();

            GetAccountFromJwtToken(token, out customer, out account);

            if (account == null || customer == null) return UnauthorizedResponse();

            if (account.Email != accountFromRefreshToken.Email) return UnauthorizedResponse();

            var newToken = CreateToken(customer, account);

            RefreshTokenDTO refreshTokenDTO = GenerateRefreshToken();
            refreshToken.RefreshToken1 = refreshTokenDTO.RefreshToken1;
            refreshToken.Expire = refreshTokenDTO.Expire;
            refreshToken.Created = refreshTokenDTO.Created;
            await this.SaveChanges(_context);
            account.RefreshToken = refreshToken.RefreshToken1;
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Get new token success",
                Status = (int)StatusCodes.Status200OK,
                Value = newToken
            });
        }
        private ObjectResult UnauthorizedResponse()
        {
            return Unauthorized(new APIResponseBase
            {
                IsSuccess = false,
                Message = "Login timeout.",
                Status = (int)StatusCodes.Status401Unauthorized,
            });
        }

        private string CreateToken(Customer customer, Account account)
        {
            var builder = WebApplication.CreateBuilder();
            var issuer = builder.Configuration["Jwt:Issuer"];
            var audience = builder.Configuration["Jwt:Audience"];
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key cannot be null.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, customer.FullName),
                    new Claim(ClaimTypes.Email, account.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(builder.Configuration["Jwt:Expires"])),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (key, SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

        private RefreshTokenDTO GenerateRefreshToken()
        {
            var rfsToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var rfsTokenModel = _context.RefreshTokens.Find(rfsToken);

            while (rfsTokenModel != null)
            {
                rfsToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                rfsTokenModel = _context.RefreshTokens.Find(rfsToken);
            }

            var refreshToken = new RefreshTokenDTO
            {
                RefreshToken1 = rfsToken,
                Expire = DateTime.UtcNow.AddDays(1),
                Created = DateTime.UtcNow
            };

            return refreshToken;
        }

        private void GetAccountFromJwtToken(string token, out Customer? customer, out Account? account)
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
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            ClaimsPrincipal principal;

            try
            {
                principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                var claims = principal.Claims;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                account = _context.Accounts.Where(x => x.Email == email && (x.BlockExpire < DateTime.UtcNow || x.BlockExpire == null)).FirstOrDefault();
                customer = _context.Customers.Find(account?.CustomerId ?? -1);

            }
            catch (SecurityTokenException)
            {
                account = null;
                customer = null;
            }
        }

        private APIResponseBase ReturnAPIResponse(string message, object value, int StatusCode, bool isSuccess = false)
        {
            return new APIResponseBase
            {
                Message = message,
                Value = value,
            };
        }
    }
}
