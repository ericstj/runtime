// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace System.Security.Principal
{
    public enum WindowsBuiltInRole
    {
        Administrator = 0x220,
        User = 0x221,
        Guest = 0x222,
        PowerUser = 0x223,
        AccountOperator = 0x224,
        SystemOperator = 0x225,
        PrintOperator = 0x226,
        BackupOperator = 0x227,
        Replicator = 0x228
    }

    public partial class WindowsPrincipal : ClaimsPrincipal
    {
        private readonly WindowsIdentity _identity;

        //
        // Constructors.
        //

        public WindowsPrincipal(WindowsIdentity ntIdentity)
            : base(ntIdentity)
        {
            ArgumentNullException.ThrowIfNull(ntIdentity);

            _identity = ntIdentity;
        }

        //
        // Properties.
        //
        public override IIdentity Identity => _identity;

        //
        // Public methods.
        //

        public override bool IsInRole(string role)
        {
            if (role == null || role.Length == 0)
                return false;

            NTAccount ntAccount = new NTAccount(role);
            IdentityReferenceCollection source = new IdentityReferenceCollection(1);
            source.Add(ntAccount);
            IdentityReferenceCollection target = NTAccount.Translate(source, typeof(SecurityIdentifier), false);

            if (target[0] is SecurityIdentifier sid)
            {
                if (IsInRole(sid))
                {
                    return true;
                }
            }

            // possible that identity has other role claims that match
            return base.IsInRole(role);
        }

        // <summary
        // Returns all of the claims from all of the identities that are windows user claims
        // found in the NT token.
        // </summary>
        public virtual IEnumerable<Claim> UserClaims
        {
            get
            {
                foreach (ClaimsIdentity identity in Identities)
                {
                    if (identity is WindowsIdentity wi)
                    {
                        foreach (Claim claim in wi.UserClaims)
                        {
                            yield return claim;
                        }
                    }
                }
            }
        }

        // <summary
        // Returns all of the claims from all of the identities that are windows device claims
        // found in the NT token.
        // </summary>
        public virtual IEnumerable<Claim> DeviceClaims
        {
            get
            {
                foreach (ClaimsIdentity identity in Identities)
                {
                    if (identity is WindowsIdentity wi)
                    {
                        foreach (Claim claim in wi.DeviceClaims)
                        {
                            yield return claim;
                        }
                    }
                }
            }
        }

        public virtual bool IsInRole(WindowsBuiltInRole role)
        {
            if (role < WindowsBuiltInRole.Administrator || role > WindowsBuiltInRole.Replicator)
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)role), nameof(role));

            return IsInRole((int)role);
        }

        public virtual bool IsInRole(int rid)
        {
            return IsInRole(
                new SecurityIdentifier(
                    IdentifierAuthority.NTAuthority,
                    stackalloc
                    int[] { Interop.SecurityIdentifier.SECURITY_BUILTIN_DOMAIN_RID, rid }
                )
            );
        }

        // This is called by AppDomain.GetThreadPrincipal() via reflection.
        private static WindowsPrincipal GetDefaultInstance() => new WindowsPrincipal(WindowsIdentity.GetCurrent());
    }
}
