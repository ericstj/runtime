// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Security.Principal
{
    public enum WindowsAccountType
    {
        Normal = 0,
        Guest = 1,
        System = 2,
        Anonymous = 3
    }

    public partial class WindowsIdentity : System.Security.Claims.ClaimsIdentity, System.IDisposable, System.Runtime.Serialization.IDeserializationCallback, System.Runtime.Serialization.ISerializable
    {
        public new const string DefaultIssuer = "AD AUTHORITY";
        public WindowsIdentity(System.IntPtr userToken) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public WindowsIdentity(System.IntPtr userToken, string type) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public WindowsIdentity(System.IntPtr userToken, string type, System.Security.Principal.WindowsAccountType acctType) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public WindowsIdentity(System.IntPtr userToken, string type, System.Security.Principal.WindowsAccountType acctType, bool isAuthenticated) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public WindowsIdentity(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        protected WindowsIdentity(System.Security.Principal.WindowsIdentity identity) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public WindowsIdentity(string sUserPrincipalName) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public Microsoft.Win32.SafeHandles.SafeAccessTokenHandle AccessToken { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public sealed override string? AuthenticationType { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public override System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> Claims { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public virtual System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> DeviceClaims { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public System.Security.Principal.IdentityReferenceCollection? Groups { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public System.Security.Principal.TokenImpersonationLevel ImpersonationLevel { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public virtual bool IsAnonymous { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public override bool IsAuthenticated { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public virtual bool IsGuest { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public virtual bool IsSystem { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public override string Name { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public System.Security.Principal.SecurityIdentifier? Owner { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public virtual System.IntPtr Token { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public System.Security.Principal.SecurityIdentifier? User { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public virtual System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> UserClaims { get { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  } }
        public override System.Security.Claims.ClaimsIdentity Clone() { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        public static System.Security.Principal.WindowsIdentity GetAnonymous() { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static System.Security.Principal.WindowsIdentity GetCurrent() { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static System.Security.Principal.WindowsIdentity? GetCurrent(bool ifImpersonating) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static System.Security.Principal.WindowsIdentity GetCurrent(System.Security.Principal.TokenAccessLevels desiredAccess) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static void RunImpersonated(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Action action) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static System.Threading.Tasks.Task RunImpersonatedAsync(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Func<System.Threading.Tasks.Task> func) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static System.Threading.Tasks.Task<T> RunImpersonatedAsync<T>(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Func<System.Threading.Tasks.Task<T>> func) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        public static T RunImpersonated<T>(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Func<T> func) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        void System.Runtime.Serialization.IDeserializationCallback.OnDeserialization(object? sender) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { throw new System.PlatformNotSupportedException(System.SR.PlatformNotSupported_Principal);  }
    }
}
