using System;
using System.Runtime.InteropServices;

namespace NextAiVPN.Streaming;

internal static class VpnCredentialsSetter
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct RASCREDENTIALS
	{
		public int dwSize;

		public int dwMask;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szUserName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szPassword;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
		public string szDomain;
	}

	public const int RASCM_Username = 1;

	public const int RASCM_Password = 2;

	[DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
	public static extern uint RasSetCredentials(string lpszPhonebook, string lpszEntry, ref RASCREDENTIALS lpCredentials, bool fClearCredentials);

	public static void SetVpnCredentials(string entryName, string userName, string password)
	{
		RASCREDENTIALS lpCredentials = new RASCREDENTIALS
		{
			dwSize = Marshal.SizeOf(typeof(RASCREDENTIALS)),
			szUserName = userName,
			szPassword = password,
			dwMask = 3
		};
		uint num = RasSetCredentials(null, entryName, ref lpCredentials, fClearCredentials: false);
		if (num != 0)
		{
			throw new Exception("Failed to set VPN credentials. Error code: " + num);
		}
	}
}
