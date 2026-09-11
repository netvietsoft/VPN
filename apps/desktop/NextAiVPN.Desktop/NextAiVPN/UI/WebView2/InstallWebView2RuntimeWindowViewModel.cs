using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace NextAiVPN.UI.WebView2;

public class InstallWebView2RuntimeWindowViewModel
{
	private InstallWebView2RuntimeWindow _parentWindow;

	public bool DialogResult { get; set; }

	public string TitleText { get; set; } = "Install Windows Edge";

	public string BodyText { get; set; } = "It looks like you don't have a compatible Edge Webview2 Runtime. To seamlessly run our app, please consider installing Windows Edge.";

	public ICommand CloseXButtonClickCommand { get; set; }

	public ICommand AgreeButtonClickCommand { get; set; }

	public ICommand CancelButtonClickCommand { get; set; }

	public InstallWebView2RuntimeWindowViewModel()
	{
		CloseXButtonClickCommand = new ActionCommand(CloseXButtonClickCommandExecute);
		AgreeButtonClickCommand = new ActionCommand(AgreeButtonClickCommandExecute);
		CancelButtonClickCommand = new ActionCommand(CancelButtonClickCommandExecute);
	}

	public void SetParentWindow(InstallWebView2RuntimeWindow parentWindow)
	{
		_parentWindow = parentWindow;
	}

	private void CancelButtonClickCommandExecute()
	{
		Close(result: false);
	}

	private void AgreeButtonClickCommandExecute()
	{
		Close(result: true);
	}

	private void CloseXButtonClickCommandExecute()
	{
		Close(result: false);
	}

	private void Close(bool result)
	{
		DialogResult = result;
		_parentWindow?.Close();
	}
}
