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
        //
        // Constructs a SecurityIdentifier object from an IntPtr
        //

        public SecurityIdentifier(IntPtr binaryForm)
            : this(Win32.ConvertIntPtrSidToByteArraySid(binaryForm), 0)
        {
        }

        //
        // Constructs a SecurityIdentifier object from its string representation
        // Returns 'null' if string passed in is not a valid SID
        // NOTE: although there is a P/Invoke call involved in the implementation of this method,
        //       there is no security risk involved, so no security demand is being made.
        //


        public SecurityIdentifier(string sddlForm)
        {
            ArgumentNullException.ThrowIfNull(sddlForm);

            //
            // Call into the underlying O/S conversion routine
            //

            int error = Win32.CreateSidFromString(sddlForm, out byte[]? resultSid);

            if (error == Interop.Errors.ERROR_INVALID_SID)
            {
                throw new ArgumentException(SR.Argument_InvalidValue, nameof(sddlForm));
            }
            else if (error == Interop.Errors.ERROR_NOT_ENOUGH_MEMORY)
            {
                throw new OutOfMemoryException();
            }
            else if (error != Interop.Errors.ERROR_SUCCESS)
            {
                Debug.Fail($"Win32.CreateSidFromString returned unrecognized error {error}");
                throw new Win32Exception(error);
            }

            CreateFromBinaryForm(resultSid!, 0);
        }

        //
        // Constructs a well-known SID
        // The 'domainSid' parameter is optional and only used
        // by the well-known types that require it
        // NOTE: although there is a P/Invoke call involved in the implementation of this constructor,
        //       there is no security risk involved, so no security demand is being made.
        //


        public SecurityIdentifier(WellKnownSidType sidType, SecurityIdentifier? domainSid)
        {
            //
            // sidType must not be equal to LogonIdsSid
            //

            if (sidType == WellKnownSidType.LogonIdsSid)
            {
                throw new ArgumentException(SR.IdentityReference_CannotCreateLogonIdsSid, nameof(sidType));
            }

            //
            // sidType should not exceed the max defined value
            //

            if ((sidType < WellKnownSidType.NullSid) || (sidType > WellKnownSidType.WinCapabilityRemovableStorageSid))
            {
                throw new ArgumentException(SR.Argument_InvalidValue, nameof(sidType));
            }

            //
            // for sidType between 38 to 50, the domainSid parameter must be specified
            //
            int error;
            if ((sidType >= WellKnownSidType.AccountAdministratorSid) && (sidType <= WellKnownSidType.AccountRasAndIasServersSid))
            {
                if (domainSid == null)
                {
                    throw new ArgumentNullException(nameof(domainSid), SR.Format(SR.IdentityReference_DomainSidRequired, sidType));
                }

                //
                // verify that the domain sid is a valid windows domain sid
                // to do that we call GetAccountDomainSid and the return value should be the same as the domainSid
                //
                error = Win32.GetWindowsAccountDomainSid(domainSid, out SecurityIdentifier? resultDomainSid);

                if (error == Interop.Errors.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new OutOfMemoryException();
                }
                else if (error == Interop.Errors.ERROR_NON_ACCOUNT_SID)
                {
                    // this means that the domain sid is not valid
                    throw new ArgumentException(SR.IdentityReference_NotAWindowsDomain, nameof(domainSid));
                }
                else if (error != Interop.Errors.ERROR_SUCCESS)
                {
                    Debug.Fail($"Win32.GetWindowsAccountDomainSid returned unrecognized error {error}");
                    throw new Win32Exception(error);
                }

                //
                // if domainSid is passed in as S-1-5-21-3-4-5-6,  the above api will return S-1-5-21-3-4-5 as the domainSid
                // Since these do not match S-1-5-21-3-4-5-6 is not a valid domainSid (wrong number of subauthorities)
                //
                if (resultDomainSid != domainSid)
                {
                    throw new ArgumentException(SR.IdentityReference_NotAWindowsDomain, nameof(domainSid));
                }
            }


            error = Win32.CreateWellKnownSid(sidType, domainSid, out byte[]? resultSid);

            if (error == Interop.Errors.ERROR_INVALID_PARAMETER)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly, combination of arguments used
                throw new ArgumentException(Marshal.GetPInvokeErrorMessage(error), "sidType/domainSid");
#pragma warning restore CS2208
            }
            else if (error != Interop.Errors.ERROR_SUCCESS)
            {
                Debug.Fail($"Win32.CreateWellKnownSid returned unrecognized error {error}");
                throw new Win32Exception(error);
            }

            CreateFromBinaryForm(resultSid!, 0);
        }


        internal SecurityIdentifier? GetAccountDomainSid()
        {
            int error = Win32.GetWindowsAccountDomainSid(this, out SecurityIdentifier? resultSid);

            if (error == Interop.Errors.ERROR_INSUFFICIENT_BUFFER)
            {
                throw new OutOfMemoryException();
            }
            else if (error == Interop.Errors.ERROR_NON_ACCOUNT_SID)
            {
                resultSid = null;
            }
            else if (error != Interop.Errors.ERROR_SUCCESS)
            {
                Debug.Fail($"Win32.GetWindowsAccountDomainSid returned unrecognized error {error}");
                throw new Win32Exception(error);
            }
            return resultSid;
        }

        public bool IsWellKnown(WellKnownSidType type)
        {
            return Win32.IsWellKnownSid(this, type);
        }

        public bool IsEqualDomainSid(SecurityIdentifier sid)
        {
            return Win32.IsEqualDomainSid(this, sid);
        }

        private static unsafe IdentityReferenceCollection TranslateToNTAccounts(IdentityReferenceCollection sourceSids, out bool someFailed)
        {
            ArgumentNullException.ThrowIfNull(sourceSids);

            if (sourceSids.Count == 0)
            {
                throw new ArgumentException(SR.Arg_EmptyCollection, nameof(sourceSids));
            }

            IntPtr[] SidArrayPtr = new IntPtr[sourceSids.Count];
            GCHandle[] HandleArray = new GCHandle[sourceSids.Count];
            SafeLsaPolicyHandle? LsaHandle = null;
            SafeLsaMemoryHandle? ReferencedDomainsPtr = null;
            SafeLsaMemoryHandle? NamesPtr = null;

            try
            {
                //
                // Pin all elements in the array of SIDs
                //

                int currentSid = 0;
                foreach (IdentityReference id in sourceSids)
                {
                    if (!(id is SecurityIdentifier sid))
                    {
                        throw new ArgumentException(SR.Argument_ImproperType, nameof(sourceSids));
                    }

                    HandleArray[currentSid] = GCHandle.Alloc(sid.BinaryForm, GCHandleType.Pinned);
                    SidArrayPtr[currentSid] = HandleArray[currentSid].AddrOfPinnedObject();
                    currentSid++;
                }

                //
                // Open LSA policy (for lookup requires it)
                //

                LsaHandle = Win32.LsaOpenPolicy(null, Interop.Advapi32.PolicyRights.POLICY_LOOKUP_NAMES);

                //
                // Perform the actual lookup
                //

                someFailed = false;
                uint ReturnCode;
                ReturnCode = Interop.Advapi32.LsaLookupSids(LsaHandle, sourceSids.Count, SidArrayPtr, out ReferencedDomainsPtr, out NamesPtr);

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

                    Debug.Fail($"Interop.LsaLookupSids returned {win32ErrorCode}");
                    throw new Win32Exception(unchecked((int)win32ErrorCode));
                }


                NamesPtr.Initialize((uint)sourceSids.Count, (uint)sizeof(Interop.LSA_TRANSLATED_NAME));
                ReferencedDomainsPtr.InitializeReferencedDomainsList();

                //
                // Interpret the results and generate NTAccount objects
                //

                IdentityReferenceCollection Result = new IdentityReferenceCollection(sourceSids.Count);

                if (ReturnCode == 0 || ReturnCode == Interop.StatusOptions.STATUS_SOME_NOT_MAPPED)
                {
                    //
                    // Interpret the results and generate NT Account objects
                    //

                    Interop.LSA_REFERENCED_DOMAIN_LIST rdl = ReferencedDomainsPtr.Read<Interop.LSA_REFERENCED_DOMAIN_LIST>(0);
                    string[] ReferencedDomains = new string[rdl.Entries];

                    for (int i = 0; i < rdl.Entries; i++)
                    {
                        Interop.LSA_TRUST_INFORMATION* ti = (Interop.LSA_TRUST_INFORMATION*)rdl.Domains + i;
                        ReferencedDomains[i] = Marshal.PtrToStringUni(ti->Name.Buffer, ti->Name.Length / sizeof(char));
                    }

                    Interop.LSA_TRANSLATED_NAME[] translatedNames = new Interop.LSA_TRANSLATED_NAME[sourceSids.Count];
                    NamesPtr.ReadArray(0, translatedNames, 0, translatedNames.Length);

                    for (int i = 0; i < sourceSids.Count; i++)
                    {
                        Interop.LSA_TRANSLATED_NAME Ltn = translatedNames[i];

                        switch ((SidNameUse)Ltn.Use)
                        {
                            case SidNameUse.User:
                            case SidNameUse.Group:
                            case SidNameUse.Alias:
                            case SidNameUse.Computer:
                            case SidNameUse.WellKnownGroup:
                                string account = Marshal.PtrToStringUni(Ltn.Name.Buffer, Ltn.Name.Length / sizeof(char));
                                string domain = ReferencedDomains[Ltn.DomainIndex];
                                Result.Add(new NTAccount(domain, account));
                                break;

                            default:
                                someFailed = true;
                                Result.Add(sourceSids[i]);
                                break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sourceSids.Count; i++)
                    {
                        Result.Add(sourceSids[i]);
                    }
                }

                return Result;
            }
            finally
            {
                for (int i = 0; i < sourceSids.Count; i++)
                {
                    if (HandleArray[i].IsAllocated)
                    {
                        HandleArray[i].Free();
                    }
                }

                LsaHandle?.Dispose();
                ReferencedDomainsPtr?.Dispose();
                NamesPtr?.Dispose();
            }
        }
    }
}
