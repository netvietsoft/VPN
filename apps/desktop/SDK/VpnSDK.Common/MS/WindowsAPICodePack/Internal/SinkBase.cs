using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MS.WindowsAPICodePack.Internal;

internal abstract class SinkBase
{
	private static object _threadLock = new object();

	private static ArrayList _eventSinkHelpers = new ArrayList();

	private IConnectionPoint _connectionPoint;

	private int _cookie;

	protected Delegate ForwardingDelegate { get; set; }

	public static void AddSink<TSinkType>(IConnectionPointContainer container, Type eventInterface, Delegate @delegate) where TSinkType : SinkBase, new()
	{
		lock (_threadLock)
		{
			TSinkType val = new TSinkType();
			Guid riid = eventInterface.GUID;
			container.FindConnectionPoint(ref riid, out val._connectionPoint);
			val._connectionPoint.Advise(val, out var pdwCookie);
			val._cookie = pdwCookie;
			val.ForwardingDelegate = @delegate;
			_eventSinkHelpers.Add(val);
		}
	}

	public static void RemoveSink<TSinkType>(Delegate @delegate) where TSinkType : SinkBase
	{
		lock (_threadLock)
		{
			if (_eventSinkHelpers == null)
			{
				return;
			}
			int count = _eventSinkHelpers.Count;
			int num = 0;
			if (count <= 0)
			{
				return;
			}
			do
			{
				if (_eventSinkHelpers[num] is TSinkType val && (object)val.ForwardingDelegate != null && val.ForwardingDelegate.Equals(@delegate))
				{
					_eventSinkHelpers.RemoveAt(num);
					val.UnAdvise();
					break;
				}
				num++;
			}
			while (num < count);
		}
	}

	private void UnAdvise()
	{
		_connectionPoint.Unadvise(_cookie);
		Marshal.ReleaseComObject(_connectionPoint);
		_connectionPoint = null;
		_cookie = 0;
		ForwardingDelegate = null;
	}
}
