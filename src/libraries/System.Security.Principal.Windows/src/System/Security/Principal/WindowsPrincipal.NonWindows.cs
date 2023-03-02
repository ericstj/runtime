// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace System.Security.Principal
{
    public partial class WindowsPrincipal : ClaimsPrincipal
    {
        public virtual bool IsInRole(SecurityIdentifier sid)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
