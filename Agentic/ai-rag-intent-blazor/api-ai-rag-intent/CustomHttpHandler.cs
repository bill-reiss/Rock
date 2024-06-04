using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_ai_rag_intent
{
    internal class CustomHttpHandler : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestText = request.Content.ReadAsStringAsync().Result;
            Console.WriteLine("");
            Console.WriteLine(requestText);
            Console.WriteLine("");
            var response = base.SendAsync(request, cancellationToken);
            
            return response;
        }
    }
}
