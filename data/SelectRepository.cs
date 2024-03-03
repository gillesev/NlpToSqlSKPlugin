using Microsoft.EntityFrameworkCore;

namespace MinimalApi.Services;

public class SelectRepository
{
    private readonly DbContext _dbContext;

    public SelectRepository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<string> ExecuteSqlSelectStatementAsync(string select, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = select;
            await _dbContext.Database.OpenConnectionAsync(cancellationToken);
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                if (!reader.HasRows)
                {
                    return "NO_RECORDS";
                }

                // Do something with result
                while (await reader.ReadAsync() == true)
                {
                    result += "\n";
                    // hopefully this value is a constant across rows
                    int noOfColumns = reader.FieldCount;
                    for (int i = 0; i < noOfColumns; i++)
                    {
                        if (i > 0) { result += ","; }
                        result += reader.GetValue(i)?.ToString();
                    }
                }
            }
        }

        return result;
    }
}