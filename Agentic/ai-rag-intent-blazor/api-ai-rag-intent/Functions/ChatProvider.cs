using api_ai_rag_intent.Agents;
using api_ai_rag_intent.Models;
using api_ai_rag_intent.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace api_ai_rag_intent.Functions
{
    public class ChatProvider
    {
        private readonly ILogger<ChatProvider> _logger;
        private static Kernel? _kernel;
        private readonly IChatCompletionService _chat;
        private readonly ChatHistory _chatHistory;
        private readonly string _aiSearchIndex = Helper.GetEnvironmentVariable("AISearchIndex");
        private readonly string _semanticSearchConfigName = Helper.GetEnvironmentVariable("AISearchSemanticConfigName");

        public ChatProvider(ILogger<ChatProvider> logger, Kernel kernel, IChatCompletionService chat, ChatHistory chatHistory, IServiceProvider services)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistory = chatHistory;
        }

        [Function("ChatProvider")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _chatHistory.Clear();
            _logger.LogInformation("C# HTTP SentimentAnalysis trigger function processed a request.");

            _chatHistory.AddUserMessage("Do NOT format responses as markdown. Do NOT use external data to answer questions");
            _chatHistory.AddAssistantMessage("If the user asks for assignees based on incident status, query for incident status then query for assignee");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var chatRequest = JsonSerializer.Deserialize<ChatProviderRequest>(requestBody);
            if (chatRequest == null || chatRequest.userId == null || chatRequest.sessionId == null || chatRequest.tenantId == null || chatRequest.prompt == null)
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }

            var intent = await Util.Intent.GetIntent(_chat, chatRequest.prompt);
            // The purpose of using an Intent pattern is to allow you to make decisions about how you want to invoke the LLM
            // In the case of RAG, if you can detect the user intent is to related to searching documents, then you can only perform that action when the intent is to search documents
            // this allows you to reduce the token useage and save you TPM and dollars
            SKAgentBase? skAgent = null;
            switch (intent)
            {
                case "home":
                    {
                        Console.WriteLine("Intent: home");
                        skAgent = _kernel!.Services.GetService<HomeAgent>();
                        _chatHistory.AddUserMessage(chatRequest.prompt);
                        break;
                    }
                case "sql":
                    {
                        Console.WriteLine("Intent: sql");
                        skAgent = _kernel!.Services.GetService<SqlAgent>();
                        _chatHistory.AddUserMessage(chatRequest.prompt);
                        break;
                    }
                case "not_found":
                    {
                        _chatHistory.AddUserMessage("Simply reponse in a polite way that the request is not related to documents or Subgraph Queries so you are unable to help.");
                        Console.WriteLine("Intent: not_found");
                        break;
                    }

            }
            ChatMessageContent? result = null;

            if (intent != "not_found")
            {
                result = await skAgent!.HandleRequestAsync(_chat, _chatHistory, chatRequest.prompt);
            }

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                var notfound = @$"Your question isn't related to anything I have knowledge of, so I am unable to help. Please ask a question related to home settings, customers, or incidents.";
                await response.WriteStringAsync(result?.Content ?? notfound);
            }
            catch (Exception ex)
            {
                // Log exception details here
                Console.WriteLine(ex.ToString());
                throw; // Re-throw the exception to propagate it further
            }

            return response;
        }
    }
}
