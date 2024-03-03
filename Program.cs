// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Plugins;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(0)
        .AddDebug();
});

// Create kernel
var builder = Kernel.CreateBuilder();
builder.WithCompletionService();
builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
var serviceProvider = builder.Services.BuildServiceProvider();

// Replace with your own schema
string schema = @$"
    CREATE TABLE products (product_id INT PRIMARY KEY, product_name VARCHAR(100), product_description TEXT, product_price DECIMAL(10, 2), product_category VARCHAR(50), in_stock BIT);

    CREATE TABLE sellers (seller_id INT PRIMARY KEY, seller_name VARCHAR(100), seller_email VARCHAR(100), seller_contact_number VARCHAR(15), seller_address TEXT);

    CREATE TABLE sales_transaction (transaction_id INT PRIMARY KEY, product_id INT, seller_id INT, quantity INT, transaction_date DATE, FOREIGN KEY (product_id) REFERENCES products(product_id), FOREIGN KEY (seller_id) REFERENCES sellers(seller_id));
";

string connectionString = Env.Var("ConnectionStrings:Database")!;

builder.Plugins.AddFromObject(new NlpToSqlPlugin(connectionString, schema, serviceProvider, loggerFactory), nameof(NlpToSqlPlugin));
var kernel = builder.Build();

// Create chat history
ChatHistory history = [];

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Start the conversation
while (true)
{
    // Get user input
    Console.Write("User > ");
    history.AddUserMessage(Console.ReadLine()!);

    // Enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    var first = true;
    await foreach (var content in result)
    {
        if (content.Role.HasValue && first)
        {
            Console.Write("Assistant > ");
            first = false;
        }
        Console.Write(content.Content);
        fullMessage += content.Content;
    }
    Console.WriteLine();

    // Add the message from the agent to the chat history
    history.AddAssistantMessage(fullMessage);
}