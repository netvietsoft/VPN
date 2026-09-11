using System;
using System.Runtime.InteropServices;
using ARSoft.Tools.Net.Dns;

namespace VpnSDK.DnsMonitor.DTO;

internal class DnsRequest
{
	private readonly byte[] _dataBuffer;

	public DnsRequest(IntPtr buf, int len)
	{
		if (buf != IntPtr.Zero)
		{
			_dataBuffer = new byte[len];
			Marshal.Copy(buf, _dataBuffer, 0, len);
		}
	}

	public string DecodeUri()
	{
		string result = null;
		try
		{
			if (_dataBuffer == null)
			{
				return null;
			}
			DnsMessage dnsMessage = DnsMessage.Parse(_dataBuffer);
			if (dnsMessage != null && dnsMessage.IsQuery && dnsMessage.OperationCode == OperationCode.Query && dnsMessage.Questions.Count > 0 && dnsMessage.AnswerRecords.Count == 0 && dnsMessage.AuthorityRecords.Count == 0)
			{
				DnsQuestion dnsQuestion = dnsMessage.Questions[0];
				if (dnsQuestion != null && (dnsQuestion.RecordType == RecordType.A || dnsQuestion.RecordType == RecordType.Aaaa))
				{
					result = dnsQuestion.Name?.ToString().TrimEnd(new char[1] { '.' });
				}
			}
		}
		catch (Exception)
		{
		}
		return result;
	}
}
