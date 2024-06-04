using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SKDynamicSqlPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_ai_rag_intent.Agents
{
    internal class SqlAgent : SKAgentBase
    {
        public SqlAgent(IConfiguration config) : base(config)
        {
        }

        internal override void AddPlugins(IKernelBuilder builder)
        {
            builder.Plugins.AddFromType<SqlServerPlugin>();
        }
    }
}
