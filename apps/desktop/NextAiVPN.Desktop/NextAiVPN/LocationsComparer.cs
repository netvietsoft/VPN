using System;
using System.Collections.Generic;
using DynamicData.Binding;
using VpnSDK.Interfaces;

namespace NextAiVPN;

public class LocationsComparer : List<SortExpression<ILocation>>, IComparer<ILocation>
{
	public static LocationsComparer Ascending(Func<ILocation, IComparable> expression)
	{
		return new LocationsComparer
		{
			new SortExpression<ILocation>(expression)
		};
	}

	public static LocationsComparer Descending(Func<ILocation, IComparable> expression)
	{
		return new LocationsComparer
		{
			new SortExpression<ILocation>(expression, SortDirection.Descending)
		};
	}

	public int Compare(ILocation x, ILocation y)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SortExpression<ILocation> current = enumerator.Current;
				if (x == null && y == null)
				{
					continue;
				}
				if (x == null)
				{
					return -1;
				}
				if (y == null)
				{
					return 1;
				}
				IComparable comparable = current.Expression(x);
				IComparable comparable2 = current.Expression(y);
				if (comparable != null || comparable2 != null)
				{
					if (comparable == null)
					{
						return -1;
					}
					if (comparable2 == null)
					{
						return 1;
					}
					int num = comparable.CompareTo(comparable2);
					if (num != 0)
					{
						return (current.Direction == SortDirection.Ascending) ? num : (-num);
					}
				}
			}
		}
		return 0;
	}

	public LocationsComparer ThenByAscending(Func<ILocation, IComparable> expression)
	{
		Add(new SortExpression<ILocation>(expression));
		return this;
	}

	public LocationsComparer ThenByDescending(Func<ILocation, IComparable> expression)
	{
		Add(new SortExpression<ILocation>(expression, SortDirection.Descending));
		return this;
	}
}
