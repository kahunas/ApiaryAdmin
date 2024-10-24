using ApiaryAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiaryAdmin.Data;

public class ApiaryDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<Apiary> Apiaries { get; set; }
    public DbSet<Hive> Hives { get; set; }
    public DbSet<Inspection> Inspections { get; set; }

    public ApiaryDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSQL"));
    }
}