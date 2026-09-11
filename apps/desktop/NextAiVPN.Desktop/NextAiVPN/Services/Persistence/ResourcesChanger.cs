using System;
using System.Collections.Generic;
using System.Windows;

namespace NextAiVPN.Services.Persistence;

internal class ResourcesChanger : IResourcesChanger
{
	private ResourceDictionary _currentTheme;

	public void ChangeResource(string stylePath)
	{
		ResourceDictionary newTheme = Application.LoadComponent(new Uri(stylePath, UriKind.Relative)) as ResourceDictionary;
		SwapTheme(Application.Current.Resources.MergedDictionaries, newTheme);
	}

	internal void SwapTheme(ICollection<ResourceDictionary> mergedDictionaries, ResourceDictionary newTheme)
	{
		if (_currentTheme != null)
		{
			mergedDictionaries.Remove(_currentTheme);
		}
		mergedDictionaries.Add(newTheme);
		_currentTheme = newTheme;
	}
}
