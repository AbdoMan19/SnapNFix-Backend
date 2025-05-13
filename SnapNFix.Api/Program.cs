using System.Text;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SnapNFix.Api.Extensions;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Extensions;

namespace SnapNFix.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddWebApiServices(builder.Configuration)
            .AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddCustomSwagger();


        

        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context => ValidationResult(context))
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        
        builder.Services.AddAuthentication(options =>
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
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
        
        

        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SnapNFixContext>();
            dbContext.Database.Migrate();
        }

        app.UseWebApiMiddleware();

        app.Run();
    }
    static BadRequestObjectResult ValidationResult(ActionContext context)
    {
        var errorList = context.ModelState
            .Where(state => state.Value.ValidationState == ModelValidationState.Invalid)
            .SelectMany(
                state => state.Value.Errors,
                (state, error) => new ErrorResponseModel
                {
                    PropertyName = state.Key,
                    Message = error.ErrorMessage,
                })
            .ToList();

        return new BadRequestObjectResult(GenericResponseModel<bool>.Failure("Validation Error", errorList));
    }
}