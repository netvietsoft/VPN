using System;

namespace NextAiVPN.Streaming.Exceptions;

public class CredentialStoreException : Exception
{
	public CredentialStoreException()
		: base("Unable to read credentials from Windows Credential Manager.")
	{
	}

	public CredentialStoreException(string message)
		: base(message)
	{
	}

	public CredentialStoreException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
