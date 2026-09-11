using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class AppSettingsXmlFileHelper : IAppSettingsHelper
{
	private readonly string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN\\AppSettings.xml");

	private static readonly HashSet<string> SensitiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"access_token",
		"refresh_token",
		"id_token",
		"CodeVerifier",
		AppSettingsKeys.VpnUsername,
		AppSettingsKeys.VpnPassword
	};

	private readonly object _locker = new object();

	private readonly IAppLogger _logger;

	public AppSettingsXmlFileHelper(IAppLogger logger)
	{
		_logger = logger;
	}

	public string GetValue(string variableName)
	{
		if (string.IsNullOrWhiteSpace(variableName))
		{
			return string.Empty;
		}
		lock (_locker)
		{
			try
			{
				XmlDocument doc = LoadDocument();
				XmlNode xmlNode = FindNode(doc, variableName);
				if (xmlNode == null)
				{
					AddNode(variableName, string.Empty, doc);
					return string.Empty;
				}
				string innerText = xmlNode.InnerText;
				if (string.IsNullOrEmpty(innerText))
				{
					return string.Empty;
				}
				if (!IsSensitive(variableName))
				{
					return innerText;
				}
				if (!Dpapi.IsEncrypted(innerText))
				{
					SetValue(variableName, innerText);
					return innerText;
				}
				return Dpapi.DecryptFromBase64(innerText);
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, "GetValue", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsXmlFileHelper.cs", 86);
				return string.Empty;
			}
		}
	}

	public void SetValue(string variableName, string value)
	{
		if (string.IsNullOrWhiteSpace(variableName))
		{
			return;
		}
		lock (_locker)
		{
			try
			{
				XmlDocument xmlDocument = LoadDocument();
				XmlNode xmlNode = FindNode(xmlDocument, variableName);
				if (xmlNode == null)
				{
					AddNode(variableName, value, xmlDocument);
					return;
				}
				WriteNodeValue(variableName, value, xmlNode);
				xmlDocument.Save(_filePath);
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, "SetValue", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsXmlFileHelper.cs", 119);
			}
		}
	}

	private XmlDocument LoadDocument()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(_filePath);
		return xmlDocument;
	}

	private static XmlNode FindNode(XmlDocument doc, string variableName)
	{
		return doc.SelectSingleNode("/Settings/" + variableName);
	}

	private static void WriteNodeValue(string variableName, string value, XmlNode node)
	{
		node.InnerText = (IsSensitive(variableName) ? Dpapi.EncryptToBase64(value) : (value ?? string.Empty));
	}

	private static bool IsSensitive(string variableName)
	{
		return SensitiveKeys.Contains(variableName);
	}

	private void AddNode(string variableName, string variableValue, XmlDocument doc)
	{
		try
		{
			XmlNode xmlNode = doc.SelectSingleNode("/Settings");
			if (xmlNode == null)
			{
				_logger?.Error("Settings root node not found.", "AddNode", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsXmlFileHelper.cs", 156);
				return;
			}
			XmlElement xmlElement = doc.CreateElement(variableName);
			WriteNodeValue(variableName, variableValue, xmlElement);
			xmlNode.AppendChild(xmlElement);
			doc.Save(_filePath);
			_logger?.Information("Added variable: " + variableName, "AddNode", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsXmlFileHelper.cs", 167);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "AddNode", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsXmlFileHelper.cs", 171);
		}
	}
}
