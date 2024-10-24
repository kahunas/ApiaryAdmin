using ApiaryAdmin.Data;
using ApiaryAdmin.Data.DTOs;
using ApiaryAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
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
        apiariesGroups.MapPost("/apiaries", async (CreateApiaryDto dto, ApiaryDbContext dbContext) =>
        {
            var apiary = new Apiary { Name = dto.Name, Location = dto.Location, Description = dto.Description, CreationDate = DateTimeOffset.UtcNow};
            dbContext.Apiaries.Add(apiary);
            
            await dbContext.SaveChangesAsync();
            
            return TypedResults.Created($"/api/apiaries/{apiary.Id}", apiary.ToDto());
        });
        apiariesGroups.MapPut("/apiaries/{apiaryId}", async (UpdateApiaryDto dto, int apiaryId, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            if (apiary is null)
            {
                return Results.NotFound();
            }

            apiary.Name = dto.Name;
            apiary.Location = dto.Location;
            apiary.Description = dto.Description;
            
            dbContext.Apiaries.Update(apiary);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok(apiary.ToDto());
        });
        apiariesGroups.MapDelete("/apiaries/{apiaryId}", async (int apiaryId, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            if (apiary is null)
            {
                return Results.NotFound();
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

        hivesGroups.MapPost("/hives", async (int apiaryId, CreateHiveDto dto, ApiaryDbContext dbContext) =>
        {
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            if (apiary is null)
            {
                return Results.NotFound($"Apiary with ID {apiaryId} not found.");
            }

            dbContext.Entry(apiary).State = EntityState.Unchanged;  // Ensure Apiary is tracked

            var hive = new Hive { Name = dto.Name, Description = dto.Description, Apiary = apiary };
            dbContext.Hives.Add(hive);
            await dbContext.SaveChangesAsync();

            return TypedResults.Created($"/api/apiaries/{apiaryId}/hives/{hive.Id}", hive.ToDto());
        });
        hivesGroups.MapPut("/hives/{hiveId}", async (int apiaryId, int hiveId, UpdateHiveDto dto, ApiaryDbContext dbContext) =>
        {
            var hive = await dbContext.Hives.Where(h => h.Id == hiveId && h.Apiary.Id == apiaryId).FirstOrDefaultAsync();
            if (hive is null) return Results.NotFound();

            hive.Name = dto.Name;
            hive.Description = dto.Description;
            
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            hive.Apiary = apiary;
            
            dbContext.Hives.Update(hive);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok(hive.ToDto());
        });
        hivesGroups.MapDelete("/hives/{hiveId}", async (int apiaryId, int hiveId, ApiaryDbContext dbContext) =>
        {
            var hive = await dbContext.Hives.Where(h => h.Id == hiveId && h.Apiary.Id == apiaryId).FirstOrDefaultAsync();
            if (hive is null) return Results.NotFound();
            
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
        inspectionGroups.MapPost("/inspections", async (int apiaryId, int hiveId, CreateInspectionDto dto, ApiaryDbContext dbContext) =>
        {
            var hive = await dbContext.Hives.FindAsync(hiveId);
            if (hive is null) return Results.NotFound($"Hive with ID {hiveId} not found.");
            
            var inspection = new Inspection { Title = dto.Title, Date = dto.Date, Notes = dto.Notes, Hive = hive };
            var apiary = await dbContext.Apiaries.FindAsync(apiaryId);
            hive.Apiary = apiary;
            dbContext.Inspections.Add(inspection);
            await dbContext.SaveChangesAsync();
            
            return TypedResults.Created($"/api/apiaries/{hive.Apiary.Id}/hives/{hive.Id}/inspections/{inspection.Id}", inspection.ToDto());//added slash before
        });
        inspectionGroups.MapPut("/inspections/{inspectionId}", async (int hiveId, int inspectionId, UpdateInspectionDto dto, ApiaryDbContext dbContext) =>
        {
            var inspection = await dbContext.Inspections.Where(i => i.Id == inspectionId && i.Hive.Id == hiveId).FirstOrDefaultAsync();
            if (inspection is null) return Results.NotFound();
            var hive = await dbContext.Hives.FindAsync(hiveId);
            inspection.Hive = hive;

            inspection.Title = dto.Title;
            inspection.Date = dto.Date;
            inspection.Notes = dto.Notes;
            
            dbContext.Inspections.Update(inspection);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok(inspection.ToDto());
        });
        inspectionGroups.MapDelete("/inspections/{inspectionId}", async (int hiveId, int inspectionId, ApiaryDbContext dbContext) =>
        {
            var inspection = await dbContext.Inspections.Where(i => i.Id == inspectionId && i.Hive.Id == hiveId).FirstOrDefaultAsync();
            if (inspection is null) return Results.NotFound();
            
            dbContext.Inspections.Remove(inspection);
            await dbContext.SaveChangesAsync();
            
            return TypedResults.NoContent();
        });
    }
}