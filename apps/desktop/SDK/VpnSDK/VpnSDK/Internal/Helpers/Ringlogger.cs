using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace VpnSDK.Internal.Helpers;

internal class Ringlogger
{
	private struct UnixTimestamp(long ns)
	{
		private long _ns = ns;

		public static UnixTimestamp Empty => new UnixTimestamp(0L);

		public static UnixTimestamp Now
		{
			get
			{
				DateTimeOffset utcNow = DateTimeOffset.UtcNow;
				long num = utcNow.Subtract(DateTimeOffset.FromUnixTimeSeconds(0L)).Ticks * 100 % 1000000000;
				return new UnixTimestamp(utcNow.ToUnixTimeSeconds() * 1000000000 + num);
			}
		}

		public bool IsEmpty => _ns == 0;

		public long Nanoseconds => _ns;

		public override string ToString()
		{
			return DateTimeOffset.FromUnixTimeSeconds(_ns / 1000000000).LocalDateTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'") + (_ns % 1000000000 + "00000").Substring(0, 6);
		}
	}

	private struct Line
	{
		private const int MaxLineLength = 512;

		private const int OffsetTimeNs = 0;

		private const int OffsetLine = 8;

		private readonly MemoryMappedViewAccessor _view;

		private readonly int _start;

		public static int Bytes => 520;

		public UnixTimestamp Timestamp
		{
			get
			{
				return new UnixTimestamp(_view.ReadInt64(_start));
			}
			set
			{
				_view.Write(_start, value.Nanoseconds);
			}
		}

		public string Text
		{
			get
			{
				byte[] array = new byte[512];
				_view.ReadArray(_start + 8, array, 0, array.Length);
				int num = Array.IndexOf(array, (byte)0);
				if (num <= 0)
				{
					return null;
				}
				return Encoding.UTF8.GetString(array, 0, num);
			}
			set
			{
				if (value == null)
				{
					_view.WriteArray(_start + 8, new byte[512], 0, 512);
					return;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(value);
				int num = Math.Min(511, bytes.Length);
				_view.Write((long)(_start + 8 + num), (byte)0);
				_view.WriteArray(_start + 8, bytes, 0, num);
			}
		}

		public Line(MemoryMappedViewAccessor view, uint index)
		{
			int start = (int)(Log.HeaderBytes + index * Bytes);
			_view = view;
			_start = start;
		}

		public override string ToString()
		{
			UnixTimestamp timestamp = Timestamp;
			if (timestamp.IsEmpty)
			{
				return null;
			}
			string text = Text;
			if (text == null)
			{
				return null;
			}
			return $"{timestamp}: {text}";
		}
	}

	private struct Log(MemoryMappedViewAccessor view)
	{
		private const uint MaxLines = 2048u;

		private const uint MagicUint = 195934910u;

		private const int OffsetMagic = 0;

		private const int OffsetNextIndex = 4;

		private const int OffsetLines = 8;

		private readonly MemoryMappedViewAccessor _view = view;

		public static int HeaderBytes => 8;

		public static int Bytes => (int)(HeaderBytes + (long)Line.Bytes * 2048L);

		public uint LineCount => 2048u;

		public uint ExpectedMagic => 195934910u;

		public uint Magic
		{
			get
			{
				return _view.ReadUInt32(0L);
			}
			set
			{
				_view.Write(0L, value);
			}
		}

		public uint NextIndex
		{
			get
			{
				return _view.ReadUInt32(4L);
			}
			set
			{
				_view.Write(4L, value);
			}
		}

		public Line this[uint i] => new Line(_view, i % 2048);

		public unsafe uint InsertNextIndex()
		{
			byte* pointer = null;
			_view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
			int result = Interlocked.Increment(ref Unsafe.AsRef<int>(pointer + 4));
			_view.SafeMemoryMappedViewHandle.ReleasePointer();
			return (uint)result;
		}

		public void Clear()
		{
			_view.WriteArray(0L, new byte[Bytes], 0, Bytes);
		}
	}

	public static readonly uint CursorAll = uint.MaxValue;

	private readonly Log _log;

	private readonly string _tag;

	private FileStream _fileStream;

	private MemoryMappedFile _mmFile;

	private MemoryMappedViewAccessor _mmView;

	public Ringlogger(string filename, string tag)
	{
		_fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
		_fileStream.SetLength(Log.Bytes);
		_mmFile = MemoryMappedFile.CreateFromFile(_fileStream, null, 0L, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, leaveOpen: false);
		_mmView = _mmFile.CreateViewAccessor(0L, Log.Bytes, MemoryMappedFileAccess.ReadWrite);
		_log = new Log(_mmView);
		if (_log.Magic != _log.ExpectedMagic)
		{
			_log.Clear();
			_log.Magic = _log.ExpectedMagic;
		}
		_tag = tag;
	}

	public void Dispose()
	{
		try
		{
			_mmView.Dispose();
			_mmFile.Dispose();
			_fileStream.Dispose();
		}
		catch
		{
		}
	}

	public void Write(string line)
	{
		UnixTimestamp now = UnixTimestamp.Now;
		Line line2 = _log[_log.InsertNextIndex() - 1];
		line2.Timestamp = UnixTimestamp.Empty;
		line2.Text = null;
		line2.Text = $"[{_tag}] {line.Trim()}";
		line2.Timestamp = now;
	}

	public void WriteTo(TextWriter writer)
	{
		uint nextIndex = _log.NextIndex;
		for (uint num = 0u; num < _log.LineCount; num++)
		{
			Line line = _log[num + nextIndex];
			if (!line.Timestamp.IsEmpty)
			{
				string text = line.ToString();
				if (text != null)
				{
					writer.WriteLine(text);
				}
			}
		}
	}

	public List<string> FollowFromCursor(ref uint cursor)
	{
		List<string> list = new List<string>((int)_log.LineCount);
		uint num = cursor;
		bool flag = cursor == CursorAll;
		if (flag)
		{
			num = _log.NextIndex;
		}
		uint num2 = 0u;
		while (num2 < _log.LineCount && (flag || num % _log.LineCount != _log.NextIndex % _log.LineCount))
		{
			Line line = _log[num];
			if (line.Timestamp.IsEmpty)
			{
				if (!flag)
				{
					break;
				}
			}
			else
			{
				cursor = (num + 1) % _log.LineCount;
				string text = line.ToString();
				if (text != null)
				{
					list.Add(text);
				}
			}
			num2++;
			num++;
		}
		return list;
	}
}
