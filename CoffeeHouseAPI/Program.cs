
using CoffeeHouseAPI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CoffeeHouseAPI.Extensions.MiddleWares;
using CoffeeHouseAPI.Services.HubService;

namespace CoffeeHouseAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Register(builder.Configuration);

            var app = builder.Build();

            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI();

            // Use cors
            app.UseCors("AllowAll");

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseMiddleware<ExceptionMiddleWare>();

            app.MapControllers();

            app.MapHub<OrderHub>("/orderHub");
            app.MapHub<MessageHub>("/messageHub");

            app.Run();
        }
    }
}
