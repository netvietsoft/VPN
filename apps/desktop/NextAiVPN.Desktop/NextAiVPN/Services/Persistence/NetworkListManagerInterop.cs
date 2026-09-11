using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NextAiVPN.Services.Persistence;

internal static class NetworkListManagerInterop
{
	private static readonly Guid NetworkListManagerClsid = new Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B");

	private const int ConnectedNetworksFilter = 1;

	public static IReadOnlyList<string> GetConnectedNetworkNames()
	{
		List<string> list = new List<string>();
		dynamic val = Activator.CreateInstance(Type.GetTypeFromCLSID(NetworkListManagerClsid));
		try
		{
			dynamic val2 = null;
			try
			{
				val2 = val.GetNetworks(1);
				IEnumerator enumerator = ((IEnumerable)val2).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						dynamic current = enumerator.Current;
						try
						{
							list.Add((string)current.GetName());
						}
						finally
						{
							FinalReleaseComObject((object)current);
						}
					}
					return list;
				}
				finally
				{
					FinalReleaseComObject(enumerator);
				}
			}
			finally
			{
				NetworkListManagerInterop.FinalReleaseComObject(val2);
			}
		}
		finally
		{
			NetworkListManagerInterop.FinalReleaseComObject(val);
		}
	}

	private static void FinalReleaseComObject(object comObject)
	{
		if (comObject != null && Marshal.IsComObject(comObject))
		{
			Marshal.FinalReleaseComObject(comObject);
		}
	}
}
