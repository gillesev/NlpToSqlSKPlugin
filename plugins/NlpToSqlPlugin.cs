using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;

namespace Plugins;

public class NlpToSqlPlugin
{
    private readonly string _connectionString;
    private readonly string _schema;
    private readonly IServiceProvider _sp;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<NlpToSqlPlugin> _logger;

    public NlpToSqlPlugin(
        string connectionString,
        string schema,
        IServiceProvider sp,
        ILoggerFactory loggerFactory)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<NlpToSqlPlugin>();
    }

    [KernelFunction]
    [Description("Creates a SQL query from a natural language statement input and query the database.")]
    public async Task<string> RunNLPToSQLQueryAsync(
        Kernel kernel,
        [Description("The natural language statement input")] string statement)
    {
        _logger.LogDebug($"The Nlp statement: {statement}.");

        var kernelWithSql = kernel.Clone();

        // Remove this plugin so that we don't get into an infinite loop
        kernelWithSql.Plugins.Remove(kernelWithSql.Plugins[nameof(NlpToSqlPlugin)]);

        // Add the SQl plugins but what should the service scope be?
        kernelWithSql.Plugins.AddFromObject(new ConvertNlpToSqlPlugin(_schema, this._loggerFactory), nameof(ConvertNlpToSqlPlugin));
        kernelWithSql.Plugins.AddFromObject(new SqlDbPlugin(_connectionString, _loggerFactory));
        kernelWithSql.Plugins.AddFromType<WriteDatabaseResponsePlugin>(nameof(WriteDatabaseResponsePlugin), _sp);

        // Creates the plan
        var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = false });
        var goal = $"Create a SQL query according to the following request: {statement} and query the database to get the result.";

        var plan = await planner.CreatePlanAsync(kernelWithSql, goal);
        this._logger.LogInformation($"Plan: {plan}");

        // Execute the plan
        var result = (await plan.InvokeAsync(kernelWithSql, [])).Trim();
        this._logger.LogInformation($"Results: {result}");

        return result;
    }
}