// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Extensions.Logging.Generators
{
    public partial class LoggerMessageGenerator
    {
        /// <summary> Full type name, used for diagnostics </summary>
        private static readonly string s_generatorName = typeof(LoggerMessageGenerator).FullName!;

        /// <summary> Assembly location, used for diagnostics </summary>
        private static readonly string s_generatorLocation = typeof(LoggerMessageGenerator).Assembly.Location;
    }
}
