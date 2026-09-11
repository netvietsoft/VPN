using System;

namespace VpnSDK;

public class DataTransferEventArgs : EventArgs
{
	public long DownloadedBytes { get; internal set; }

	public long UploadedBytes { get; internal set; }

	public DataTransferEventArgs(long downloadedBytes, long uploadedBytes)
	{
		DownloadedBytes = downloadedBytes;
		UploadedBytes = uploadedBytes;
	}

	public static bool operator ==(DataTransferEventArgs argsLeft, DataTransferEventArgs argsRight)
	{
		if (argsLeft?.DownloadedBytes == argsRight?.DownloadedBytes)
		{
			return argsLeft?.UploadedBytes == argsRight?.UploadedBytes;
		}
		return false;
	}

	public static bool operator !=(DataTransferEventArgs argsLeft, DataTransferEventArgs argsRight)
	{
		if (argsLeft?.DownloadedBytes == argsRight?.DownloadedBytes)
		{
			return argsLeft?.UploadedBytes != argsRight?.UploadedBytes;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is DataTransferEventArgs e && e != null)
		{
			return e == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
