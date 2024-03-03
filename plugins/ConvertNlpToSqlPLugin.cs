using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Plugins;

public class ConvertNlpToSqlPlugin
{
    private readonly string _schema;
    private readonly ILogger<ConvertNlpToSqlPlugin> _logger;

    public ConvertNlpToSqlPlugin(
        string schema,
        ILoggerFactory loggerFactory)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        this._logger = loggerFactory.CreateLogger<ConvertNlpToSqlPlugin>();
    }

    [KernelFunction, Description("Write SQL queries given a Natural Language description")]
    public async Task<string> ConvertAsync(
        Kernel kernel,
        [Description("Define the Natural Language input text")] string input)
    {
        // Create chat history
        ChatHistory history = [];

        // Get chat completion service
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var prompt = @$"
            You are an expert at writing SQL queries through a given Natural Language description of the OBJECTIVE. 
            ---
            {input}
            ---

            You will generate a SQL SELECT query that is compatible with Transact-SQL and achieves the given OBJECTIVE. 
            You use only the tables and views described in following SCHEMA:

            {_schema}

            The output must be a SQL SELECT query that achieves the OBJECTIVE.
            Use Transact-SQL syntax to write the query compatible with Microsoft SQL Server and Azure SQL Database.        
        ";

        history.AddUserMessage(prompt);

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            Temperature = .8
        };

        var response = await chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings);

        return response.Content ?? "";
    }
}