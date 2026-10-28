using AlfaBank.Core.Data;
using Microsoft.EntityFrameworkCore.Design;

namespace AlfaBank.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => AppDbContextFactory.Create();
}
