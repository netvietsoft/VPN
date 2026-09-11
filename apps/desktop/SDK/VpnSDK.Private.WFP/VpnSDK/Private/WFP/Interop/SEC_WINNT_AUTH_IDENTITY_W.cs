using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct SEC_WINNT_AUTH_IDENTITY_W
{
	public IntPtr User;

	public uint UserLength;

	public IntPtr Domain;

	public uint DomainLength;

	public IntPtr Password;

	public uint PasswordLength;

	public uint Flags;
}
