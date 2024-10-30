using AuthManager.Automapper;
using AuthManager.Contexts;
using AuthManager.Services.AuthenticationService;
using AuthManager.Services.UserService;
using AutoMapper;
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

            services.AddControllers();
            //.AddJsonOptions(options => 
            //    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve
            //);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            MapperConfiguration mapperConfig = new(mc =>
                mc.AddProfile(new MappingProfile())
            );

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new Exception("Key not found")))
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
