using CoffeeHouseAPI.Extensions.MiddleWares;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.AddressService;
using CoffeeHouseAPI.Services.Email;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseAPI.Services.OrderService;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseAPI.Services.VoucherService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace CoffeeHouseAPI.Extensions
{
    public static class ServiceExtension
    {
        public static void Register(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký các dịch vụ
            ServiceRegister(services, configuration);
        }

        private static void ServiceRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // CORS 
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("https://coffeehousee.site")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Register middleware validate
            services.AddScoped<ValidationFilter>();

            // Register Service
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailSender, EmailSender>();

            // Register for database context
            services.AddDbContext<CoffeeHouseLib.Models.DbcoffeeHouseContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            services.AddHttpClient();

            // Register for Firebase Service
            services.AddSingleton<FirebaseService>();

            services.AddScoped<MasterAuth>();

            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IOrderService, OrderService>();

            var jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key cannot be null.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddControllers(
                options =>
                {
                    options.Filters.Add<ValidationFilter>();
                }
            );

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token theo format: Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
            services.AddHttpClient();
        }
    }
}
