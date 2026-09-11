using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN.UI.MessageBoxWindows.InfoMessageBox;

public partial class MessageBoxWindowWindow : Window, IComponentConnector
{
	public MessageBoxWindowWindow(string title, string subtitle)
	{
		InitializeComponent();
		TextBlockTitle.Text = title;
		SubTitle.Text = subtitle;
	}

	private void SetTextBoxWidth()
	{
		TextBlockTitle.Width = GetWidth();
		SubTitle.Width = GetWidth();
	}

	private double GetWidth()
	{
		return Window.Width - 50.0;
	}

	private void XCloseImage_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		Close();
	}

	private void IUnderstand_OnMouseDown(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void MessageBoxWindowWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		SetTextBoxWidth();
	}
}
