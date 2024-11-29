using ApiaryAdmin.Auth.Model;
using ApiaryAdmin.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiaryAdmin.Data;

public class ApiaryDbContext : IdentityDbContext<ApiaryUser>
{
    private readonly IConfiguration _configuration;
    public DbSet<Apiary> Apiaries { get; set; }
    public DbSet<Hive> Hives { get; set; }
    public DbSet<Inspection> Inspections { get; set; }
    public DbSet<Session> Sessions { get; set; }

    public ApiaryDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSQL"));
    }
    
    /*
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the Apiary -> Hive relationship with cascade delete
        modelBuilder.Entity<Apiary>()
            .HasMany(a => a.Hives)
            .WithOne(h => h.Apiary)
            .HasForeignKey(h => h.ApiaryId) // Ensure foreign key property
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the Hive -> Inspection relationship with cascade delete
        modelBuilder.Entity<Hive>()
            .HasMany(h => h.Inspections)
            .WithOne(i => i.Hive)
            .HasForeignKey(i => i.HiveId) // Ensure foreign key property
            .OnDelete(DeleteBehavior.Cascade);
    }
    */
    
}