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
        // This method (with a SID parameter) is more general than the 2 overloads that accept a WindowsBuiltInRole or
        // a rid (as an int). It is also better from a performance standpoint than the overload that accepts a string.
        // The aforementioned overloads remain in this class since we do not want to introduce a
        // breaking change. However, this method should be used in all new applications.

        public virtual bool IsInRole(SecurityIdentifier sid)
        {
            ArgumentNullException.ThrowIfNull(sid);

            // special case the anonymous identity.
            if (_identity.AccessToken.IsInvalid)
                return false;

            // CheckTokenMembership expects an impersonation token
            using SafeAccessTokenHandle invalidHandle = SafeAccessTokenHandle.InvalidHandle;
            SafeAccessTokenHandle token = invalidHandle;
            try
            {
                if (_identity.ImpersonationLevel == TokenImpersonationLevel.None)
                {
                    if (!Interop.Advapi32.DuplicateTokenEx(_identity.AccessToken,
                                                      (uint)TokenAccessLevels.Query,
                                                      IntPtr.Zero,
                                                      (uint)TokenImpersonationLevel.Identification,
                                                      (uint)TokenType.TokenImpersonation,
                                                      ref token))
                    {
                        throw new SecurityException(Marshal.GetLastPInvokeErrorMessage());
                    }
                }

                bool isMember = false;

                // CheckTokenMembership will check if the SID is both present and enabled in the access token.
                if (!Interop.Advapi32.CheckTokenMembership((_identity.ImpersonationLevel != TokenImpersonationLevel.None ? _identity.AccessToken : token),
                                                      sid.BinaryForm,
                                                      ref isMember))
                {
                    throw new SecurityException(Marshal.GetLastPInvokeErrorMessage());
                }

                return isMember;
            }
            finally
            {
                token.Dispose();
            }
        }

    }
}
