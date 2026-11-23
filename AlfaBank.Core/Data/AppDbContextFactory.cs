using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Data;

public static class AppDbContextFactory
{
    public static DbContextOptions<AppDbContext> CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(ConnectionStringProvider.GetConnectionString());

        return optionsBuilder.Options;
    }

    public static AppDbContext Create() => new(CreateOptions());
}
