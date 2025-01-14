using CoffeeHouseAPI.Services.Email;
using CoffeeHouseAPI.Services.Firebase;
using Microsoft.AspNetCore.Identity.UI.Services;
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


            // Register Service
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailSender, EmailSender>();

            // Register for database context
            services.AddDbContext<CoffeeHouseLib.Models.DbcoffeeHouseContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // Register for Firebase Service
            services.AddSingleton<FirebaseService>();

        }
    }
}
