using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using NextAiVPN.Services;

namespace NextAiVPN.UI;

public partial class SignInCommonWindow : Window, IComponentConnector
{
	private readonly ISignInService _signInNextAiTechnologyService;

	private readonly ISignInService _signInNextAiGlobalService;

	private readonly MainWindow _mainWindow;

	public SignInCommonWindow(ISignInService signInNextAiTechnologyService, ISignInService signInNextAiGlobalService, MainWindow mainWindow)
	{
		_signInNextAiTechnologyService = signInNextAiTechnologyService;
		_signInNextAiGlobalService = signInNextAiGlobalService;
		_mainWindow = mainWindow;
		InitializeComponent();
		WindowHeader.GetParent(this);
	}

	private void SignInCommonWindow_OnClosing(object sender, CancelEventArgs e)
	{
		_mainWindow.Show();
	}

	private void NextAiGlobalButton_OnClick(object sender, RoutedEventArgs e)
	{
		_signInNextAiGlobalService.SignInValidations();
		Hide();
	}

	private void NextAiTechnologyButton_OnClick(object sender, RoutedEventArgs e)
	{
		_signInNextAiTechnologyService.SignInValidations();
		Hide();
	}
}
