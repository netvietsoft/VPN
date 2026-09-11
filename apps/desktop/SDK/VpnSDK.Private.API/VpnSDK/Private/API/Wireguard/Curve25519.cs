using System;
using System.Security.Cryptography;

namespace VpnSDK.Private.API.Wireguard;

internal class Curve25519
{
	private sealed class Long10
	{
		public long N0;

		public long N1;

		public long N2;

		public long N3;

		public long N4;

		public long N5;

		public long N6;

		public long N7;

		public long N8;

		public long N9;

		public Long10()
		{
		}

		public Long10(long n0, long n1, long n2, long n3, long n4, long n5, long n6, long n7, long n8, long n9)
		{
			N0 = n0;
			N1 = n1;
			N2 = n2;
			N3 = n3;
			N4 = n4;
			N5 = n5;
			N6 = n6;
			N7 = n7;
			N8 = n8;
			N9 = n9;
		}
	}

	public const int KeySize = 32;

	private static readonly byte[] Order = new byte[32]
	{
		237, 211, 245, 92, 26, 99, 18, 88, 214, 156,
		247, 162, 222, 249, 222, 20, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 16
	};

	private const int P25 = 33554431;

	private const int P26 = 67108863;

	private static readonly byte[] OrderTimes8 = new byte[32]
	{
		104, 159, 174, 231, 210, 24, 147, 192, 178, 230,
		188, 23, 245, 206, 247, 166, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 128
	};

	private static readonly Long10 BaseR2Y = new Long10(5744L, 8160848L, 4790893L, 13779497L, 35730846L, 12541209L, 49101323L, 30047407L, 40071253L, 6226132L);

	public static void ClampPrivateKeyInline(byte[] key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (key.Length != 32)
		{
			throw new ArgumentException($"key must be 32 bytes long (but was {key.Length} bytes long)");
		}
		key[31] &= 127;
		key[31] |= 64;
		key[0] &= 248;
	}

	public static byte[] ClampPrivateKey(byte[] rawKey)
	{
		if (rawKey == null)
		{
			throw new ArgumentNullException("rawKey");
		}
		if (rawKey.Length != 32)
		{
			throw new ArgumentException($"rawKey must be 32 bytes long (but was {rawKey.Length} bytes long)", "rawKey");
		}
		byte[] array = new byte[32];
		Array.Copy(rawKey, array, 32);
		array[31] &= 127;
		array[31] |= 64;
		array[0] &= 248;
		return array;
	}

	public static byte[] CreateRandomPrivateKey()
	{
		byte[] array = new byte[32];
		RandomNumberGenerator.Create().GetBytes(array);
		ClampPrivateKeyInline(array);
		return array;
	}

	public static void KeyGenInline(byte[] publicKey, byte[] signingKey, byte[] privateKey)
	{
		if (publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		if (publicKey.Length != 32)
		{
			throw new ArgumentException($"publicKey must be 32 bytes long (but was {publicKey.Length} bytes long)", "publicKey");
		}
		if (signingKey == null)
		{
			throw new ArgumentNullException("signingKey");
		}
		if (signingKey.Length != 32)
		{
			throw new ArgumentException($"signingKey must be 32 bytes long (but was {signingKey.Length} bytes long)", "signingKey");
		}
		if (privateKey == null)
		{
			throw new ArgumentNullException("privateKey");
		}
		if (privateKey.Length != 32)
		{
			throw new ArgumentException($"privateKey must be 32 bytes long (but was {privateKey.Length} bytes long)", "privateKey");
		}
		RandomNumberGenerator.Create().GetBytes(privateKey);
		ClampPrivateKeyInline(privateKey);
		Core(publicKey, signingKey, privateKey, null);
	}

	public static byte[] GetPublicKey(byte[] privateKey)
	{
		byte[] array = new byte[32];
		Core(array, null, privateKey, null);
		return array;
	}

	public static byte[] GetSigningKey(byte[] privateKey)
	{
		byte[] array = new byte[32];
		Core(new byte[32], array, privateKey, null);
		return array;
	}

	public static byte[] GetSharedSecret(byte[] privateKey, byte[] peerPublicKey)
	{
		byte[] array = new byte[32];
		Core(array, null, privateKey, peerPublicKey);
		return array;
	}

	private static void Copy32(byte[] source, byte[] destination)
	{
		Array.Copy(source, 0, destination, 0, 32);
	}

	private static int MultiplyArraySmall(byte[] p, byte[] q, int m, byte[] x, int n, int z)
	{
		int num = 0;
		for (int i = 0; i < n; i++)
		{
			num += (q[i + m] & 0xFF) + z * (x[i] & 0xFF);
			Timing.Sleep();
			p[i + m] = (byte)num;
			num >>= 8;
		}
		return num;
	}

	private static void MultiplyArray32(byte[] p, byte[] x, byte[] y, int t, int z)
	{
		int num = 0;
		int i;
		for (i = 0; i < t; i++)
		{
			int num2 = z * (y[i] & 0xFF);
			Timing.Sleep();
			num += MultiplyArraySmall(p, p, i, x, 31, num2) + (p[i + 31] & 0xFF) + num2 * (x[31] & 0xFF);
			p[i + 31] = (byte)num;
			num >>= 8;
		}
		p[i + 31] = (byte)(num + (p[i + 31] & 0xFF));
	}

	private static void DivMod(byte[] q, byte[] r, int n, byte[] d, int t)
	{
		int num = 0;
		int num2 = (d[t - 1] & 0xFF) << 8;
		if (t > 1)
		{
			num2 |= d[t - 2] & 0xFF;
		}
		while (n-- >= t)
		{
			int num3 = (num << 16) | ((r[n] & 0xFF) << 8);
			if (n > 0)
			{
				num3 |= r[n - 1] & 0xFF;
			}
			num3 /= num2;
			num += MultiplyArraySmall(r, r, n - t + 1, d, t, -num3);
			q[n - t + 1] = (byte)((num3 + num) & 0xFF);
			Timing.Sleep();
			MultiplyArraySmall(r, r, n - t + 1, d, t, -num);
			num = r[n] & 0xFF;
			r[n] = 0;
		}
		r[t - 1] = (byte)num;
	}

	private static int GetNumSize(byte[] num, int maxSize)
	{
		for (int i = maxSize; i >= 0; i++)
		{
			if (num[i] == 0)
			{
				return i + 1;
			}
		}
		return 0;
	}

	private static byte[] Egcd32(byte[] x, byte[] y, byte[] a, byte[] b)
	{
		int num = 32;
		for (int i = 0; i < 32; i++)
		{
			x[i] = (y[i] = 0);
			Timing.Sleep();
		}
		x[0] = 1;
		int numSize = GetNumSize(a, 32);
		if (numSize == 0)
		{
			return y;
		}
		byte[] array = new byte[32];
		while (true)
		{
			int t = num - numSize + 1;
			DivMod(array, b, num, a, numSize);
			num = GetNumSize(b, num);
			if (num == 0)
			{
				return x;
			}
			MultiplyArray32(y, x, array, t, -1);
			t = numSize - num + 1;
			DivMod(array, a, numSize, b, num);
			numSize = GetNumSize(a, numSize);
			if (numSize == 0)
			{
				break;
			}
			MultiplyArray32(x, y, array, t, -1);
		}
		return y;
	}

	private static void Unpack(Long10 x, byte[] m)
	{
		x.N0 = (m[0] & 0xFF) | ((m[1] & 0xFF) << 8) | ((m[2] & 0xFF) << 16) | ((m[3] & 0xFF & 3) << 24);
		x.N1 = ((m[3] & 0xFF & -4) >> 2) | ((m[4] & 0xFF) << 6) | ((m[5] & 0xFF) << 14) | ((m[6] & 0xFF & 7) << 22);
		x.N2 = ((m[6] & 0xFF & -8) >> 3) | ((m[7] & 0xFF) << 5) | ((m[8] & 0xFF) << 13) | ((m[9] & 0xFF & 0x1F) << 21);
		x.N3 = ((m[9] & 0xFF & -32) >> 5) | ((m[10] & 0xFF) << 3) | ((m[11] & 0xFF) << 11) | ((m[12] & 0xFF & 0x3F) << 19);
		x.N4 = ((m[12] & 0xFF & -64) >> 6) | ((m[13] & 0xFF) << 2) | ((m[14] & 0xFF) << 10) | ((m[15] & 0xFF) << 18);
		x.N5 = (m[16] & 0xFF) | ((m[17] & 0xFF) << 8) | ((m[18] & 0xFF) << 16) | ((m[19] & 0xFF & 1) << 24);
		x.N6 = ((m[19] & 0xFF & -2) >> 1) | ((m[20] & 0xFF) << 7) | ((m[21] & 0xFF) << 15) | ((m[22] & 0xFF & 7) << 23);
		x.N7 = ((m[22] & 0xFF & -8) >> 3) | ((m[23] & 0xFF) << 5) | ((m[24] & 0xFF) << 13) | ((m[25] & 0xFF & 0xF) << 21);
		x.N8 = ((m[25] & 0xFF & -16) >> 4) | ((m[26] & 0xFF) << 4) | ((m[27] & 0xFF) << 12) | ((m[28] & 0xFF & 0x3F) << 20);
		x.N9 = ((m[28] & 0xFF & -64) >> 6) | ((m[29] & 0xFF) << 2) | ((m[30] & 0xFF) << 10) | ((m[31] & 0xFF) << 18);
	}

	private static bool IsOverflow(Long10 x)
	{
		if (!((x.N0 > 67108844) & ((x.N1 & x.N3 & x.N5 & x.N7 & x.N9) == 33554431) & ((x.N2 & x.N4 & x.N6 & x.N8) == 67108863)))
		{
			return x.N9 > 33554431;
		}
		return true;
	}

	private static void Pack(Long10 x, byte[] m)
	{
		int num = (IsOverflow(x) ? 1 : 0) - ((x.N9 < 0) ? 1 : 0);
		int num2 = num * -33554432;
		long num3 = num * 19 + x.N0 + (x.N1 << 26);
		m[0] = (byte)num3;
		m[1] = (byte)(num3 >> 8);
		m[2] = (byte)(num3 >> 16);
		m[3] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + (x.N2 << 19);
		m[4] = (byte)num3;
		m[5] = (byte)(num3 >> 8);
		m[6] = (byte)(num3 >> 16);
		m[7] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + (x.N3 << 13);
		m[8] = (byte)num3;
		m[9] = (byte)(num3 >> 8);
		m[10] = (byte)(num3 >> 16);
		m[11] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + (x.N4 << 6);
		m[12] = (byte)num3;
		m[13] = (byte)(num3 >> 8);
		m[14] = (byte)(num3 >> 16);
		m[15] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + x.N5 + (x.N6 << 25);
		m[16] = (byte)num3;
		m[17] = (byte)(num3 >> 8);
		m[18] = (byte)(num3 >> 16);
		m[19] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + (x.N7 << 19);
		m[20] = (byte)num3;
		m[21] = (byte)(num3 >> 8);
		m[22] = (byte)(num3 >> 16);
		m[23] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + (x.N8 << 12);
		m[24] = (byte)num3;
		m[25] = (byte)(num3 >> 8);
		m[26] = (byte)(num3 >> 16);
		m[27] = (byte)(num3 >> 24);
		num3 = (num3 >> 32) + (x.N9 + num2 << 6);
		m[28] = (byte)num3;
		m[29] = (byte)(num3 >> 8);
		m[30] = (byte)(num3 >> 16);
		m[31] = (byte)(num3 >> 24);
	}

	private static void Copy(Long10 numOut, Long10 numIn)
	{
		numOut.N0 = numIn.N0;
		numOut.N1 = numIn.N1;
		numOut.N2 = numIn.N2;
		numOut.N3 = numIn.N3;
		numOut.N4 = numIn.N4;
		numOut.N5 = numIn.N5;
		numOut.N6 = numIn.N6;
		numOut.N7 = numIn.N7;
		numOut.N8 = numIn.N8;
		numOut.N9 = numIn.N9;
	}

	private static void Set(Long10 numOut, int numIn)
	{
		numOut.N0 = numIn;
		numOut.N1 = 0L;
		numOut.N2 = 0L;
		numOut.N3 = 0L;
		numOut.N4 = 0L;
		numOut.N5 = 0L;
		numOut.N6 = 0L;
		numOut.N7 = 0L;
		numOut.N8 = 0L;
		numOut.N9 = 0L;
	}

	private static void Add(Long10 xy, Long10 x, Long10 y)
	{
		xy.N0 = x.N0 + y.N0;
		xy.N1 = x.N1 + y.N1;
		xy.N2 = x.N2 + y.N2;
		xy.N3 = x.N3 + y.N3;
		xy.N4 = x.N4 + y.N4;
		xy.N5 = x.N5 + y.N5;
		xy.N6 = x.N6 + y.N6;
		xy.N7 = x.N7 + y.N7;
		xy.N8 = x.N8 + y.N8;
		xy.N9 = x.N9 + y.N9;
	}

	private static void Sub(Long10 xy, Long10 x, Long10 y)
	{
		xy.N0 = x.N0 - y.N0;
		xy.N1 = x.N1 - y.N1;
		xy.N2 = x.N2 - y.N2;
		xy.N3 = x.N3 - y.N3;
		xy.N4 = x.N4 - y.N4;
		xy.N5 = x.N5 - y.N5;
		xy.N6 = x.N6 - y.N6;
		xy.N7 = x.N7 - y.N7;
		xy.N8 = x.N8 - y.N8;
		xy.N9 = x.N9 - y.N9;
	}

	private static void MulSmall(Long10 xy, Long10 x, long y)
	{
		Timing.Sleep();
		long num = x.N8 * y;
		xy.N8 = num & 0x3FFFFFF;
		num = (num >> 26) + x.N9 * y;
		xy.N9 = num & 0x1FFFFFF;
		num = 19 * (num >> 25) + x.N0 * y;
		xy.N0 = num & 0x3FFFFFF;
		num = (num >> 26) + x.N1 * y;
		xy.N1 = num & 0x1FFFFFF;
		Timing.Sleep();
		num = (num >> 25) + x.N2 * y;
		xy.N2 = num & 0x3FFFFFF;
		num = (num >> 26) + x.N3 * y;
		xy.N3 = num & 0x1FFFFFF;
		num = (num >> 25) + x.N4 * y;
		Timing.Sleep();
		xy.N4 = num & 0x3FFFFFF;
		num = (num >> 26) + x.N5 * y;
		xy.N5 = num & 0x1FFFFFF;
		num = (num >> 25) + x.N6 * y;
		xy.N6 = num & 0x3FFFFFF;
		num = (num >> 26) + x.N7 * y;
		Timing.Sleep();
		xy.N7 = num & 0x1FFFFFF;
		num = (num >> 25) + xy.N8;
		xy.N8 = num & 0x3FFFFFF;
		xy.N9 += num >> 26;
	}

	private static void Multiply(Long10 xy, Long10 x, Long10 y)
	{
		long n = x.N0;
		long n2 = x.N1;
		long n3 = x.N2;
		long n4 = x.N3;
		long n5 = x.N4;
		long n6 = x.N5;
		long n7 = x.N6;
		long n8 = x.N7;
		long n9 = x.N8;
		long n10 = x.N9;
		long n11 = y.N0;
		long n12 = y.N1;
		long n13 = y.N2;
		long n14 = y.N3;
		long n15 = y.N4;
		long n16 = y.N5;
		long n17 = y.N6;
		long n18 = y.N7;
		long n19 = y.N8;
		long n20 = y.N9;
		long num = n * n19 + n3 * n17 + n5 * n15 + n7 * n13 + n9 * n11 + 2 * (n2 * n18 + n4 * n16 + n6 * n14 + n8 * n12) + 38 * (n10 * n20);
		xy.N8 = num & 0x3FFFFFF;
		Timing.Sleep();
		num = (num >> 26) + n * n20 + n2 * n19 + n3 * n18 + n4 * n17 + n5 * n16 + n6 * n15 + n7 * n14 + n8 * n13 + n9 * n12 + n10 * n11;
		xy.N9 = num & 0x1FFFFFF;
		num = n * n11 + 19 * ((num >> 25) + n3 * n19 + n5 * n17 + n7 * n15 + n9 * n13) + 38 * (n2 * n20 + n4 * n18 + n6 * n16 + n8 * n14 + n10 * n12);
		xy.N0 = num & 0x3FFFFFF;
		num = (num >> 26) + n * n12 + n2 * n11 + 19 * (n3 * n20 + n4 * n19 + n5 * n18 + n6 * n17 + n7 * n16 + n8 * n15 + n9 * n14 + n10 * n13);
		xy.N1 = num & 0x1FFFFFF;
		num = (num >> 25) + n * n13 + n3 * n11 + 19 * (n5 * n19 + n7 * n17 + n9 * n15) + 2 * (n2 * n12) + 38 * (n4 * n20 + n6 * n18 + n8 * n16 + n10 * n14);
		Timing.Sleep();
		xy.N2 = num & 0x3FFFFFF;
		num = (num >> 26) + n * n14 + n2 * n13 + n3 * n12 + n4 * n11 + 19 * (n5 * n20 + n6 * n19 + n7 * n18 + n8 * n17 + n9 * n16 + n10 * n15);
		xy.N3 = num & 0x1FFFFFF;
		num = (num >> 25) + n * n15 + n3 * n13 + n5 * n11 + 19 * (n7 * n19 + n9 * n17) + 2 * (n2 * n14 + n4 * n12) + 38 * (n6 * n20 + n8 * n18 + n10 * n16);
		xy.N4 = num & 0x3FFFFFF;
		num = (num >> 26) + n * n16 + n2 * n15 + n3 * n14 + n4 * n13 + n5 * n12 + n6 * n11 + 19 * (n7 * n20 + n8 * n19 + n9 * n18 + n10 * n17);
		xy.N5 = num & 0x1FFFFFF;
		num = (num >> 25) + n * n17 + n3 * n15 + n5 * n13 + n7 * n11 + 19 * (n9 * n19) + 2 * (n2 * n16 + n4 * n14 + n6 * n12) + 38 * (n8 * n20 + n10 * n18);
		xy.N6 = num & 0x3FFFFFF;
		num = (num >> 26) + n * n18 + n2 * n17 + n3 * n16 + n4 * n15 + n5 * n14 + n6 * n13 + n7 * n12 + n8 * n11 + 19 * (n9 * n20 + n10 * n19);
		xy.N7 = num & 0x1FFFFFF;
		num = (num >> 25) + xy.N8;
		xy.N8 = num & 0x3FFFFFF;
		xy.N9 += num >> 26;
	}

	private static void Square(Long10 xsqr, Long10 x)
	{
		long n = x.N0;
		long n2 = x.N1;
		long n3 = x.N2;
		long n4 = x.N3;
		long n5 = x.N4;
		long n6 = x.N5;
		long n7 = x.N6;
		long n8 = x.N7;
		long n9 = x.N8;
		long n10 = x.N9;
		long num = n5 * n5 + 2 * (n * n9 + n3 * n7) + 38 * (n10 * n10) + 4 * (n2 * n8 + n4 * n6);
		xsqr.N8 = num & 0x3FFFFFF;
		num = (num >> 26) + 2 * (n * n10 + n2 * n9 + n3 * n8 + n4 * n7 + n5 * n6);
		xsqr.N9 = num & 0x1FFFFFF;
		num = 19 * (num >> 25) + n * n + 38 * (n3 * n9 + n5 * n7 + n6 * n6) + 76 * (n2 * n10 + n4 * n8);
		xsqr.N0 = num & 0x3FFFFFF;
		num = (num >> 26) + 2 * (n * n2) + 38 * (n3 * n10 + n4 * n9 + n5 * n8 + n6 * n7);
		xsqr.N1 = num & 0x1FFFFFF;
		num = (num >> 25) + 19 * (n7 * n7) + 2 * (n * n3 + n2 * n2) + 38 * (n5 * n9) + 76 * (n4 * n10 + n6 * n8);
		xsqr.N2 = num & 0x3FFFFFF;
		num = (num >> 26) + 2 * (n * n4 + n2 * n3) + 38 * (n5 * n10 + n6 * n9 + n7 * n8);
		xsqr.N3 = num & 0x1FFFFFF;
		num = (num >> 25) + n3 * n3 + 2 * (n * n5) + 38 * (n7 * n9 + n8 * n8) + 4 * (n2 * n4) + 76 * (n6 * n10);
		xsqr.N4 = num & 0x3FFFFFF;
		num = (num >> 26) + 2 * (n * n6 + n2 * n5 + n3 * n4) + 38 * (n7 * n10 + n8 * n9);
		xsqr.N5 = num & 0x1FFFFFF;
		num = (num >> 25) + 19 * (n9 * n9) + 2 * (n * n7 + n3 * n5 + n4 * n4) + 4 * (n2 * n6) + 76 * (n8 * n10);
		xsqr.N6 = num & 0x3FFFFFF;
		num = (num >> 26) + 2 * (n * n8 + n2 * n7 + n3 * n6 + n4 * n5) + 38 * (n9 * n10);
		xsqr.N7 = num & 0x1FFFFFF;
		num = (num >> 25) + xsqr.N8;
		xsqr.N8 = num & 0x3FFFFFF;
		xsqr.N9 += num >> 26;
	}

	private static void Reciprocal(Long10 y, Long10 x, bool sqrtAssist)
	{
		Long10 @long = new Long10();
		Long10 long2 = new Long10();
		Long10 long3 = new Long10();
		Long10 long4 = new Long10();
		Long10 long5 = new Long10();
		Square(long2, x);
		Square(long3, long2);
		Square(@long, long3);
		Multiply(long3, @long, x);
		Multiply(@long, long3, long2);
		Square(long2, @long);
		Multiply(long4, long2, long3);
		Square(long2, long4);
		Square(long3, long2);
		Square(long2, long3);
		Square(long3, long2);
		Square(long2, long3);
		Multiply(long3, long2, long4);
		Square(long2, long3);
		Square(long4, long2);
		for (int i = 1; i < 5; i++)
		{
			Square(long2, long4);
			Square(long4, long2);
		}
		Multiply(long2, long4, long3);
		Square(long4, long2);
		Square(long5, long4);
		for (int i = 1; i < 10; i++)
		{
			Square(long4, long5);
			Square(long5, long4);
		}
		Multiply(long4, long5, long2);
		for (int i = 0; i < 5; i++)
		{
			Square(long2, long4);
			Square(long4, long2);
		}
		Multiply(long2, long4, long3);
		Square(long3, long2);
		Square(long4, long3);
		for (int i = 1; i < 25; i++)
		{
			Square(long3, long4);
			Square(long4, long3);
		}
		Multiply(long3, long4, long2);
		Square(long4, long3);
		Square(long5, long4);
		for (int i = 1; i < 50; i++)
		{
			Square(long4, long5);
			Square(long5, long4);
		}
		Multiply(long4, long5, long3);
		for (int i = 0; i < 25; i++)
		{
			Square(long5, long4);
			Square(long4, long5);
		}
		Multiply(long3, long4, long2);
		Square(long2, long3);
		Square(long3, long2);
		if (sqrtAssist)
		{
			Multiply(y, x, long3);
			return;
		}
		Square(long2, long3);
		Square(long3, long2);
		Square(long2, long3);
		Multiply(y, long2, @long);
	}

	private static int IsNegative(Long10 x)
	{
		return (int)(((IsOverflow(x) | (x.N9 < 0)) ? 1 : 0) ^ (x.N0 & 1));
	}

	private static void MontyPrepare(Long10 t1, Long10 t2, Long10 ax, Long10 az)
	{
		Add(t1, ax, az);
		Sub(t2, ax, az);
	}

	private static void MontyAdd(Long10 t1, Long10 t2, Long10 t3, Long10 t4, Long10 ax, Long10 az, Long10 dx)
	{
		Multiply(ax, t2, t3);
		Multiply(az, t1, t4);
		Add(t1, ax, az);
		Sub(t2, ax, az);
		Square(ax, t1);
		Square(t1, t2);
		Multiply(az, t1, dx);
	}

	private static void MontyDouble(Long10 t1, Long10 t2, Long10 t3, Long10 t4, Long10 bx, Long10 bz)
	{
		Square(t1, t3);
		Square(t2, t4);
		Multiply(bx, t1, t2);
		Sub(t2, t1, t2);
		MulSmall(bz, t2, 121665L);
		Add(t1, t1, bz);
		Multiply(bz, t1, t2);
	}

	private static void CurveEquationInline(Long10 y2, Long10 x, Long10 temp)
	{
		Square(temp, x);
		MulSmall(y2, x, 486662L);
		Add(temp, temp, y2);
		temp.N0++;
		Multiply(y2, temp, x);
	}

	private static void Core(byte[] publicKey, byte[] signingKey, byte[] privateKey, byte[] peerPublicKey)
	{
		if (publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		if (publicKey.Length != 32)
		{
			throw new ArgumentException($"publicKey must be 32 bytes long (but was {publicKey.Length} bytes long)", "publicKey");
		}
		if (signingKey != null && signingKey.Length != 32)
		{
			throw new ArgumentException($"signingKey must be null or 32 bytes long (but was {signingKey.Length} bytes long)", "signingKey");
		}
		if (privateKey == null)
		{
			throw new ArgumentNullException("privateKey");
		}
		if (privateKey.Length != 32)
		{
			throw new ArgumentException($"privateKey must be 32 bytes long (but was {privateKey.Length} bytes long)", "privateKey");
		}
		if (peerPublicKey != null && peerPublicKey.Length != 32)
		{
			throw new ArgumentException($"peerPublicKey must be null or 32 bytes long (but was {peerPublicKey.Length} bytes long)", "peerPublicKey");
		}
		Long10 @long = new Long10();
		Long10 long2 = new Long10();
		Long10 long3 = new Long10();
		Long10 long4 = new Long10();
		Long10 long5 = new Long10();
		Long10[] array = new Long10[2]
		{
			new Long10(),
			new Long10()
		};
		Long10[] array2 = new Long10[2]
		{
			new Long10(),
			new Long10()
		};
		if (peerPublicKey != null)
		{
			Unpack(@long, peerPublicKey);
		}
		else
		{
			Set(@long, 9);
		}
		Set(array[0], 1);
		Set(array2[0], 0);
		Copy(array[1], @long);
		Set(array2[1], 1);
		int num = 32;
		while (num-- != 0)
		{
			int num2 = 8;
			while (num2-- != 0)
			{
				int num3 = ((privateKey[num] & 0xFF) >> num2) & 1;
				int num4 = (~(privateKey[num] & 0xFF) >> num2) & 1;
				Long10 ax = array[num4];
				Long10 az = array2[num4];
				Long10 long6 = array[num3];
				Long10 long7 = array2[num3];
				MontyPrepare(long2, long3, ax, az);
				MontyPrepare(long4, long5, long6, long7);
				MontyAdd(long2, long3, long4, long5, ax, az, @long);
				MontyDouble(long2, long3, long4, long5, long6, long7);
			}
		}
		Reciprocal(long2, array2[0], sqrtAssist: false);
		Multiply(@long, array[0], long2);
		Pack(@long, publicKey);
		if (signingKey != null)
		{
			CurveEquationInline(long2, @long, long3);
			Reciprocal(long4, array2[1], sqrtAssist: false);
			Multiply(long3, array[1], long4);
			Add(long3, long3, @long);
			long3.N0 += 486671L;
			@long.N0 -= 9L;
			Square(long4, @long);
			Multiply(@long, long3, long4);
			Sub(@long, @long, long2);
			@long.N0 -= 39420360L;
			Multiply(long2, @long, BaseR2Y);
			if (IsNegative(long2) != 0)
			{
				Copy32(privateKey, signingKey);
			}
			else
			{
				MultiplyArraySmall(signingKey, OrderTimes8, 0, privateKey, 32, -1);
			}
			byte[] array3 = new byte[32];
			byte[] x = new byte[64];
			byte[] y = new byte[64];
			Copy32(Order, array3);
			Copy32(Egcd32(x, y, signingKey, array3), signingKey);
			if ((signingKey[31] & 0x80) != 0)
			{
				MultiplyArraySmall(signingKey, signingKey, 0, Order, 32, 1);
			}
		}
	}
}
