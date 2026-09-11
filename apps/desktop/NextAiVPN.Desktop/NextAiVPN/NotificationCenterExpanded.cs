using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using Newtonsoft.Json;

namespace NextAiVPN;

public partial class NotificationCenterExpanded : UserControl, IComponentConnector, IStyleConnector
{
	private readonly DoubleAnimation _doubleAnimation = new DoubleAnimation();

	private readonly RotateTransform _rotateTransform = new RotateTransform();

	private DispatcherTimer _notificationsUpdateTimer;

	private ObservableCollection<Notification> _notificationList;

	private int _index = -1;

	private VPNWindowExpanded _expandedVpnWindow;

	private IDisplayNotificationFactory _displayNotificationFactory;

	private IBrowserLinksOpener _browserLinksOpener;

	public INotificationService NotificationService { get; set; }

	public IBugsnagService BugsnagService { get; set; } = new NullBugsnagService();

	public NotificationCenterExpanded()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
		notificationslist.SizeChanged += Notificationslist_SizeChanged;
		SetNotificationsUpdateTimer();
	}

	private void Notificationslist_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		ManageNotifications();
		notificationslist.Loaded += TestUserControl_Loaded;
	}

	private void NotificationCenterExpanded_Loaded1(object sender, RoutedEventArgs e)
	{
		notificationslist.Loaded += TestUserControl_Loaded;
	}

	private void TestUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		PresentationSource.FromVisual((Visual)sender).ContentRendered += TestUserControl_ContentRendered;
	}

	private void TestUserControl_ContentRendered(object sender, EventArgs e)
	{
		((PresentationSource)sender).ContentRendered -= TestUserControl_ContentRendered;
		ManageNotifications();
	}

	private void SetNotificationsUpdateTimer()
	{
		_notificationsUpdateTimer = new DispatcherTimer();
		_notificationsUpdateTimer.Tick += _notificationsUpdateTimer_Tick;
		_notificationsUpdateTimer.Interval = TimeSpan.FromHours(24);
	}

	private void StyleChangedEventStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		ChangeStyle(appStyle);
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += StyleChangedEventStyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= StyleChangedEventStyleChanged;
	}

	private static void ChangeStyle(NextAiVPN.Services.Persistence.Style appStyle)
	{
	}

	private void BindNotifications()
	{
		notificationslist.ItemsSource = (from y in _notificationList.Where((Notification x) => x.Header != "NEW").ToList()
			where y.Header != "OLDEST"
			select y).ToList();
		SortList();
	}

	private void ShowNotificationScreenState(int param, bool toggle)
	{
		switch (param)
		{
		case 1:
			if (toggle)
			{
				EmptyState.Visibility = Visibility.Visible;
				notificationslist.Visibility = Visibility.Collapsed;
			}
			else
			{
				EmptyState.Visibility = Visibility.Collapsed;
				notificationslist.Visibility = Visibility.Visible;
			}
			break;
		case 2:
			if (toggle)
			{
				LoadingState.Visibility = Visibility.Visible;
				notificationslist.Visibility = Visibility.Collapsed;
			}
			else
			{
				LoadingState.Visibility = Visibility.Collapsed;
				notificationslist.Visibility = Visibility.Visible;
			}
			break;
		default:
			throw new InvalidOperationException("ShowNotificationScreenState");
		}
	}

	public void GetExpandedVpnWindow(VPNWindowExpanded expanded, IBrowserLinksOpener browserLinksOpener)
	{
		_expandedVpnWindow = expanded;
		_browserLinksOpener = browserLinksOpener;
		_displayNotificationFactory = new DisplayNotificationFactory(_browserLinksOpener);
	}

	public async Task LoadNotifications()
	{
		ShowNotificationScreenState(1, toggle: false);
		ShowNotificationScreenState(2, toggle: true);
		await Task.Delay(200);
		if (_expandedVpnWindow.SdkObject.AccountTypeHelper.GetAccountType() == AccountType.NextAiGlobal)
		{
			await Utils.Api.RefreshNextAiGlobalTokensIfNeeded();
		}
		_notificationList = await NotificationService.GetNotificationsAsync();
		BindNotifications();
		await Task.Delay(200);
		ManageNotifications();
		ShowNotificationScreenState(2, toggle: false);
		ShowNotificationScreenState(1, notificationslist.Items.Count <= 0);
		RefreshNotificationsUpdateTimer();
	}

	public void ManageNotifications()
	{
		_displayNotificationFactory.ManageNotifications(ref notificationslist);
	}

	private void RefreshNotificationsUpdateTimer()
	{
		_notificationsUpdateTimer.Stop();
		_notificationsUpdateTimer.Start();
	}

	private async void _notificationsUpdateTimer_Tick(object sender, EventArgs e)
	{
		if (_expandedVpnWindow != null)
		{
			bool hasNewNotification = await NotificationService.HasNewNotificationAsync();
			await LoadNotifications();
			if (hasNewNotification)
			{
				_expandedVpnWindow?.SdkObject.TaskBarService.ShowSystemTrayNotificationIcon();
			}
		}
	}

	private void Notificationslist_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		_index = notificationslist.SelectedIndex;
	}

	private async void Readoption_Click(object sender, RoutedEventArgs e)
	{
		await SetNotificationAsReadAsync();
	}

	private async void Deleteoption_Click(object sender, RoutedEventArgs e)
	{
		await DeleteNotificationAsync();
	}

	private async Task SetNotificationAsReadAsync()
	{
		if (_index <= -1)
		{
			return;
		}
		Notification notification = (Notification)notificationslist.Items[_index];
		notification.IsRead = true;
		notification.ItemBackground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		notification.TitleFontColor = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#787879");
		ObservableCollection<Notification> observableCollection = new ObservableCollection<Notification>();
		foreach (Notification item in (from y in _notificationList.Where((Notification x) => x.Header != "NEW").ToList()
			where y.Header != "OLDEST"
			select y).ToList())
		{
			if (notification.Id == item.Id)
			{
				observableCollection.Add(notification);
			}
			else
			{
				observableCollection.Add(item);
			}
		}
		string contents = JsonConvert.SerializeObject(observableCollection, Formatting.Indented);
		File.WriteAllText(VPNConstants.FilePath.NotificationsJsonFilePath, contents);
		_notificationList.Clear();
		_notificationList = await NotificationService.GetNotificationsAsync();
		notificationslist.ItemsSource = (from y in _notificationList.Where((Notification x) => x.Header != "NEW").ToList()
			where y.Header != "OLDEST"
			select y).ToList();
		notificationslist.Items.Refresh();
		SortList();
	}

	private async Task DeleteNotificationAsync()
	{
		if (_index <= -1)
		{
			return;
		}
		Notification notification = (Notification)notificationslist.Items[_index];
		ObservableCollection<Notification> observableCollection = new ObservableCollection<Notification>();
		foreach (Notification item in (from y in _notificationList.Where((Notification x) => x.Header != "NEW").ToList()
			where y.Header != "OLDEST"
			select y).ToList())
		{
			if (notification.Id != item.Id)
			{
				observableCollection.Add(item);
			}
		}
		string contents = JsonConvert.SerializeObject(observableCollection, Formatting.Indented);
		File.WriteAllText(VPNConstants.FilePath.NotificationsJsonFilePath, contents);
		_notificationList.Clear();
		_notificationList = await NotificationService.GetNotificationsAsync();
		notificationslist.ItemsSource = (from y in _notificationList.Where((Notification x) => x.Header != "NEW").ToList()
			where y.Header != "OLDEST"
			select y).ToList();
		notificationslist.Items.Refresh();
		SortList();
		if (notificationslist.Items.Count <= 0)
		{
			notificationslist.Visibility = Visibility.Collapsed;
			EmptyState.Visibility = Visibility.Visible;
		}
	}

	private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ComboBox comboBox = sender as ComboBox;
		if (_notificationList != null)
		{
			switch (comboBox.SelectedIndex)
			{
			case 0:
				OrderListBy("Most recent");
				NotificationService.SaveSortSelected("Most recent");
				break;
			case 1:
				OrderListBy("Unread first");
				NotificationService.SaveSortSelected("Unread first");
				break;
			case 2:
				OrderListBy("Oldest");
				NotificationService.SaveSortSelected("Oldest");
				break;
			}
		}
	}

	public void OrderListBy(string by)
	{
		switch (by)
		{
		case "Most recent":
		{
			List<Notification> list2 = _notificationList.OrderByDescending(delegate(Notification d)
			{
				try
				{
					return d.Date;
				}
				catch (Exception)
				{
					return DateTime.Now;
				}
			}).ToList();
			_notificationList.Clear();
			foreach (Notification item in list2)
			{
				_notificationList.Add(item);
			}
			break;
		}
		case "Unread first":
		{
			List<Notification> list3 = _notificationList.OrderBy((Notification d) => d.IsRead).ToList();
			_notificationList.Clear();
			foreach (Notification item2 in list3)
			{
				_notificationList.Add(item2);
			}
			break;
		}
		case "Oldest":
		{
			List<Notification> list = _notificationList.OrderBy(delegate(Notification d)
			{
				try
				{
					return d.Date;
				}
				catch (Exception)
				{
					return DateTime.Now;
				}
			}).ToList();
			_notificationList.Clear();
			foreach (Notification item3 in list)
			{
				_notificationList.Add(item3);
			}
			break;
		}
		}
		notificationslist.ItemsSource = (from y in _notificationList.Where((Notification x) => x.Header != "NEW").ToList()
			where y.Header != "OLDEST"
			select y).ToList();
		notificationslist.Items.Refresh();
	}

	public void SortingIndex(string by)
	{
		switch (by)
		{
		case "Most recent":
			sorting.SelectedIndex = 0;
			break;
		case "Unread first":
			sorting.SelectedIndex = 1;
			break;
		case "Oldest":
			sorting.SelectedIndex = 2;
			break;
		}
	}

	public void SortList()
	{
		sorting.SelectedIndex = -1;
		SortingIndex(Utils.AppSettingsHelper.GetValue("sort"));
	}

	private void DotsImage_MouseDown(object sender, MouseButtonEventArgs e)
	{
		try
		{
			if (ItemsControl.ContainerFromElement(notificationslist, e.OriginalSource as DependencyObject) is ListViewItem listViewItem)
			{
				Notification notification = (Notification)listViewItem.Content;
				for (int i = 0; i < notificationslist.Items.Count; i++)
				{
					Notification notification2 = (Notification)notificationslist.Items[i];
					if (notification.Id == notification2.Id)
					{
						_index = i;
						break;
					}
				}
			}
			if (e.ChangedButton == MouseButton.Left)
			{
				Image image = (Image)(sender as Grid).Children[1];
				ContextMenu contextMenu = image.ContextMenu;
				contextMenu.PlacementTarget = image;
				contextMenu.IsOpen = true;
				e.Handled = true;
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "DotsImage_MouseDown", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\NotificationCenterExpanded.xaml.cs", 459);
		}
	}

	private void MenuGrid_MouseEnter(object sender, MouseEventArgs e)
	{
		((sender as Grid).Children[0] as Border).Opacity = 0.12;
	}

	private void MenuGrid_MouseLeave(object sender, MouseEventArgs e)
	{
		((sender as Grid).Children[0] as Border).Opacity = 0.0;
	}

	public static Visual GetDescendantByType(Visual element, Type type)
	{
		if (element == null)
		{
			return null;
		}
		if (element.GetType() == type)
		{
			return element;
		}
		Visual visual = null;
		if (element is FrameworkElement)
		{
			(element as FrameworkElement).ApplyTemplate();
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
		{
			visual = GetDescendantByType(VisualTreeHelper.GetChild(element, i) as Visual, type);
			if (visual != null)
			{
				break;
			}
		}
		return visual;
	}

	private void UpdateBtn_OnClick(object sender, RoutedEventArgs e)
	{
		ListViewItem notificationItem = ItemsControl.ContainerFromElement(notificationslist, e.OriginalSource as DependencyObject) as ListViewItem;
		SpinnerAnimation(notificationItem, isAnimate: true);
		_expandedVpnWindow.UpdateService.Update();
	}

	public void SpinnerAnimation(ListViewItem notificationItem, bool isAnimate)
	{
		_doubleAnimation.From = 0.0;
		_doubleAnimation.To = 360.0;
		_doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		try
		{
			Image image = FindVisualChild<Image>(notificationItem);
			Button obj = FindVisualChild<Button>(notificationItem);
			TextBlock textBlock = FindVisualChild<TextBlock>(obj);
			image.RenderTransform = _rotateTransform;
			image.RenderTransformOrigin = new Point(0.5, 0.5);
			if (isAnimate)
			{
				_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _doubleAnimation);
				textBlock.Visibility = Visibility.Hidden;
				image.Visibility = Visibility.Visible;
			}
			else
			{
				_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
				textBlock.Visibility = Visibility.Visible;
				image.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex)
		{
			BugsnagService.Notify("Spinner Animation Error. Exception " + ex.Message);
		}
	}

	public void SpinnerAnimation(bool isAnimate)
	{
		_doubleAnimation.From = 0.0;
		_doubleAnimation.To = 360.0;
		_doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		try
		{
			Image image = FindVisualChild<Image>(notificationslist);
			Button obj = FindVisualChild<Button>(notificationslist);
			TextBlock textBlock = FindVisualChild<TextBlock>(obj);
			image.RenderTransform = _rotateTransform;
			image.RenderTransformOrigin = new Point(0.5, 0.5);
			if (isAnimate)
			{
				_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _doubleAnimation);
				textBlock.Visibility = Visibility.Hidden;
				image.Visibility = Visibility.Visible;
			}
			else
			{
				_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
				textBlock.Visibility = Visibility.Visible;
				image.Visibility = Visibility.Hidden;
			}
		}
		catch (Exception ex)
		{
			BugsnagService.Notify("Spinner Animation Error. Exception " + ex.Message);
		}
	}

	private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(obj, i);
			if (child != null && child is childItem)
			{
				if (!(child.GetType() == typeof(Image)) || (child as Image).Name.Equals("UpdateSpinner"))
				{
					return (childItem)child;
				}
				continue;
			}
			childItem val = FindVisualChild<childItem>(child);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	public DependencyObject FindChild(DependencyObject o, Type childType)
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

	private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		TextBlock textBlock = sender as TextBlock;
		try
		{
			_browserLinksOpener.OpenBrowserLink(textBlock.Text);
		}
		catch (Exception ex)
		{
			BugsnagService.Notify("Notifications : can't define uri. " + ex.Message);
		}
	}

	private void GoToAccountButton_OnClick(object sender, RoutedEventArgs e)
	{
		_expandedVpnWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Account);
	}
}
