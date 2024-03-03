using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MinimalApi.Data;
using MinimalApi.Services;

namespace Plugins;

public class SqlDbPlugin
{
    private readonly ILogger<SqlDbPlugin> _logger;
    private readonly string _connectionString;

    public SqlDbPlugin(
        string connectionString,
        ILoggerFactory loggerFactory)
    {
        this._connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        this._logger = loggerFactory.CreateLogger<SqlDbPlugin>();
    }

    [KernelFunction, Description("Query a database using a SQL query")]
    public async Task<string> QueryDbAsync(
        Kernel kernel,
        [Description("SQL Query to be executed")] string input)
    {
        _logger.LogDebug($"Select SQL statement: {input}");

        var parsedInput = ParseInput(input);

        _logger.LogDebug($"Select SQL statement: {parsedInput}");

        var optionsBuilder = new DbContextOptionsBuilder<SelectContext>()
            .UseSqlServer(this._connectionString);

        using var db = new SelectContext(optionsBuilder.Options);
        var repo = new SelectRepository(db);
        var result = await repo.ExecuteSqlSelectStatementAsync(parsedInput, CancellationToken.None);

        return result;
    }

    private string ParseInput(string input)
    {
        var startIndex = input.IndexOf("```sql") + 6;
        var str = input.Substring(startIndex, input.Length - startIndex);
        startIndex = str.IndexOf("SELECT ");
        str = str.Substring(startIndex, str.Length - startIndex);
        var endIndex = str.IndexOf("```");
        str = str.Substring(0, endIndex);
        return str;
    }
}