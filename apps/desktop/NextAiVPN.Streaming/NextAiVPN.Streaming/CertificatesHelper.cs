using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace NextAiVPN.Streaming;

public class CertificatesHelper : ICertificatesHelper
{
	public void AddCertificate(string fileName, string certificateName, byte[] fileBytes)
	{
		using X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
		x509Store.Open(OpenFlags.ReadWrite);
		try
		{
			string text = Path.Combine(Directory.GetCurrentDirectory(), fileName);
			if (!File.Exists(text))
			{
				File.WriteAllBytes(text, fileBytes);
				Logger.Log.Information("Copied " + text, "AddCertificate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\CertificatesHelper.cs", 27);
			}
			if (File.Exists(text))
			{
				using (X509Certificate2 certificate = X509CertificateLoader.LoadCertificateFromFile(text))
				{
					x509Store.Add(certificate);
				}
				Logger.Log.Information("Certificate " + certificateName + " has been added!", "AddCertificate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\CertificatesHelper.cs", 37);
				File.Delete(text);
			}
		}
		catch (Exception ex)
		{
			Logger.Log.Error(ex.Message, "AddCertificate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\CertificatesHelper.cs", 43);
		}
	}
}
