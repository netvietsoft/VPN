using System;
using System.Security.Cryptography;
using System.Text;

namespace VpnSDK.Private.WFP.Utilities;

internal static class DeterministicGuid
{
	public static readonly Guid DnsNamespace = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

	public static readonly Guid UrlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

	public static readonly Guid IsoOidNamespace = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

	public static Guid Create(Guid namespaceId, string name)
	{
		return Create(namespaceId, name, 5);
	}

	public static Guid Create(Guid namespaceId, string name, int version)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (version != 3 && version != 5)
		{
			throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(name);
		byte[] array = namespaceId.ToByteArray();
		SwapByteOrder(array);
		byte[] hash;
		using (HashAlgorithm hashAlgorithm = ((version == 3) ? ((HashAlgorithm)MD5.Create()) : ((HashAlgorithm)SHA1.Create())))
		{
			hashAlgorithm.TransformBlock(array, 0, array.Length, null, 0);
			hashAlgorithm.TransformFinalBlock(bytes, 0, bytes.Length);
			hash = hashAlgorithm.Hash;
		}
		byte[] array2 = new byte[16];
		Array.Copy(hash, 0, array2, 0, 16);
		array2[6] = (byte)((array2[6] & 0xF) | (version << 4));
		array2[8] = (byte)((array2[8] & 0x3F) | 0x80);
		SwapByteOrder(array2);
		return new Guid(array2);
	}

	internal static void SwapByteOrder(byte[] guid)
	{
		SwapBytes(guid, 0, 3);
		SwapBytes(guid, 1, 2);
		SwapBytes(guid, 4, 5);
		SwapBytes(guid, 6, 7);
	}

	private static void SwapBytes(byte[] guid, int left, int right)
	{
		byte b = guid[left];
		guid[left] = guid[right];
		guid[right] = b;
	}
}
