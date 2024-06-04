using api_ai_rag_intent.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_ai_rag_intent.Agents
{
    public class HomeAgent : SKAgentBase
    {
        public HomeAgent(IConfiguration config) : base(config)
        {

        }

        override internal void AddPlugins(IKernelBuilder builder)
        {
            builder.Plugins.AddFromType<HomeACPlugin>();
            builder.Plugins.AddFromType<HomeLightsPlugin>();
        }
    }
}
