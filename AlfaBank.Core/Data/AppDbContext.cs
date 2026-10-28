using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();

    public DbSet<CreditProduct> CreditProducts => Set<CreditProduct>();

    public DbSet<CreditApplication> CreditApplications => Set<CreditApplication>();

    public DbSet<Credit> Credits => Set<Credit>();

    public DbSet<CreditScheduleItem> CreditScheduleItems => Set<CreditScheduleItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<ApplicationStatusHistory> ApplicationStatusHistory => Set<ApplicationStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Configure(modelBuilder);
    }
}
