
using api_ai_rag_intent;
using api_ai_rag_intent.Interfaces;
using api_ai_rag_intent.Util;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Memory;
using SemanticKernel.Data.Nl2Sql.Library.Schema;
using SKDynamicSqlPlugin;
using System.Reflection;

//
string _apiDeploymentName = Helper.GetEnvironmentVariable("ApiDeploymentName");
string _apiEndpoint = Helper.GetEnvironmentVariable("ApiEndpoint");
string _apiKey = Helper.GetEnvironmentVariable("ApiKey");;
string _textEmbeddingName = Helper.GetEnvironmentVariable("EmbeddingName");
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(cb =>
    {
        Environment.SetEnvironmentVariable("RootConfigFolder", Repo.RootConfigFolder);
        cb.AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly());
    })
    .ConfigureServices(async (hostContext, services) =>
    {
        services.Configure<IConfiguration>(hostContext.Configuration);
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<Kernel>(s =>
        {
            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<IConfiguration>(hostContext.Configuration);
            builder.AddAzureOpenAIChatCompletion(
                _apiDeploymentName,
                _apiEndpoint,
                _apiKey
                );
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var memory = KernelFactory.CreateMemory(services.BuildServiceProvider());
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.Services.AddSingleton<ISemanticTextMemory>(memory);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.AddAzureOpenAITextEmbeddingGeneration(_textEmbeddingName, _apiEndpoint, _apiKey);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // builder.Plugins.AddFromType<DBQueryPlugin>();  
            builder.Plugins.AddFromType<SqlServerPlugin>();

            return builder.Build();
        });

        
        services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
        const string systemmsg = @$"You are a friendly assistant that can respond to general purpose questions";
        services.AddSingleton<ChatHistory>(s =>
        {
            var chathistory = new ChatHistory();
            chathistory.AddSystemMessage(systemmsg);
            return chathistory;
        });
    })
    .Build();

host.Run();

