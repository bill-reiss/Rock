// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;

namespace SKDynamicSqlPlugin;

/// <summary>
/// Utility class to assist in resolving file-system paths.
/// </summary>
internal static class Repo
{
    private static string RootFolder { get; } = GetRoot();

    public static string RootConfigFolder { get; } = $@"{RootFolder}";

    private static string GetRoot()
    {
        var current = Environment.CurrentDirectory;

        var folder = new DirectoryInfo(current);

        return folder.FullName;
    }
}
