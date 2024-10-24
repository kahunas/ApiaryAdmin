using ApiaryAdmin.Data;
using ApiaryAdmin.Data.Entities;
using ApiaryAdmin.Helpers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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

            // Optionally add XML comments
            //var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //ar xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });
        
        
        var app = builder.Build();
        
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
        
        app.AddApiaryApi();
        app.AddHiveApi();
        app.AddInspectionApi();
        app.Run();
    }
}