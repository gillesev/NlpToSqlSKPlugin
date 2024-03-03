# Using planner

The `CsharpNlpToSql` console application shows how a Semantic Kernel plugin can be used to support the model to query a Transact-Sql compatible database.

## Prerequisites

- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) is required to run this sample.
- Install the recommended extensions
- [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
- [Semantic Kernel Tools](https://marketplace.visualstudio.com/items?itemName=ms-semantic-kernel.semantic-kernel) (optional)

## Configuring the sample

The sample can be configured by using the command line with .NET [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to avoid the risk of leaking secrets into the repository, branches and pull requests.

This sample has been tested with the following models:

| Service      | Model type      | Model            | Model version | Supported |
| ------------ | --------------- | ---------------- | ------------: | --------- |
| Azure OpenAI | Chat Completion | gpt-4            |          0613 | ✅        |
| Azure OpenAI | Chat Completion | gpt-4            |          1106 | ✅        |

This sample uses function calling, so it only works on models newer than 0613 and works best with gpt-4.

### Using .NET [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

Configure an Azure OpenAI endpoint

```powershell
cd CSharpNlpToSql

dotnet user-secrets set "Global:LlmService" "AzureOpenAI"
dotnet user-secrets set "ConnectionStrings:Database" "{your SQL Server database connection string HERE}"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://{your Azure Open AI resource name HERE}.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentType" "chat-completion"
dotnet user-secrets set "AzureOpenAI:ChatCompletionModelId" "gpt-4-1106"
dotnet user-secrets set "AzureOpenAI:ChatCompletionDeploymentName" "gpt-4"
dotnet user-secrets set "AzureOpenAI:ApiKey" "{your Azure Open AI Key HERE}"

## Running the sample

After configuring the sample, to build and run the console application just hit `F5`.

To build and run the console application from the terminal use the following commands:

```powershell
dotnet build
dotnet run
```