// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.Text.Json.Generator.Tests
{
    public static partial class GeneratorTests
    {
        [Fact]
        public static void CanUse()
        {
            JsonGenerator generator = new JsonGenerator();
            generator.Initialize();
            generator.Execute(null);
        }
    }
}
