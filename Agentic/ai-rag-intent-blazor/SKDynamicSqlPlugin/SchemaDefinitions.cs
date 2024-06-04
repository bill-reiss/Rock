// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Configuration;

namespace SKDynamicSqlPlugin;

/// <summary>
/// Defines the schemas initialized by the console.
/// </summary>
internal static class SchemaDefinitions
{
    /// <summary>
    /// Enumerates the names of the schemas to be registered with the console.
    /// </summary>
    /// <remarks>
    /// After testing with the sample data-sources, try one of your own!
    /// </remarks>
    public static IEnumerable<string> GetNames(IConfiguration config)
    {
        var schemas = config.GetSection("Schemas").GetChildren().Select(x => x.Value).ToArray();
        return schemas ?? Array.Empty<string>();
    }
}
