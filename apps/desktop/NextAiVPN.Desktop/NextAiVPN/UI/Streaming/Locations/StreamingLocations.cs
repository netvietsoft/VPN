using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Streaming.Locations;

public partial class StreamingLocations : UserControl, IComponentConnector
{
	private StreamingLocationsViewModel _streamingLocationsViewModel;

	private DoubleAnimation _da;

	private RotateTransform _rt;

	public StreamingLocationsViewModel StreamingLocationsViewModel
	{
		get
		{
			return _streamingLocationsViewModel;
		}
		set
		{
			if (_streamingLocationsViewModel != value)
			{
				_streamingLocationsViewModel = value;
				if (LocationsListHeader.SearchTextChangedFunc == null)
				{
					LocationsListHeader locationsListHeader = LocationsListHeader;
					locationsListHeader.SearchTextChangedFunc = (Action<string>)Delegate.Combine(locationsListHeader.SearchTextChangedFunc, new Action<string>(StreamingLocationsViewModel.SearchTextChangedFunc));
				}
				if (LocationsListHeader.PingSortFunc == null)
				{
					LocationsListHeader locationsListHeader2 = LocationsListHeader;
					locationsListHeader2.PingSortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader2.PingSortFunc, new Action<LocationsOrder, bool>(StreamingLocationsViewModel.PingSortFunc));
				}
				if (LocationsListHeader.CountrySortFunc == null)
				{
					LocationsListHeader locationsListHeader3 = LocationsListHeader;
					locationsListHeader3.CountrySortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader3.CountrySortFunc, new Action<LocationsOrder, bool>(StreamingLocationsViewModel.CountrySortFunc));
				}
			}
		}
	}

	public StreamingLocations()
	{
		InitializeComponent();
		LocationsListHeader.SetAsFavoriteHeader();
	}

	public async void SetCountryFlagSpinnerAnimation(string countryName)
	{
		try
		{
			Image flagImageByCountryName = GetFlagImageByCountryName(countryName);
			if (flagImageByCountryName != null)
			{
				flagImageByCountryName.Source = new BitmapImage(new Uri(IconHelper.GetIcon("spinnerblue_whitebg"), UriKind.Relative));
				SpinnerAnimation(animate: true, flagImageByCountryName);
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetCountryFlagSpinnerAnimation", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Streaming\\Locations\\StreamingLocations.xaml.cs", 68);
		}
	}

	public async void SetCountryFlagConnected(string countryName)
	{
		try
		{
			Image flagImageByCountryName = GetFlagImageByCountryName(countryName);
			if (flagImageByCountryName != null)
			{
				flagImageByCountryName.Source = new BitmapImage(new Uri(IconHelper.GetIcon("check_whitebg"), UriKind.Relative));
				SpinnerAnimation(animate: false, flagImageByCountryName);
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetCountryFlagConnected", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Streaming\\Locations\\StreamingLocations.xaml.cs", 85);
		}
	}

	public async void SetCountryFlag(string countryName, string countryCode)
	{
		try
		{
			Image flagImageByCountryName = GetFlagImageByCountryName(countryName);
			if (flagImageByCountryName != null)
			{
				flagImageByCountryName.Source = new BitmapImage(new Uri("/Resources/Flags/" + countryCode + ".png", UriKind.Relative));
				SpinnerAnimation(animate: false, flagImageByCountryName);
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetCountryFlag", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Streaming\\Locations\\StreamingLocations.xaml.cs", 102);
		}
	}

	private void SpinnerAnimation(bool animate, Image image)
	{
		_da = new DoubleAnimation();
		_rt = new RotateTransform();
		_da.From = 0.0;
		_da.To = 360.0;
		_da.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_da.RepeatBehavior = RepeatBehavior.Forever;
		image.RenderTransform = _rt;
		image.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			_rt.BeginAnimation(RotateTransform.AngleProperty, _da);
			return;
		}
		_rt.BeginAnimation(RotateTransform.AngleProperty, null);
		_rt = null;
		_da = null;
	}

	private Image GetFlagImageByCountryName(string countryName)
	{
		Image result = null;
		List<StackPanel> list = new List<StackPanel>();
		foreach (IStreamingLocation item in (IEnumerable)LocationsList.Items)
		{
			DependencyObject dependencyObject = LocationsList.ItemContainerGenerator.ContainerFromItem(item);
			if (dependencyObject != null)
			{
				list.AddRange(FindVisualChildren<StackPanel>(dependencyObject));
			}
		}
		foreach (StackPanel item2 in list)
		{
			TextBlock textBlock = FindChild<TextBlock>(item2, "CountryFullName");
			if (textBlock != null && textBlock.Text.Equals(countryName))
			{
				Image image = FindChild<Image>(item2, "CountryFlag");
				if (image != null)
				{
					result = image;
					break;
				}
			}
		}
		return result;
	}

	private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
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
			if (child as T != null)
			{
				if (string.IsNullOrEmpty(childName))
				{
					val = (T)child;
					break;
				}
				if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
				{
					val = (T)child;
					break;
				}
			}
			val = FindChild<T>(child, childName);
			if (val != null)
			{
				break;
			}
		}
		return val;
	}

	private List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
	{
		List<T> list = new List<T>();
		int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child != null && child is T)
			{
				list.Add((T)child);
			}
			list.AddRange(FindVisualChildren<T>(child));
		}
		return list;
	}
}
