using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace NextAiVPN.Services.Persistence;

internal static class ControlFinder
{
	public static FrameworkElement FindChild(DependencyObject parent, string childName)
	{
		if (parent == null || string.IsNullOrEmpty(childName))
		{
			return null;
		}
		if (parent is FrameworkElement frameworkElement && frameworkElement.Name == childName)
		{
			return frameworkElement;
		}
		FrameworkElement frameworkElement2 = null;
		if (parent is FrameworkElement frameworkElement3)
		{
			frameworkElement3.ApplyTemplate();
		}
		int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childrenCount; i++)
		{
			frameworkElement2 = FindChild(VisualTreeHelper.GetChild(parent, i), childName);
			if (frameworkElement2 != null)
			{
				break;
			}
		}
		return frameworkElement2;
	}

	public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
	{
		if (parent == null)
		{
			return null;
		}
		T val = null;
		int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child as T == null)
			{
				val = FindChild<T>(child, childName);
				if (val != null)
				{
					break;
				}
				continue;
			}
			if (!string.IsNullOrEmpty(childName))
			{
				if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
				{
					val = (T)child;
					break;
				}
				continue;
			}
			val = (T)child;
			break;
		}
		return val;
	}

	private static childItem FindVisualChild<childItem>(this DependencyObject obj) where childItem : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(obj, i);
			if (child != null && child is childItem)
			{
				return (childItem)child;
			}
			childItem val = child.FindVisualChild<childItem>();
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static DependencyObject FindChild(DependencyObject o, Type childType)
	{
		DependencyObject result = null;
		if (o != null)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(o);
			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(o, i);
				if (child.GetType() != childType)
				{
					result = FindChild(child, childType);
					continue;
				}
				result = child;
				break;
			}
		}
		return result;
	}

	public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
	{
		if (depObj == null)
		{
			yield break;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
			if (child != null && child is T)
			{
				yield return (T)child;
			}
			foreach (T item in FindVisualChildren<T>(child))
			{
				yield return item;
			}
		}
	}
}
