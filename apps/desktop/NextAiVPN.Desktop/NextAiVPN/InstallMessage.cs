using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class InstallMessage : Window, IComponentConnector
{
	private readonly ITapDriverInstaller _tapDriverInstaller;

	private readonly IBrowserLinksOpener _browserLinksOpener;

	private int _retryCount = 1;

	private readonly DoubleAnimation _da = new DoubleAnimation();

	private readonly RotateTransform _rt = new RotateTransform();

	public InstallMessage(ITapDriverInstaller tapDriverInstaller, IBrowserLinksOpener browserLinksOpener)
	{
		_tapDriverInstaller = tapDriverInstaller;
		_browserLinksOpener = browserLinksOpener;
		InitializeComponent();
		_tapDriverInstaller.InstallStatusChanged += OnInstallStatusChanged;
		base.Closed += delegate
		{
			_tapDriverInstaller.InstallStatusChanged -= OnInstallStatusChanged;
		};
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}

	private async void OnInstallStatusChanged(TapDriverInstallStatus status)
	{
		switch (status)
		{
		case TapDriverInstallStatus.Installing:
			await SetButtonIcon(1);
			break;
		case TapDriverInstallStatus.Succeeded:
			await SetButtonIcon(2);
			break;
		case TapDriverInstallStatus.Failed:
			await SetButtonIcon(3);
			break;
		}
	}

	private async Task ClosingTasks(bool close)
	{
		await Task.Delay(1500);
		if (close)
		{
			Close();
		}
	}

	private async void Button_Click(object sender, RoutedEventArgs e)
	{
		Button button = sender as Button;
		if (button.Content.ToString().Equals("Got it"))
		{
			await ClosingTasks(close: true);
			return;
		}
		if (button.Content.ToString().Equals("Retry"))
		{
			_retryCount++;
		}
		await _tapDriverInstaller.InstallTapDriver();
	}

	public void SpinnerAnimation(bool animate)
	{
		_da.From = 0.0;
		_da.To = 360.0;
		_da.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_da.RepeatBehavior = RepeatBehavior.Forever;
		btnInstallSpinner.RenderTransform = _rt;
		btnInstallSpinner.RenderTransformOrigin = new Point(0.5, 0.5);
		_rt.BeginAnimation(RotateTransform.AngleProperty, animate ? _da : null);
	}

	public async Task SetButtonIcon(int option)
	{
		switch (option)
		{
		case 1:
			SpinnerAnimation(animate: true);
			btnInstall.Content = string.Empty;
			btnInstallSpinner.Visibility = Visibility.Visible;
			break;
		case 2:
			SpinnerAnimation(animate: false);
			btnInstall.Content = "Got it";
			MessageTitle.Text = "Congrats";
			SetProgressText("The installation was successful, you are now able to connect");
			btnInstallSpinner.Visibility = Visibility.Collapsed;
			break;
		case 3:
			SpinnerAnimation(animate: false);
			if (_retryCount < 2)
			{
				SetProgressText("The installation failed, please try again\n");
				btnInstall.Content = "Retry";
				MessageTitle.Text = "Error";
				btnInstallSpinner.Visibility = Visibility.Collapsed;
			}
			else
			{
				SetProgressText("Unable to install, please contact our");
				CustomerLink.Visibility = Visibility.Visible;
				MessageTitle.Text = "Error";
				btnInstallSpinner.Visibility = Visibility.Collapsed;
				btnInstall.Visibility = Visibility.Collapsed;
			}
			break;
		}
	}

	public void SetProgressText(string message1)
	{
		MessageContent.Text = message1;
	}

	private void CustomerLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_browserLinksOpener.OpenBrowserLink(3);
		Close();
	}
}
