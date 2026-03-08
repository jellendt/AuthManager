using AuthManager.Contexts;
using AuthManager.Extensions;
using AuthManager.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace AuthManager
{
    public class Program
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddUserSecrets<Program>();

            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddService(builder.Configuration);
            builder.Services.AddMapper();
            builder.Services.AddDatabase(builder.Configuration);

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                ApplyDatabaseMigrations(app);
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors("AllowSpecificOrigin");
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();


            app.Run();
        }

        private static void ApplyDatabaseMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DbAuthContext>();
            dbContext.Database.Migrate();
        }
    }
}