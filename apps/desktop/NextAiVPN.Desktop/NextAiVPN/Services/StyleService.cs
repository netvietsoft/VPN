using System;
using System.Collections;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;
using NextAiVPN.Extensions;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class StyleService : IStyleService
{
	private const string LightStyle = "Resources/Styles/Theme.Light.xaml";

	private const string DarkStyle = "Resources/Styles/Theme.Dark.xaml";

	private SDKMonitor _sdk;

	private readonly IResourcesChanger _resourcesChanger;

	public StyleService(SDKMonitor sdk)
	{
		_sdk = sdk;
		_resourcesChanger = new ResourcesChanger();
	}

	public void SetStyle(NextAiVPN.Services.Persistence.Style style)
	{
		// [VI] Luôn luôn nạp Clean Emerald Theme chuẩn để đảm bảo tính nhất quán giao diện
		// [EN] Always load standard Clean Emerald Theme to ensure UI consistency
		_resourcesChanger.ChangeResource("Resources/Styles/Theme.Light.xaml");
	}

	public void SetTaskbarIconStyle(NextAiVPN.Services.Persistence.Style style, TaskbarIcon taskBar, string connectionStatus)
	{
		Icon icon = null;
		switch (connectionStatus.ToLower())
		{
		case "connected":
			icon = (style.Equals(NextAiVPN.Services.Persistence.Style.Dark) ? ResourceFile.DarkModeSuccess : ResourceFile.icontray_connected);
			break;
		case "disconnected":
			icon = (style.Equals(NextAiVPN.Services.Persistence.Style.Dark) ? ResourceFile.DarkModeDefault : ResourceFile.icontray_regular);
			break;
		case "error":
			icon = (style.Equals(NextAiVPN.Services.Persistence.Style.Dark) ? ResourceFile.DarkModeError : ResourceFile.icontray_error);
			break;
		}
		taskBar.Icon = icon;
	}

	public void SetContextMenuStyle(NextAiVPN.Services.Persistence.Style style, TaskbarIcon taskBar)
	{
		if (taskBar?.ContextMenu != null)
		{
			if (style == NextAiVPN.Services.Persistence.Style.Dark)
			{
				System.Windows.Style style2 = Application.Current.FindResource("ContextMenuStyle") as System.Windows.Style;
				taskBar.ContextMenu.Style = style2;
			}
			else
			{
				taskBar.ContextMenu.Style = new System.Windows.Style();
			}
			ChangeMenusIcons(taskBar, style);
		}
	}

	public void SetContextMenuStyle(NextAiVPN.Services.Persistence.Style style, ContextMenu contextMenu)
	{
		if (style == NextAiVPN.Services.Persistence.Style.Dark)
		{
			System.Windows.Style style2 = Application.Current.FindResource("ContextMenuStyle") as System.Windows.Style;
			contextMenu.Style = style2;
		}
		else
		{
			contextMenu.Style = new System.Windows.Style();
		}
	}

	public void GetSdkObject(SDKMonitor sdkMonitor)
	{
		_sdk = sdkMonitor;
	}

	public bool HasSdkObject()
	{
		return _sdk != null;
	}

	private void ChangeMenusIcons(TaskbarIcon taskBar, NextAiVPN.Services.Persistence.Style style)
	{
		foreach (object item in (IEnumerable)taskBar.ContextMenu.Items)
		{
			if (item is MenuItem menuItem && (menuItem.Header.ToString().ToLower().Contains("Favorite".ToLower()) || menuItem.Header.ToString().ToLower().Contains("Best available".ToLower()) || menuItem.Header.ToString().ToLower().Contains("Disconnect".ToLower()) || menuItem.Header.ToString().ToLower().Contains("Notifications".ToLower()) || menuItem.Header.ToString().ToLower().Contains("For Streaming".ToLower())))
			{
				ChangeMenuItemIcon(menuItem, style);
			}
		}
	}

	private void ChangeMenuItemIcon(MenuItem menuItem, NextAiVPN.Services.Persistence.Style style)
	{
		if (menuItem.Header.ToString().ToLower().Contains("For Streaming".ToLower()))
		{
			menuItem.Icon = new System.Windows.Controls.Image
			{
				Source = ((style == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.start_lightmode.ToImageSource() : ResourceFile.start_darkmode.ToImageSource())
			};
		}
		else if (menuItem.Header.ToString().ToLower().Contains("Best available".ToLower()))
		{
			double opacity = 1.0;
			if (_sdk != null)
			{
				opacity = (_sdk.ConnectionStatus.Equals("connected") ? 0.4 : 1.0);
			}
			menuItem.Icon = new System.Windows.Controls.Image
			{
				Source = ((style == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.LightModeServerLocation.ToImageSource() : ResourceFile.DarkModeServerLocation.ToImageSource()),
				Opacity = opacity
			};
		}
		else if (menuItem.Header.ToString().ToLower().Contains("Disconnect".ToLower()))
		{
			menuItem.Icon = new System.Windows.Controls.Image
			{
				Source = ((style == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.Favorite.ToImageSource() : ResourceFile.DarkModeFavorite.ToImageSource())
			};
		}
		else if (menuItem.Header.ToString().ToLower().Contains("Favorite".ToLower()))
		{
			menuItem.Icon = new System.Windows.Controls.Image
			{
				Source = ((style == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.Favorite.ToImageSource() : ResourceFile.DarkModeFavorite.ToImageSource())
			};
		}
		else if (menuItem.Header.ToString().ToLower().Contains("Notifications".ToLower()))
		{
			if (menuItem.Icon != null)
			{
				menuItem.Icon = new System.Windows.Controls.Image
				{
					Source = ((style == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.NotificationsOrangeLightMode.ToImageSource() : ResourceFile.NotificationsOrangeDarkMode.ToImageSource())
				};
			}
		}
		else
		{
			menuItem.Icon = new System.Windows.Controls.Image
			{
				Source = ((style == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.icontray_regular.ToImageSource() : ResourceFile.DarkModeDefault.ToImageSource())
			};
		}
	}
}
