// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Principal
{
    public partial class SecurityIdentifier
    {
        public SecurityIdentifier(IntPtr binaryForm)
        {
            throw new PlatformNotSupportedException();
        }

        public SecurityIdentifier(string sddlForm)
        {
            throw new PlatformNotSupportedException();
        }

        public SecurityIdentifier(WellKnownSidType sidType, SecurityIdentifier? domainSid)
        {
            throw new PlatformNotSupportedException();
        }

        #pragma warning disable CA1822
        internal SecurityIdentifier? GetAccountDomainSid()
        #pragma warning restore CA1822
        {
            throw new PlatformNotSupportedException();
        }

        public bool IsWellKnown(WellKnownSidType type)
        {
            throw new PlatformNotSupportedException();
        }

        public bool IsEqualDomainSid(SecurityIdentifier sid)
        {
            throw new PlatformNotSupportedException();
        }

        #pragma warning disable IDE0060
        private static unsafe IdentityReferenceCollection TranslateToNTAccounts(IdentityReferenceCollection sourceSids, out bool someFailed)
        #pragma warning restore IDE0060
        {
            throw new PlatformNotSupportedException();
        }
    }
}
