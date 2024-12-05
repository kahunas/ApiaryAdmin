using System.Text;
using ApiaryAdmin.Auth;
using ApiaryAdmin.Auth.Model;
using ApiaryAdmin.Data;
using ApiaryAdmin.Data.Entities;
using ApiaryAdmin.Helpers;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiaryAdmin;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); 
        builder.Services.AddDbContext<ApiaryDbContext>();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddFluentValidationAutoValidation(configuration =>
        {
            configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
            });
        });

        builder.Services.AddTransient<JwtTokenService>();
        builder.Services.AddTransient<SessionService>();
        builder.Services.AddScoped<AuthSeeder>();
        
        builder.Services.AddIdentity<ApiaryUser, IdentityRole>()
            .AddEntityFrameworkStores<ApiaryDbContext>()
            .AddDefaultTokenProviders();
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
            options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]));
        });

        builder.Services.AddAuthorization();
        
        // Add Swagger generator to the service collection
        builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Apiary Admin API",
                Description = "An API for managing apiaries, hives, and inspections",
                Contact = new OpenApiContact
                {
                    Name = "Your Name",
                    Email = "your.email@example.com"
                }
            });
        });
        
        
        var app = builder.Build();
        app.UseCors();
        
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiaryDbContext>();
        dbContext.Database.Migrate();
        
        var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
        dbSeeder.SeedAsync();

        
        // Configure Swagger middleware
        if (app.Environment.IsDevelopment())
        {
            // Enable Swagger middleware for JSON and YAML output
            app.UseSwagger(c =>
            {
                // Serve the Swagger JSON and YAML formats
                c.SerializeAsV2 = false; // Ensure OpenAPI 3.0 (can be omitted if this is default)
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Apiary Admin API v1 (JSON)");
                c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Apiary Admin API v1 (YAML)");
                c.RoutePrefix = string.Empty; // Make Swagger UI the root page
            });

            // Add a middleware to serve the YAML format as well
            app.MapGet("/swagger/v1/swagger.yaml", async context =>
            {
                var swaggerProvider = context.RequestServices.GetRequiredService<ISwaggerProvider>();
                var swagger = swaggerProvider.GetSwagger("v1");
                var yaml = new YamlDotNet.Serialization.Serializer().Serialize(swagger);
                context.Response.ContentType = "application/x-yaml";
                await context.Response.WriteAsync(yaml);
            });
        }
        app.AddAuthApi();
        app.AddApiaryApi();
        app.AddHiveApi();
        app.AddInspectionApi();
        app.UseAuthentication();
        app.UseAuthorization();
        app.Run();
    }
}