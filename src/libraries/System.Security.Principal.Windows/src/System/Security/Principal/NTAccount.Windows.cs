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
        private static unsafe IdentityReferenceCollection TranslateToSids(IdentityReferenceCollection sourceAccounts, out bool someFailed)
        {
            ArgumentNullException.ThrowIfNull(sourceAccounts);

            if (sourceAccounts.Count == 0)
            {
                throw new ArgumentException(SR.Arg_EmptyCollection, nameof(sourceAccounts));
            }

            SafeLsaPolicyHandle? LsaHandle = null;
            SafeLsaMemoryHandle? ReferencedDomainsPtr = null;
            SafeLsaMemoryHandle? SidsPtr = null;

            try
            {
                //
                // Construct an array of unicode strings
                //

                Interop.Advapi32.MARSHALLED_UNICODE_STRING[] Names = new Interop.Advapi32.MARSHALLED_UNICODE_STRING[sourceAccounts.Count];

                int currentName = 0;
                foreach (IdentityReference id in sourceAccounts)
                {
                    if (!(id is NTAccount nta))
                    {
                        throw new ArgumentException(SR.Argument_ImproperType, nameof(sourceAccounts));
                    }

                    Names[currentName].Buffer = nta.ToString();

                    if (Names[currentName].Buffer.Length * 2 + 2 > ushort.MaxValue)
                    {
                        // this should never happen since we are already validating account name length in constructor and
                        // it is less than this limit
                        Debug.Fail("NTAccount::TranslateToSids - source account name is too long.");
                        throw new InvalidOperationException();
                    }

                    Names[currentName].Length = (ushort)(Names[currentName].Buffer.Length * 2);
                    Names[currentName].MaximumLength = (ushort)(Names[currentName].Length + 2);
                    currentName++;
                }

                //
                // Open LSA policy (for lookup requires it)
                //

                LsaHandle = Win32.LsaOpenPolicy(null, Interop.Advapi32.PolicyRights.POLICY_LOOKUP_NAMES);

                //
                // Now perform the actual lookup
                //

                someFailed = false;
                uint ReturnCode;

                ReturnCode = Interop.Advapi32.LsaLookupNames2(LsaHandle, 0, sourceAccounts.Count, Names, out ReferencedDomainsPtr, out SidsPtr);

                //
                // Make a decision regarding whether it makes sense to proceed
                // based on the return code and the value of the forceSuccess argument
                //

                if (ReturnCode == Interop.StatusOptions.STATUS_NO_MEMORY ||
                    ReturnCode == Interop.StatusOptions.STATUS_INSUFFICIENT_RESOURCES)
                {
                    throw new OutOfMemoryException();
                }
                else if (ReturnCode == Interop.StatusOptions.STATUS_ACCESS_DENIED)
                {
                    throw new UnauthorizedAccessException();
                }
                else if (ReturnCode == Interop.StatusOptions.STATUS_NONE_MAPPED ||
                    ReturnCode == Interop.StatusOptions.STATUS_SOME_NOT_MAPPED)
                {
                    someFailed = true;
                }
                else if (ReturnCode != 0)
                {
                    uint win32ErrorCode = Interop.Advapi32.LsaNtStatusToWinError(ReturnCode);

                    if (unchecked((int)win32ErrorCode) != Interop.Errors.ERROR_TRUSTED_RELATIONSHIP_FAILURE)
                    {
                        Debug.Fail($"Interop.LsaLookupNames(2) returned unrecognized error {win32ErrorCode}");
                    }

                    throw new Win32Exception(unchecked((int)win32ErrorCode));
                }

                //
                // Interpret the results and generate SID objects
                //

                IdentityReferenceCollection Result = new IdentityReferenceCollection(sourceAccounts.Count);

                if (ReturnCode == 0 || ReturnCode == Interop.StatusOptions.STATUS_SOME_NOT_MAPPED)
                {
                    SidsPtr.Initialize((uint)sourceAccounts.Count, (uint)sizeof(Interop.LSA_TRANSLATED_SID2));
                    ReferencedDomainsPtr.InitializeReferencedDomainsList();
                    Interop.LSA_TRANSLATED_SID2[] translatedSids = new Interop.LSA_TRANSLATED_SID2[sourceAccounts.Count];
                    SidsPtr.ReadArray(0, translatedSids, 0, translatedSids.Length);

                    for (int i = 0; i < sourceAccounts.Count; i++)
                    {
                        Interop.LSA_TRANSLATED_SID2 Lts = translatedSids[i];

                        //
                        // Only some names are recognized as NTAccount objects
                        //

                        switch ((SidNameUse)Lts.Use)
                        {
                            case SidNameUse.User:
                            case SidNameUse.Group:
                            case SidNameUse.Alias:
                            case SidNameUse.Computer:
                            case SidNameUse.WellKnownGroup:
                                Result.Add(new SecurityIdentifier(Lts.Sid));
                                break;

                            default:
                                someFailed = true;
                                Result.Add(sourceAccounts[i]);
                                break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sourceAccounts.Count; i++)
                    {
                        Result.Add(sourceAccounts[i]);
                    }
                }

                return Result;
            }
            finally
            {
                LsaHandle?.Dispose();
                ReferencedDomainsPtr?.Dispose();
                SidsPtr?.Dispose();
            }
        }
    }
}
