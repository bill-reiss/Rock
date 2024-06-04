using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json;
using SemanticKernel.Data.Nl2Sql.Library;
using SemanticKernel.Data.Nl2Sql.Library.Schema;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace SKDynamicSqlPlugin
{
    public class SqlServerPlugin
    {
        bool _initialized;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        private ISemanticTextMemory _memory;
        private IConfiguration _config;
        private SqlQueryGenerator? _queryGenerator;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        public SqlServerPlugin(ISemanticTextMemory memory, IConfiguration config)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        {
            _memory = memory;
            _config = config;
        }

        [KernelFunction]
        [Description("Converts a natural language query to SQL and executes against a SQL database")]
        public async Task<string> QueryAsync([Description("The original question")] string prompt, [Description("The kernel instance")] Kernel kernel)
        {
            if (!_initialized)
            {
                _initialized = true;
                var schemaNames = SchemaDefinitions.GetNames(_config).ToArray();
                await SchemaProvider.InitializeAsync(
                    _memory,
                    schemaNames.Select(s => Path.Combine(Repo.RootConfigFolder, "schemas", $"{s}"))).ConfigureAwait(false);
                string isQueryText = GetResourceFileContents("isQuery.xml");
                string generateQueryText = GetResourceFileContents("generateQuery.xml");
                _queryGenerator = new SqlQueryGenerator(kernel, _memory, isQueryText, generateQueryText, 0.7);
            }

            var req = await _queryGenerator!.SolveObjectiveAsync(prompt);
            if (req == null)
            {
                Console.WriteLine("Query generation failed");
                return "Could not generate query";
            }
            else
            {
                Console.WriteLine($"Query: {req.Query}");
            }
            var sqlProvider = new SqlConnectionProvider(_config);
            var conn = await sqlProvider.ConnectAsync(req!.Schema);
            using var command = conn.CreateCommand();
            command.CommandText = req.Query;
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var result = SqlDatoToJsonOrScalar(reader);
            Console.WriteLine($"Result: {result}");
            return result;
        }

        private string GetResourceFileContents(string resourceFilename)
        {
            var assembly = Assembly.GetAssembly(typeof(SqlServerPlugin))!;
            using var stream = assembly.GetManifestResourceStream($"SKDynamicSqlPlugin.{resourceFilename}");
            using var sr = new StreamReader(stream!);
            return sr.ReadToEnd();
        }

        private string? SqlDatoToJsonOrScalar(SqlDataReader dataReader)
        {
            var dataTable = new DataTable();
            dataTable.Load(dataReader);
            if (dataTable.Rows.Count == 0)
            {
                return null;
            }
            if (dataTable.Rows.Count == 1 && dataTable.Columns.Count == 1)
            {
                var singleValue = dataTable.Rows[0][0].ToString();
                return dataTable.Rows[0][0].ToString();
            }
            else
            {
                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(dataTable);
                return JSONString;
            }
        }
    }
}
