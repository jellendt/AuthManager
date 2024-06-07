using AuthManager.Contexts;
using AuthManager.Services.AuthenticationService;
using AuthManager.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthManager
{
    public static class Startup
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = GetConnectionString("LocalConnectionStrings", configuration);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Connection String is null or empty");

            services.AddDbContextPool<DbAuthContext>(options => options
               .UseMySql(
                   connectionString,
                   new MySqlServerVersion(new Version(10, 6, 16))
               )
             );
            return services;
        }



        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Konfiguration der JWT-Authentifizierung
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        if (context.Request.Cookies.TryGetValue("refreshToken", out string? accessToken))
                        {
                            context.Token = accessToken;
                            logger.LogInformation("Token extracted from cookie: {Token}", accessToken);
                        }
                        else
                        {
                            logger.LogWarning("No access token found in cookies.");
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Authentication failed: {Exception}", context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Token validated successfully.");
                        return Task.CompletedTask;
                    }
                };

            });

            services.AddAuthorization();

            return services;
        }
        private static string? GetConnectionString(string key, IConfiguration configuration)
        {
            string? result = configuration.GetConnectionString(key);
            if (!string.IsNullOrEmpty(result))
                return result;
            return null;
        }
    }
}
