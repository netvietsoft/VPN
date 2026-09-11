using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.MessageBoxWindows.SignOutMessageWindow;

public partial class SignOutMessageBoxWindow : Window, IComponentConnector
{
	private readonly SignOutMessageBoxWindowViewModel _messageBoxWindowViewModel;

	public new bool DialogResult => _messageBoxWindowViewModel.DialogResult;

	public SignOutMessageBoxWindow(SignOutMessageBoxWindowViewModel messageBoxWindowViewModel)
	{
		InitializeComponent();
		_messageBoxWindowViewModel = messageBoxWindowViewModel;
		_messageBoxWindowViewModel.SetParent(this);
		base.Closing += _messageBoxWindowViewModel.OnWindowClosing;
		base.DataContext = _messageBoxWindowViewModel;
	}
}
