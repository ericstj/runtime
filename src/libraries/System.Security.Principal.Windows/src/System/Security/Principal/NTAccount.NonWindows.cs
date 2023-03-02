// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Principal
{
    public partial class NTAccount
    {
        #pragma warning disable IDE0060
        private static unsafe IdentityReferenceCollection TranslateToSids(IdentityReferenceCollection sourceAccounts, out bool someFailed)
        #pragma warning restore IDE0060
        {
            throw new PlatformNotSupportedException();
        }
    }
}
