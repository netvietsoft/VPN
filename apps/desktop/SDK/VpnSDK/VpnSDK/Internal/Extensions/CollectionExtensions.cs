using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace VpnSDK.Internal.Extensions;

internal static class CollectionExtensions
{
	public static void DistinctAdd<T>(this Collection<T> collection, T item)
	{
		if (item != null && !collection.Contains(item))
		{
			collection.Add(item);
		}
	}

	public static void DistinctAddRange<T>(this Collection<T> collection, params T[] items)
	{
		foreach (T item in items)
		{
			collection.DistinctAdd(item);
		}
	}

	public static void RemoveAll<T>(this Collection<T> collection, params T[] items)
	{
		foreach (T item in items?.ToList())
		{
			collection.Remove(item);
		}
	}

	public static void RemoveAllOfType<T>(this Collection<T> collection, Type type)
	{
		collection.RemoveAll(collection.Where((T x) => x.GetType() == type).ToArray());
	}
}
