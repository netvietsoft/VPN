using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.WebView2;

public partial class InstallWebView2RuntimeWindow : Window, IComponentConnector
{
	private readonly InstallWebView2RuntimeWindowViewModel _view2RuntimeWindowViewModel;

	public new bool DialogResult
	{
		get
		{
			return _view2RuntimeWindowViewModel.DialogResult;
		}
		set
		{
			_view2RuntimeWindowViewModel.DialogResult = value;
		}
	}

	public InstallWebView2RuntimeWindow(InstallWebView2RuntimeWindowViewModel view2RuntimeWindowViewModel)
	{
		_view2RuntimeWindowViewModel = view2RuntimeWindowViewModel;
		_view2RuntimeWindowViewModel.SetParentWindow(this);
		base.DataContext = _view2RuntimeWindowViewModel;
		InitializeComponent();
	}

	private void InstallWebView2RuntimeWindow_OnClosing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		base.Visibility = Visibility.Hidden;
	}
}
