using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using VpnSDK.Core.Helpers;
using VpnSDK.Enums;
using VpnSDK.Private.OpenVpn.Enums;

namespace VpnSDK.DTO;

public class OpenVpnConfiguration
{
	private const string OpenVpnAssembliesFolder = "bin";

	private const string OpenVpnDirectoryName = "OpenVPN";

	private const string OpenVpnDriversFolder = "Drivers";

	private readonly string _architecture = string.Empty;

	private string _certFilePath = string.Empty;

	private string _defaultOpenVpnDirectory = string.Empty;

	private string _defaultOpenVpnDriverDirectory = string.Empty;

	private string _openVpnDirectory = string.Empty;

	private string _openVpnDirectoryCommon = string.Empty;

	private string _openVpnDriverDirectory = string.Empty;

	private Dictionary<string, string[]> _openVpnConfig;

	public string OpenVpnDirectory
	{
		get
		{
			return GetDirectoryPath(_openVpnDirectory, _defaultOpenVpnDirectory);
		}
		set
		{
			_openVpnDirectory = NormalizeDirectoryPath(value);
		}
	}

	public string OpenVpnDriverDirectory
	{
		get
		{
			return GetDirectoryPath(_openVpnDriverDirectory, _defaultOpenVpnDriverDirectory);
		}
		set
		{
			_openVpnDriverDirectory = NormalizeDirectoryPath(value);
		}
	}

	public string OpenVpnExecutableFileName { get; set; } = "openvpn.exe";

	public string CertificateAuthorityFilePath
	{
		get
		{
			if (!string.IsNullOrEmpty(_certFilePath))
			{
				return _certFilePath;
			}
			_certFilePath = Path.Combine(_openVpnDirectoryCommon, "ca.crt");
			return _certFilePath;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (Path.IsPathRooted(value))
				{
					_certFilePath = value;
				}
				else
				{
					_certFilePath = Path.Combine(OpenVpnDirectory, value).TrimEnd(new char[1] { '\\' });
				}
				ConfigurationFileOptions["ca"] = new string[1] { "\"" + _certFilePath.Replace("\\", "\\\\") + "\"" };
			}
			else if (ConfigurationFileOptions.ContainsKey("ca"))
			{
				ConfigurationFileOptions.Remove("ca");
			}
		}
	}

	public string TapDeviceFriendlyName { get; set; } = "WLVPN Windows Tap Adapter";

	public OpenVpnTapAdapter PreferredTapAdapter { get; set; }

	public OpenVpnLogLevel LogLevel { get; set; } = OpenVpnLogLevel.Normal;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public Dictionary<string, string[]> ConfigurationFileOptions
	{
		get
		{
			if (_openVpnConfig == null)
			{
				_openVpnConfig = new Dictionary<string, string[]>
				{
					{ "client", null },
					{
						"dev",
						new string[1] { "tun" }
					},
					{
						"resolv-retry",
						new string[1] { "infinite" }
					},
					{ "nobind", null },
					{ "persist-key", null },
					{ "persist-tun", null },
					{ "persist-remote-ip", null },
					{
						"remote-cert-tls",
						new string[1] { "server" }
					},
					{
						"ca",
						new string[1] { "\"" + CertificateAuthorityFilePath.Replace("\\", "\\\\") + "\"" }
					},
					{
						"allow-compression",
						new string[1] { "asym" }
					},
					{
						"verb",
						new string[1] { $"{(int)LogLevel}" }
					},
					{
						"auth",
						new string[1] { "SHA256" }
					},
					{
						"tls-cipher",
						new string[1] { "TLS-DHE-RSA-WITH-AES-256-CBC-SHA:TLS-DHE-DSS-WITH-AES-256-CBC-SHA:TLS-RSA-WITH-AES-256-CBC-SHA" }
					},
					{ "auth-user-pass", null },
					{
						"remap-usr1",
						new string[1] { "SIGTERM" }
					},
					{
						"hand-window",
						new string[1] { "30" }
					}
				};
			}
			return _openVpnConfig;
		}
		set
		{
			_openVpnConfig = value;
		}
	}

	internal TapDriver InstalledTapDriver { get; set; }

	public OpenVpnConfiguration()
	{
		_openVpnDirectoryCommon = Path.Combine(PathHelper.GetApplicationDirectory(), "OpenVPN", TapDriver.tapwlvpn.ToString());
		_architecture = Utils.GetSystemArchitecture(mapAsAmd64: true);
		if (_architecture == "x86")
		{
			_architecture = "i386";
		}
		_defaultOpenVpnDriverDirectory = Path.Combine(_openVpnDirectoryCommon, "Drivers", _architecture);
		_defaultOpenVpnDirectory = Path.Combine(_openVpnDirectoryCommon, "bin", _architecture);
	}

	internal void SetDefaultOpenVpnDirectory(TapDriver tapDriver)
	{
		InstalledTapDriver = tapDriver;
		_openVpnDirectoryCommon = Path.Combine(PathHelper.GetApplicationDirectory(), "OpenVPN", tapDriver.ToString());
		_defaultOpenVpnDriverDirectory = Path.Combine(_openVpnDirectoryCommon, "Drivers", _architecture);
		if (tapDriver == TapDriver.tap0901)
		{
			_defaultOpenVpnDirectory = Path.Combine(new string[1] { _openVpnDirectoryCommon });
		}
		else
		{
			_defaultOpenVpnDirectory = Path.Combine(_openVpnDirectoryCommon, "bin", _architecture);
		}
	}

	private string GetDirectoryPath(string directory, string defaultDirectory)
	{
		if (!string.IsNullOrEmpty(directory))
		{
			return directory;
		}
		return defaultDirectory;
	}

	private string NormalizeDirectoryPath(string path)
	{
		string text = path;
		if (!Path.IsPathRooted(path))
		{
			text = Path.Combine(PathHelper.GetApplicationDirectory(), path);
			if (!text.EndsWith($"{Path.DirectorySeparatorChar}"))
			{
				string text2 = text;
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				text = text2 + directorySeparatorChar;
			}
		}
		return text;
	}
}
