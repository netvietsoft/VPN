using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace VpnSDK.Private.API.Wireguard;

internal static class KeypairGenerator
{
	private static int KEY_BYTE_LENGTH;

	internal static RandomNumberGenerator SecureRandomNumberProvider;

	internal static Random RandomNumberGenerator;

	static KeypairGenerator()
	{
		KEY_BYTE_LENGTH = 32;
		SecureRandomNumberProvider = System.Security.Cryptography.RandomNumberGenerator.Create();
		byte[] array = new byte[4];
		SecureRandomNumberProvider.GetBytes(array);
		RandomNumberGenerator = new Random(BitConverter.ToInt32(array, 0));
		Array.Clear(array, 0, 4);
	}

	public static KeyPair GeneratePair()
	{
		KeyPair result = default(KeyPair);
		byte[] array = new byte[KEY_BYTE_LENGTH];
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		for (int i = 0; i < 10; i++)
		{
			SecureRandomNumberProvider.GetNonZeroBytes(array);
			Timing.Sleep();
			if (RandomNumberGenerator.Next(0, 10) == 10)
			{
				break;
			}
		}
		Curve25519.ClampPrivateKeyInline(array);
		result.Private = Convert.ToBase64String(array);
		result.Public = Convert.ToBase64String(Curve25519.GetPublicKey(array));
		Array.Clear(array, 0, array.Length);
		gCHandle.Free();
		return result;
	}
}
