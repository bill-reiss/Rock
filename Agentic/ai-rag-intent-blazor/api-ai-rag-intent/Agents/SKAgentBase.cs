using api_ai_rag_intent.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using SKDynamicSqlPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_ai_rag_intent.Agents
{
    public class SKAgentBase
    {
        Kernel _kernel;

        public SKAgentBase(IConfiguration config)
        {
            var values = config.GetSection("Values");
            string _apiDeploymentName = values.GetValue<string>("ApiDeploymentName");
            string _apiEndpoint = values.GetValue<string>("ApiEndpoint");
            string _apiKey = values.GetValue<string>("ApiKey");
            string _textEmbeddingName = values.GetValue<string>("EmbeddingName");
            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<IConfiguration>(config);
            builder.AddAzureOpenAIChatCompletion(
                _apiDeploymentName,
                _apiEndpoint,
                _apiKey, 
                httpClient: new HttpClient(new CustomHttpHandler())
                );
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var memory = KernelFactory.CreateMemory(builder.Services.BuildServiceProvider());
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.Services.AddSingleton<ISemanticTextMemory>(memory);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            builder.AddAzureOpenAITextEmbeddingGeneration(_textEmbeddingName, _apiEndpoint, _apiKey);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            AddPlugins(builder);

            _kernel = builder.Build();
        }

        virtual internal void AddPlugins(IKernelBuilder builder)
        {
        }

        virtual async public Task<ChatMessageContent> HandleRequestAsync(IChatCompletionService chat, ChatHistory chatHistory, string request)
        {
            var result = await chat.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.8, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                kernel: _kernel);
            return result;
        }
    }
}
