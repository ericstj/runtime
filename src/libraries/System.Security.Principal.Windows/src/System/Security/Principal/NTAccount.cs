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
    public sealed partial class NTAccount : IdentityReference
    {
        #region Private members

        private readonly string _name;

        //
        // Limit for nt account names for users is 20 while that for groups is 256
        //
        internal const int MaximumAccountNameLength = 256;

        //
        // Limit for dns domain names is 255
        //
        internal const int MaximumDomainNameLength = 255;

        #endregion

        #region Constructors

        public NTAccount(string domainName, string accountName)
        {
            ArgumentException.ThrowIfNullOrEmpty(accountName);

            if (accountName.Length > MaximumAccountNameLength)
            {
                throw new ArgumentException(SR.IdentityReference_AccountNameTooLong, nameof(accountName));
            }

            if (domainName != null && domainName.Length > MaximumDomainNameLength)
            {
                throw new ArgumentException(SR.IdentityReference_DomainNameTooLong, nameof(domainName));
            }

            if (domainName == null || domainName.Length == 0)
            {
                _name = accountName;
            }
            else
            {
                _name = domainName + "\\" + accountName;
            }
        }

        public NTAccount(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            if (name.Length > (MaximumDomainNameLength + 1 /* '\' */ + MaximumAccountNameLength))
            {
                throw new ArgumentException(SR.IdentityReference_AccountNameTooLong, nameof(name));
            }

            _name = name;
        }

        #endregion

        #region Inherited properties and methods
        public override string Value
        {
            get
            {
                return ToString();
            }
        }

        public override bool IsValidTargetType(Type targetType)
        {
            if (targetType == typeof(SecurityIdentifier))
            {
                return true;
            }
            else if (targetType == typeof(NTAccount))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override IdentityReference Translate(Type targetType)
        {
            ArgumentNullException.ThrowIfNull(targetType);

            if (targetType == typeof(NTAccount))
            {
                return this; // assumes that NTAccount objects are immutable
            }
            else if (targetType == typeof(SecurityIdentifier))
            {
                IdentityReferenceCollection irSource = new IdentityReferenceCollection(1);
                irSource.Add(this);
                IdentityReferenceCollection irTarget;

                irTarget = NTAccount.Translate(irSource, targetType, true);

                return irTarget[0];
            }
            else
            {
                throw new ArgumentException(SR.IdentityReference_MustBeIdentityReference, nameof(targetType));
            }
        }

        public override bool Equals([NotNullWhen(true)] object? o)
        {
            return (this == o as NTAccount); // invokes operator==
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(_name);
        }

        public override string ToString()
        {
            return _name;
        }

        internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceAccounts, Type targetType, bool forceSuccess)
        {
            IdentityReferenceCollection result = Translate(sourceAccounts, targetType, out bool someFailed);

            if (forceSuccess && someFailed)
            {
                IdentityReferenceCollection UnmappedIdentities = new IdentityReferenceCollection();

                foreach (IdentityReference id in result)
                {
                    if (id.GetType() != targetType)
                    {
                        UnmappedIdentities.Add(id);
                    }
                }

                throw new IdentityNotMappedException(SR.IdentityReference_IdentityNotMapped, UnmappedIdentities);
            }

            return result;
        }

        internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceAccounts, Type targetType, out bool someFailed)
        {
            ArgumentNullException.ThrowIfNull(sourceAccounts);

            if (targetType == typeof(SecurityIdentifier))
            {
                return TranslateToSids(sourceAccounts, out someFailed);
            }

            throw new ArgumentException(SR.IdentityReference_MustBeIdentityReference, nameof(targetType));
        }

        #endregion

        #region Operators

        public static bool operator ==(NTAccount? left, NTAccount? right)
        {
            object? l = left;
            object? r = right;

            if (l == r)
            {
                return true;
            }
            else if (l == null || r == null)
            {
                return false;
            }
            else
            {
                return (left!.ToString().Equals(right!.ToString(), StringComparison.OrdinalIgnoreCase));
            }
        }

        public static bool operator !=(NTAccount? left, NTAccount? right)
        {
            return !(left == right); // invoke operator==
        }

        #endregion
    }
}
