using AuthManager;
using AuthManager.Contexts;
using AuthManager.Services.AuthenticationService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

Startup.AddAuthenticationServices(builder.Services, builder.Configuration);
Startup.AddServices(builder.Services);
Startup.AddMapper(builder.Services);
Startup.AddDatabase(builder.Services, builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
    //.AddJsonOptions(options => 
    //    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve
    //);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthentication(); // This must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();


app.Run();
