using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Plugins;

public class WriteDatabaseResponsePlugin
{
    private readonly ILogger<WriteDatabaseResponsePlugin> _logger;

    public WriteDatabaseResponsePlugin(
        ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<WriteDatabaseResponsePlugin>();
    }

    [KernelFunction, Description("Write a friendly response given a database query result")]
    public static async Task<string> WriteDatabaseResponseAsync(
        Kernel kernel,
        [Description("Database query result")] string input)
    {
        if (input == "NO_RECORDS")
        {
            return "The query results in no records found.";
        }

        // Create chat history
        ChatHistory history = [];

        // Get chat completion service
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var prompt = @$"
            The user has provided a Natural Language description of the OBJECTIVE
            ---
            {input}
            ---
            Your goal is to create a response to the end user based on the OBJECTIVE.
            The response should be formulated based on the information returned from the database and the original user input.

            Ex: 
            Response: [{"NumberOfTransactions": 30}]
            Message -> According to the database query the number of transactions is 30.

            ---
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