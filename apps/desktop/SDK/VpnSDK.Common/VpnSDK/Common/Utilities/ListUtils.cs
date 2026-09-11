using System.Collections.Generic;

namespace VpnSDK.Common.Utilities;

internal static class ListUtils
{
	public static bool AreListsEquivalent<T>(List<T>? list1, List<T>? list2)
	{
		if (list1 == null && list2 == null)
		{
			return true;
		}
		if (list1 == null || list2 == null)
		{
			return false;
		}
		if (list1.Count != list2.Count)
		{
			return false;
		}
		HashSet<T> hashSet = new HashSet<T>(list1);
		HashSet<T> hashSet2 = new HashSet<T>(list2);
		return hashSet.SetEquals(hashSet2);
	}
}
