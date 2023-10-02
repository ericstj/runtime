// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Text.Json.SourceGeneration
{
    public sealed partial class JsonSourceGenerator
    {
        /// <summary> Full type name, used for diagnostics </summary>
        private static readonly string s_generatorName = typeof(JsonSourceGenerator).FullName!;

        /// <summary> Assembly location, used for diagnostics </summary>
        #pragma warning disable IL3000
        private static readonly string s_generatorLocation = typeof(JsonSourceGenerator).Assembly.Location;
        #pragma warning restore IL3000
    }
}
