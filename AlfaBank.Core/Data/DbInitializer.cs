using AlfaBank.Core.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace AlfaBank.Core.Data;

public static class DbInitializer
{
    public static async Task<OperationResult<int>> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = AppDbContextFactory.Create();

            await context.Database.MigrateAsync(cancellationToken);

            var userCount = await context.Users.CountAsync(cancellationToken);

            return OperationResult<int>.Success(userCount);
        }
        catch (SqlException exception)
        {
            return OperationResult<int>.Failure(
                $"Не удалось подключиться к локальной базе данных. Проверьте, что установлен Microsoft SQL Server LocalDB. {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            return OperationResult<int>.Failure($"База данных недоступна: {exception.Message}");
        }
    }
}
