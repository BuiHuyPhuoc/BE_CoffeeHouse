using CoffeeHouseAPI.Extensions.MiddleWares;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.AddressService;
using CoffeeHouseAPI.Services.Email;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseAPI.Services.VoucherService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OrderService.Helper;

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
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // CORS 
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
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

            // Register for Firebase Service
            services.AddSingleton<FirebaseService>();

            services.AddScoped<MasterAuth>();

            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IAddressService, AddressService>();
        }
    }
}
