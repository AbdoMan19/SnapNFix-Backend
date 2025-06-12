namespace SnapNFix.Api.Extensions;

public static class MiddlewareExtension
{
    public static WebApplication UseWebApiMiddleware(this WebApplication app)
    {

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
                c.OAuthClientId("swagger-ui");
                c.OAuthAppName("Swagger UI");
            });
        }
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict
        });
        app.UseHsts();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        //app.UseMiddleware<IpRateLimitingMiddleware>();
        //app.UseRateLimiter();
        app.UseCors("DefaultPolicy");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
       // app.MapHealthChecks("/health");

        return app;
    }
}