using System;
using System.Text.RegularExpressions;
using VpnSDK.Private.OpenVpn.Enums;
using VpnSDK.Private.OpenVpn.Exceptions;
using VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output;

internal class OutputParser
{
	private Regex _sigtermRegex;

	private const string ConnectedState = "CONNECTED";

	private const string Error = "ERROR";

	internal OutputParser()
	{
		_sigtermRegex = new Regex("SIGTERM\\[.+?,(.+?)\\]", RegexOptions.Compiled | RegexOptions.Singleline);
	}

	internal IOutputMessage Parse(string data)
	{
		if (data.StartsWith(">", StringComparison.OrdinalIgnoreCase))
		{
			string text = data.Substring(1, data.IndexOf(":", StringComparison.OrdinalIgnoreCase) - 1);
			string text2 = data.Substring(text.Length + 2);
			switch (text)
			{
			case "LOG":
			{
				string value;
				try
				{
					value = _sigtermRegex.Match(text2).Groups[1].Value;
				}
				catch
				{
					return new LogMessage(text2);
				}
				if (!string.IsNullOrEmpty(value))
				{
					if (value == "tls-error")
					{
						return new FatalMessage(new TlsHandshakeException("TLS handshake failed."));
					}
					return new FatalMessage(new OpenVpnException("OpenVPN process was terminated. Reason: '" + value + "'"));
				}
				return new LogMessage(text2);
			}
			case "BYTECOUNT":
			{
				string[] array2 = text2.Split(',');
				if (array2.Length == 2)
				{
					return new BytecountMessage(int.Parse(array2[0]), int.Parse(array2[1]));
				}
				return new UnknownMessage("Unknown bytecount message.");
			}
			case "ECHO":
				return new EchoMessage(text2);
			case "FATAL":
				return new FatalMessage(new OpenVpnException(text2));
			case "HOLD":
				return new HoldMessage();
			case "NEEDOK":
				return new NeedOkMessage(text2);
			case "NEEDSTR":
				try
				{
					string value2 = Regex.Match(text2, "'(.+?)'").Groups[1].Value;
					return new NeedStrMessage(text2, value2);
				}
				catch
				{
					return new NeedStrMessage(text2, null);
				}
			case "AUTH":
				if (text2.Contains("AUTH_FAILED"))
				{
					return new FatalMessage(new AuthenticationException("Authentication failure."));
				}
				return new LogMessage(text2);
			case "PASSWORD":
				if (text2.StartsWith("Need 'Auth'", StringComparison.OrdinalIgnoreCase))
				{
					return new PasswordMessage(text2, AuthenticationState.NEED_USERNAME_PASSWORD);
				}
				if (text2.StartsWith("Need 'Private Key'", StringComparison.OrdinalIgnoreCase))
				{
					return new PasswordMessage(text2, AuthenticationState.NEED_PKEY_PASSWORD);
				}
				return new FatalMessage(new AuthenticationException("Authentication failure."));
			case "STATE":
				try
				{
					string[] array = text2.Split(',');
					string text3 = array[1];
					if (array.Length > 2)
					{
						string text4 = array[2];
						if (text3 == "CONNECTED" && text4 == "ERROR")
						{
							return new FatalMessage(new OpenVpnException("Connection established with an error. Check logs for more information."));
						}
					}
					return new StateMessage((ConnectionState)Enum.Parse(typeof(ConnectionState), text3));
				}
				catch
				{
					return new UnknownMessage(text2);
				}
			default:
				return new UnknownMessage(text2);
			}
		}
		return new UnknownMessage(data);
	}
}
