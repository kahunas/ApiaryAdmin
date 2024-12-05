using System.Security.Claims;
using ApiaryAdmin.Auth.Model;
using ApiaryAdmin.Data;
using ApiaryAdmin.Data.DTOs;
using ApiaryAdmin.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace ApiaryAdmin;

public static class Endpoints
{
    public static void AddApiaryApi(this WebApplication app)
    {
        var apiariesGroups = app.MapGroup("/api").AddFluentValidationAutoValidation();

        apiariesGroups.MapGet("/apiaries", async (ApiaryDbContext dbContext) =>
        {
            return (await dbContext.Apiaries.ToListAsync()).Select(apiary => apiary.ToDto());
        });
        apiariesGroups.MapGet("/apiaries/{apiaryId}", async (int apiaryId, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            return apiary is null ? Results.NotFound() : TypedResults.Ok(apiary.ToDto());
        });
        apiariesGroups.MapPost("/apiaries", [Authorize(Roles = ApiaryRoles.ApiaryUser)] async (CreateApiaryDto dto, HttpContext httpContext, ApiaryDbContext dbContext) =>
        {
            var apiary = new Apiary { Name = dto.Name, Location = dto.Location, Description = dto.Description, CreationDate = DateTimeOffset.UtcNow,
                UserId = httpContext.User.FindFirstValue((JwtRegisteredClaimNames.Sub))};
            dbContext.Apiaries.Add(apiary);
            
            await dbContext.SaveChangesAsync();
            
            return TypedResults.Created($"/api/apiaries/{apiary.Id}", apiary.ToDto());
        });
        apiariesGroups.MapPut("/apiaries/{apiaryId}", [Authorize] async (UpdateApiaryDto dto, int apiaryId, HttpContext httpContext, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            if (apiary is null)
            {
                return Results.NotFound();
            }

            if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != apiary.UserId)
            {
                return Results.Forbid();
            }

            apiary.Name = dto.Name;
            apiary.Location = dto.Location;
            apiary.Description = dto.Description;
            
            dbContext.Apiaries.Update(apiary);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok(apiary.ToDto());
        });
        apiariesGroups.MapDelete("/apiaries/{apiaryId}", [Authorize] async (int apiaryId, HttpContext httpContext, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            if (apiary is null)
            {
                return Results.NotFound();
            }

            // Ensure the user is an admin or the owner of the apiary
            if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != apiary.UserId)
            {
                return Results.Forbid();
            }
    
            dbContext.Apiaries.Remove(apiary);
            await dbContext.SaveChangesAsync();
    
            return TypedResults.NoContent();
        });

    }

    public static void AddHiveApi(this WebApplication app)
    {
        var hivesGroups = app.MapGroup("/api/apiaries/{apiaryId}").AddFluentValidationAutoValidation();

        hivesGroups.MapGet("/hives", async (int apiaryId, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            if (apiary is null) return Results.NotFound();
            var hives = (await dbContext.Hives.Where(h => h.Apiary.Id == apiaryId).ToListAsync()).Select(hive => hive.ToDto());
            return TypedResults.Ok(hives);
        });
        hivesGroups.MapGet("/hives/{hiveId}", async (int apiaryId, int hiveId, ApiaryDbContext dbContext) =>
        {
            var hive = await dbContext.Hives.FirstOrDefaultAsync(h => h.Id == hiveId && h.Apiary.Id == apiaryId);
            if (hive is null)
            {
                return Results.NotFound();
            }

            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            hive.Apiary = apiary;
            return TypedResults.Ok(hive.ToDto());
        });

        hivesGroups.MapPost("/hives", [Authorize] async (int apiaryId, CreateHiveDto dto, HttpContext httpContext, ApiaryDbContext dbContext) =>
    {
        var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
        if (apiary is null)
        {
            return Results.NotFound($"Apiary with ID {apiaryId} not found.");
        }

        // Authorization: Ensure user is the owner of the apiary or an admin
        if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != apiary.UserId)
        {
            return Results.Forbid();
        }

        dbContext.Entry(apiary).State = EntityState.Unchanged; // Ensure Apiary is tracked

        var hive = new Hive
        {
            Name = dto.Name,
            Description = dto.Description,
            Apiary = apiary,
            UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) // Associate with user
        };
        dbContext.Hives.Add(hive);
        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/api/apiaries/{apiaryId}/hives/{hive.Id}", hive.ToDto());
    });

    hivesGroups.MapPut("/hives/{hiveId}", [Authorize] async (int apiaryId, int hiveId, UpdateHiveDto dto, HttpContext httpContext, ApiaryDbContext dbContext) =>
    {
        var hive = await dbContext.Hives.Where(h => h.Id == hiveId && h.Apiary.Id == apiaryId).FirstOrDefaultAsync();
        if (hive is null) return Results.NotFound();

        // Authorization: Ensure user is the owner of the hive or an admin
        if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != hive.UserId)
        {
            return Results.Forbid();
        }

        hive.Name = dto.Name;
        hive.Description = dto.Description;

        var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
        hive.Apiary = apiary;

        dbContext.Hives.Update(hive);
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(hive.ToDto());
    });

    hivesGroups.MapDelete("/hives/{hiveId}", [Authorize] async (int apiaryId, int hiveId, HttpContext httpContext, ApiaryDbContext dbContext) =>
    {
        var hive = await dbContext.Hives.Where(h => h.Id == hiveId && h.Apiary.Id == apiaryId).FirstOrDefaultAsync();
        if (hive is null) return Results.NotFound();

        // Authorization: Ensure user is the owner of the hive or an admin
        if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != hive.UserId)
        {
            return Results.Forbid();
        }

        dbContext.Hives.Remove(hive);
        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    });
    }

    public static void AddInspectionApi(this WebApplication app)
    {
        var inspectionGroups = app.MapGroup("/api/apiaries/{apiaryId}/hives/{hiveId}").AddFluentValidationAutoValidation();
        
        inspectionGroups.MapGet("/inspections", async (int hiveId, ApiaryDbContext dbContext) =>
        {
            var hive = await dbContext.Hives.FindAsync(hiveId);
            if (hive is null) return Results.NotFound();
            var inspections = (await dbContext.Inspections.Where(i => i.Hive.Id == hiveId).ToListAsync()).Select(inspection => inspection.ToDto());
            return TypedResults.Ok(inspections);
        });
        inspectionGroups.MapGet("/inspections/{inspectionId}", async (int hiveId, int inspectionId, ApiaryDbContext dbContext) =>
        {
            var inspection = await dbContext.Inspections.Where(i => i.Id == inspectionId && i.Hive.Id == hiveId).FirstOrDefaultAsync();
            if (inspection is null) return Results.NotFound();
            var hive = await dbContext.Hives.FindAsync(hiveId);
            inspection.Hive = hive;
            return TypedResults.Ok(inspection.ToDto());
        });
        inspectionGroups.MapPost("/inspections", [Authorize] async (int apiaryId, int hiveId, CreateInspectionDto dto, HttpContext httpContext, ApiaryDbContext dbContext) =>
        {
            var hive = await dbContext.Hives.FindAsync(hiveId);
            if (hive is null) return Results.NotFound($"Hive with ID {hiveId} not found.");
            
            // Authorization: Ensure user is the owner of the hive or an admin
            if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != hive.UserId)
            {
                return Results.Forbid();
            }
            
            var inspection = new Inspection { Title = dto.Title, Date = dto.Date, Notes = dto.Notes, Hive = hive, UserId = ""};
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            hive.Apiary = apiary;
            dbContext.Inspections.Add(inspection);
            await dbContext.SaveChangesAsync();
            
            return TypedResults.Created($"/api/apiaries/{hive.Apiary.Id}/hives/{hive.Id}/inspections/{inspection.Id}", inspection.ToDto());//added slash before
        });
        inspectionGroups.MapPut("/inspections/{inspectionId}", [Authorize] async (int hiveId, int inspectionId, UpdateInspectionDto dto, HttpContext httpContext, ApiaryDbContext dbContext) =>
        {
            var inspection = await dbContext.Inspections.Where(i => i.Id == inspectionId && i.Hive.Id == hiveId).FirstOrDefaultAsync();
            if (inspection is null) return Results.NotFound();

            var hive = await dbContext.Hives.FindAsync(hiveId);
            if (hive is null) return Results.NotFound($"Hive with ID {hiveId} not found.");

            // Authorization: Ensure user is the owner of the inspection or an admin
            if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != inspection.UserId)
            {
                return Results.Forbid();
            }

            inspection.Hive = hive;
            inspection.Title = dto.Title;
            inspection.Date = dto.Date;
            inspection.Notes = dto.Notes;

            dbContext.Inspections.Update(inspection);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok(inspection.ToDto());
        });

        inspectionGroups.MapDelete("/inspections/{inspectionId}", [Authorize] async (int hiveId, int inspectionId, HttpContext httpContext, ApiaryDbContext dbContext) =>
        {
            var inspection = await dbContext.Inspections.Where(i => i.Id == inspectionId && i.Hive.Id == hiveId).FirstOrDefaultAsync();
            if (inspection is null) return Results.NotFound();

            // Authorization: Ensure user is the owner of the inspection or an admin
            if (!httpContext.User.IsInRole(ApiaryRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != inspection.UserId)
            {
                return Results.Forbid();
            }

            dbContext.Inspections.Remove(inspection);
            await dbContext.SaveChangesAsync();

            return TypedResults.NoContent();
        });
    }
}