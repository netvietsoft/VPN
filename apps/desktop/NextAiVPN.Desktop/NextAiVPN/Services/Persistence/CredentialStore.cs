using System;
using System.Runtime.InteropServices;
using System.Text;
using NextAiVPN.Common;
using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

internal class CredentialStore : ICredentialStore
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CREDENTIAL
	{
		public int Flags;

		public int Type;

		public string TargetName;

		public string Comment;

		public long LastWritten;

		public int CredentialBlobSize;

		public nint CredentialBlob;

		public int Persist;

		public int AttributeCount;

		public nint Attributes;

		public string TargetAlias;

		public string UserName;
	}

	private const string VpnTarget = "NextAiVpn:Credentials";

	private const int CRED_TYPE_GENERIC = 1;

	private const int CRED_PERSIST_LOCAL_MACHINE = 2;

	private const int ERROR_NOT_FOUND = 1168;

	private readonly IAppLogger _logger;

	[DllImport("Advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CredWriteW", SetLastError = true)]
	private static extern bool CredWrite(ref CREDENTIAL credential, int flags);

	[DllImport("Advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CredReadW", SetLastError = true)]
	private static extern bool CredRead(string target, int type, int reservedFlag, out nint credentialPtr);

	[DllImport("Advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CredDeleteW", SetLastError = true)]
	private static extern bool CredDelete(string target, int type, int flags);

	[DllImport("Advapi32.dll", SetLastError = true)]
	private static extern void CredFree(nint credentialPtr);

	public CredentialStore(IAppLogger logger)
	{
		_logger = logger;
	}

	public void SaveCredentials(VpnCredentials credentials)
	{
		string vpnUsername = credentials.VpnUsername;
		byte[] bytes = Encoding.Unicode.GetBytes(credentials.VpnPassword ?? string.Empty);
		nint num = Marshal.AllocHGlobal((bytes.Length == 0) ? 1 : bytes.Length);
		try
		{
			if (bytes.Length != 0)
			{
				Marshal.Copy(bytes, 0, num, bytes.Length);
			}
			CREDENTIAL credential = new CREDENTIAL
			{
				Type = 1,
				TargetName = "NextAiVpn:Credentials",
				CredentialBlobSize = bytes.Length,
				CredentialBlob = num,
				Persist = 2,
				UserName = vpnUsername
			};
			if (!CredWrite(ref credential, 0))
			{
				InvalidOperationException ex = new InvalidOperationException("Failed to save credentials to Windows Credential Manager. Target: NextAiVpn:Credentials");
				_logger?.Error(ex, "SaveCredentials", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\CredentialStore.cs", 97);
				throw ex;
			}
		}
		finally
		{
			Marshal.FreeHGlobal(num);
		}
	}

	public VpnCredentials GetCredentials()
	{
		if (!CredRead("NextAiVpn:Credentials", 1, 0, out var credentialPtr))
		{
			return null;
		}
		try
		{
			CREDENTIAL cREDENTIAL = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
			string credPassword;
			if (cREDENTIAL.CredentialBlobSize > 0)
			{
				byte[] array = new byte[cREDENTIAL.CredentialBlobSize];
				Marshal.Copy(cREDENTIAL.CredentialBlob, array, 0, cREDENTIAL.CredentialBlobSize);
				credPassword = Encoding.Unicode.GetString(array);
			}
			else
			{
				credPassword = string.Empty;
			}
			return new VpnCredentials(cREDENTIAL.UserName, credPassword);
		}
		finally
		{
			CredFree(credentialPtr);
		}
	}

	public void DeleteCredentials()
	{
		if (!CredDelete("NextAiVpn:Credentials", 1, 0) && Marshal.GetLastWin32Error() != 1168)
		{
			_logger?.Error("Failed to delete credentials to Windows Credential Manager. Target: NextAiVpn:Credentials", "DeleteCredentials", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\CredentialStore.cs", 151);
		}
	}
}
