using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using SnapNFix.Domain.Entities;
using SnapNFix.Infrastructure.Context;
using SnapNFix.Infrastructure.Extensions;
using System.Threading.RateLimiting;
using MediatR;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SnapNFix.Api.Extensions;
using SnapNFix.Application.Extensions;

namespace SnapNFix.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddWebApiServices()
            .AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddCustomSwagger();
        

        builder.Services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Jwt:Key"]))
                };
            });
        
        

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SnapNFixContext>();
            dbContext.Database.Migrate(); 
        }
        app.UseWebApiMiddleware();
        app.UseRouting();

        app.MapControllers();

        app.Run();
    }
}