using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace VpnSDK.Internal.WireGuard;

internal class Driver
{
	internal class Adapter
	{
		private enum IoctlInterfaceFlags : uint
		{
			HasPublicKey = 1u,
			HasPrivateKey = 2u,
			HasListenPort = 4u,
			ReplacePeers = 8u
		}

		private enum IoctlPeerFlags : uint
		{
			HasPublicKey = 1u,
			HasPresharedKey = 2u,
			HasPersistentKeepalive = 4u,
			HasEndpoint = 8u,
			ReplaceAllowedIPs = 0x20u,
			Remove = 0x40u,
			UpdateOnly = 0x80u
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 80)]
		private struct IoctlInterface
		{
			public IoctlInterfaceFlags Flags;

			public ushort ListenPort;

			public unsafe fixed byte PrivateKey[32];

			public unsafe fixed byte PublicKey[32];

			public uint PeersCount;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 136)]
		private struct IoctlPeer
		{
			public IoctlPeerFlags Flags;

			public uint Reserved;

			public unsafe fixed byte PublicKey[32];

			public unsafe fixed byte PresharedKey[32];

			public ushort PersistentKeepalive;

			public NativeMethods.SOCKADDR_INET Endpoint;

			public ulong TxBytes;

			public ulong RxBytes;

			public ulong LastHandshake;

			public uint AllowedIPsCount;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 24)]
		private struct IoctlAllowedIP
		{
			[FieldOffset(0)]
			[MarshalAs(UnmanagedType.Struct)]
			public NativeMethods.IN_ADDR V4;

			[FieldOffset(0)]
			[MarshalAs(UnmanagedType.Struct)]
			public NativeMethods.IN6_ADDR V6;

			[FieldOffset(16)]
			public NativeMethods.ADDRESS_FAMILY AddressFamily;

			[FieldOffset(20)]
			public byte Cidr;
		}

		public class Key
		{
			private byte[] _bytes;

			public byte[] Bytes
			{
				get
				{
					return _bytes;
				}
				set
				{
					if (value == null || value.Length != 32)
					{
						throw new ArgumentException("Keys must be 32 bytes");
					}
					_bytes = value;
				}
			}

			public Key(byte[] bytes)
			{
				Bytes = bytes;
			}

			public unsafe Key(byte* bytes)
			{
				_bytes = new byte[32];
				Marshal.Copy((IntPtr)bytes, _bytes, 0, 32);
			}

			public override string ToString()
			{
				return Convert.ToBase64String(_bytes);
			}
		}

		public class Interface
		{
			public ushort ListenPort { get; set; }

			public Key PrivateKey { get; set; }

			public Key PublicKey { get; set; }

			public Peer[] Peers { get; set; }
		}

		public class Peer
		{
			public Key PublicKey { get; set; }

			public Key PresharedKey { get; set; }

			public ushort PersistentKeepalive { get; set; }

			public IPEndPoint Endpoint { get; set; }

			public ulong TxBytes { get; set; }

			public ulong RxBytes { get; set; }

			public DateTime LastHandshake { get; set; }

			public AllowedIP[] AllowedIPs { get; set; }
		}

		public class AllowedIP
		{
			public IPAddress Address { get; set; }

			public byte Cidr { get; set; }
		}

		private IntPtr _handle;

		private uint _lastGetGuess;

		public Adapter(string name)
		{
			_lastGetGuess = 1024u;
			_handle = NativeMethods.openAdapter(name);
			if (_handle == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
		}

		~Adapter()
		{
			NativeMethods.freeAdapter(_handle);
		}

		public unsafe Interface GetConfiguration()
		{
			Interface obj = new Interface();
			byte[] array;
			while (true)
			{
				array = new byte[_lastGetGuess];
				if (NativeMethods.getConfiguration(_handle, array, ref _lastGetGuess))
				{
					break;
				}
				if (Marshal.GetLastWin32Error() != 234)
				{
					throw new Win32Exception();
				}
			}
			fixed (byte* ptr = array)
			{
				void* ptr2 = ptr;
				IoctlInterface* ptr3 = (IoctlInterface*)ptr2;
				if ((ptr3->Flags & IoctlInterfaceFlags.HasPublicKey) != 0)
				{
					obj.PublicKey = new Key(ptr3->PublicKey);
				}
				if ((ptr3->Flags & IoctlInterfaceFlags.HasPrivateKey) != 0)
				{
					obj.PrivateKey = new Key(ptr3->PrivateKey);
				}
				if ((ptr3->Flags & IoctlInterfaceFlags.HasListenPort) != 0)
				{
					obj.ListenPort = ptr3->ListenPort;
				}
				Peer[] array2 = new Peer[ptr3->PeersCount];
				IoctlPeer* ptr4 = (IoctlPeer*)(ptr3 + 1);
				for (uint num = 0u; num < array2.Length; num++)
				{
					Peer peer = new Peer();
					if ((ptr4->Flags & IoctlPeerFlags.HasPublicKey) != 0)
					{
						peer.PublicKey = new Key(ptr4->PublicKey);
					}
					if ((ptr4->Flags & IoctlPeerFlags.HasPresharedKey) != 0)
					{
						peer.PresharedKey = new Key(ptr4->PresharedKey);
					}
					if ((ptr4->Flags & IoctlPeerFlags.HasPersistentKeepalive) != 0)
					{
						peer.PersistentKeepalive = ptr4->PersistentKeepalive;
					}
					if ((ptr4->Flags & IoctlPeerFlags.HasEndpoint) != 0)
					{
						if (ptr4->Endpoint.si_family == NativeMethods.ADDRESS_FAMILY.AF_INET)
						{
							byte[] array3 = new byte[4];
							Marshal.Copy((IntPtr)ptr4->Endpoint.Ipv4.sin_addr.bytes, array3, 0, 4);
							peer.Endpoint = new IPEndPoint(new IPAddress(array3), (ushort)IPAddress.NetworkToHostOrder((short)ptr4->Endpoint.Ipv4.sin_port));
						}
						else if (ptr4->Endpoint.si_family == NativeMethods.ADDRESS_FAMILY.AF_INET6)
						{
							byte[] array4 = new byte[16];
							Marshal.Copy((IntPtr)ptr4->Endpoint.Ipv6.sin6_addr.bytes, array4, 0, 16);
							peer.Endpoint = new IPEndPoint(new IPAddress(array4), (ushort)IPAddress.NetworkToHostOrder((short)ptr4->Endpoint.Ipv6.sin6_port));
						}
					}
					peer.TxBytes = ptr4->TxBytes;
					peer.RxBytes = ptr4->RxBytes;
					if (ptr4->LastHandshake != 0L)
					{
						peer.LastHandshake = DateTime.FromFileTimeUtc((long)ptr4->LastHandshake);
					}
					AllowedIP[] array5 = new AllowedIP[ptr4->AllowedIPsCount];
					IoctlAllowedIP* ptr5 = (IoctlAllowedIP*)(ptr4 + 1);
					for (uint num2 = 0u; num2 < array5.Length; num2++)
					{
						AllowedIP allowedIP = new AllowedIP();
						if (ptr5->AddressFamily == NativeMethods.ADDRESS_FAMILY.AF_INET)
						{
							byte[] array6 = new byte[4];
							Marshal.Copy((IntPtr)ptr5->V4.bytes, array6, 0, 4);
							allowedIP.Address = new IPAddress(array6);
						}
						else if (ptr5->AddressFamily == NativeMethods.ADDRESS_FAMILY.AF_INET6)
						{
							byte[] array7 = new byte[16];
							Marshal.Copy((IntPtr)ptr5->V6.bytes, array7, 0, 16);
							allowedIP.Address = new IPAddress(array7);
						}
						allowedIP.Cidr = ptr5->Cidr;
						array5[num2] = allowedIP;
						ptr5++;
					}
					peer.AllowedIPs = array5;
					array2[num] = peer;
					ptr4 = (IoctlPeer*)ptr5;
				}
				obj.Peers = array2;
			}
			return obj;
		}
	}
}
