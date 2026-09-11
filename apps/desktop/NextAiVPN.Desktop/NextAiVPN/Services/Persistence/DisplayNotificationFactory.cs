using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

internal class DisplayNotificationFactory(IBrowserLinksOpener browserLinksOpener) : IDisplayNotificationFactory
{
	public void ManageNotifications(ref ListView notificationList)
	{
		foreach (Notification item in (IEnumerable)notificationList.Items)
		{
			if (item == null)
			{
				break;
			}
			if (string.IsNullOrEmpty(item.Description))
			{
				continue;
			}
			if (item.Description.StartsWith("<") && item.Description.EndsWith(">"))
			{
				TextBlock textBlock = GetTextBlock(notificationList, item);
				if (textBlock == null)
				{
					break;
				}
				new HtmlNotificationHelper(browserLinksOpener).ConvertToTextBlock(item.Description, ref textBlock);
			}
			else
			{
				if (!item.Description.Contains("http") && !item.Description.Contains("ftp"))
				{
					continue;
				}
				TextBlock textBlock2 = GetTextBlock(notificationList, item);
				if (textBlock2 == null)
				{
					break;
				}
				foreach (string item2 in UrlFinder.FindUrlList(item.Description))
				{
					if (item2.Contains("http") || item2.Contains("ftp"))
					{
						TextBlock textBlock3 = new TextBlock
						{
							TextWrapping = TextWrapping.Wrap,
							Style = (Application.Current.FindResource("LinksTextBlockStyle") as System.Windows.Style),
							Text = item2,
							Margin = new Thickness(0.0)
						};
						textBlock3.MouseLeftButtonDown += TxtBlock_MouseLeftButtonDown;
						textBlock2.Inlines.Add(textBlock3);
					}
					else
					{
						textBlock2.Inlines.Add(item2);
					}
				}
			}
		}
	}

	private static TextBlock GetTextBlock(ItemsControl notificationList, Notification notification)
	{
		if (!(notificationList.ItemContainerGenerator.ContainerFromItem(notification) is ListViewItem parent))
		{
			return null;
		}
		if (!(ControlFinder.FindChild(parent, "NotificationDescription") is TextBlock textBlock))
		{
			return null;
		}
		textBlock.Text = string.Empty;
		return textBlock;
	}

	private void TxtBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is TextBlock textBlock && !string.IsNullOrEmpty(textBlock.Text))
		{
			browserLinksOpener.OpenBrowserLink(textBlock.Text);
		}
	}
}
