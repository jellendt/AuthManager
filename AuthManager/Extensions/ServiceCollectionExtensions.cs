using AuthManager.Automapper;
using AuthManager.Contexts;
using AuthManager.DependecyInjection;
using AuthManager.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace AuthManager.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Connection String is null or empty");

            services.AddDbContextPool<DbAuthContext>(options => options
               .UseSqlServer(
                   connectionString
               )
             );
            return services;
        }
        public static IServiceCollection RegisterAssemblyByConvention(
            this IServiceCollection collection,
            Assembly assembly
        )
        {
            collection.RegisterAllAs<ITransientDependency>(assembly, ServiceLifetime.Transient);
            collection.RegisterAllAs<IScopedDependency>(assembly, ServiceLifetime.Scoped);
            collection.RegisterAllAs<ISingletonDependency>(assembly, ServiceLifetime.Singleton);

            return collection;
        }

        public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            

            services.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
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
            services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.Issuer) &&
                !string.IsNullOrWhiteSpace(o.Audience) &&
                !string.IsNullOrWhiteSpace(o.Key) &&
                o.TokenLifetimeMinutes != -1,
                "Jwt settings are incomplete.");

            JwtOptions jwtSettings = configuration
               .GetSection(JwtOptions.SectionName)
               .Get<JwtOptions>() ?? throw new Exception("Jwt configuration missing");


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => ConfigureJwtBearer(options, jwtSettings));

            services.AddCors(ConfigureCorsPolicies);
            services.AddAuthorization();

            return services;
        }

        private static void RegisterAllAs<T>(
            this IServiceCollection collection,
            Assembly assembly,
            ServiceLifetime serviceLifetime
        )
        {
            var dependencies = assembly
                .GetTypes()
                .Where(IsBasedOn<T>)
                .Where(type => !type.IsAbstract)
                .Where(type => !type.GetTypeInfo().IsGenericTypeDefinition)
                .ToArray();

            foreach (var dependency in dependencies)
            {
                collection.RegisterAs(dependency, dependency, serviceLifetime);

                foreach (var dependencyInterface in dependency.GetInterfaces())
                {
                    collection.RegisterAs(dependencyInterface, dependency, serviceLifetime);
                }
            }
        }

        private static void RegisterAs(
            this IServiceCollection collection,
            Type serviceType,
            Type implementationType,
            ServiceLifetime serviceLifetime
        )
        {
            collection.Add(
                ServiceDescriptor.Describe(serviceType, implementationType, serviceLifetime)
            );
        }

        private static bool IsBasedOn<TBaseType>(Type potentialBase)
        {
            var type = typeof(TBaseType);

            if (potentialBase == type)
            {
                return false;
            }

            if (type.GetTypeInfo().IsAssignableFrom(potentialBase))
            {
                return true;
            }
            else if (potentialBase.GetTypeInfo().IsGenericTypeDefinition)
            {
                if (
                    potentialBase.GetTypeInfo().IsInterface
                    && IsBasedOnGenericInterface<TBaseType>(potentialBase)
                )
                {
                    return true;
                }

                if (IsBasedOnGenericClass<TBaseType>(potentialBase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBasedOnGenericClass<TBaseType>(Type potentialBase)
        {
            var type = typeof(TBaseType);

            for (; type != null; type = type.GetTypeInfo().BaseType)
            {
                if (
                    type.GetTypeInfo().IsGenericType
                    && type.GetGenericTypeDefinition() == potentialBase
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBasedOnGenericInterface<TBaseType>(Type potentialBase)
        {
            var type = typeof(TBaseType);

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (
                    interfaceType.GetTypeInfo().IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == potentialBase
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static void ConfigureJwtBearer(JwtBearerOptions options, JwtOptions jwtOptions)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.Zero
            };
        }

        private static void ConfigureCorsPolicies(CorsOptions options)
        {
            options.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());
        }
    }
}
