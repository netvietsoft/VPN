using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace NextAiVPN.UI.ToolsContextMenu;

public class ToolsControlViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkMonitor;

	private bool _isContextMenuOpen;

	private Visibility _customerSupportVisibility = Visibility.Collapsed;

	private Visibility _signOutVisibility = Visibility.Collapsed;

	public bool IsContextMenuOpen
	{
		get
		{
			return _isContextMenuOpen;
		}
		set
		{
			if (_isContextMenuOpen != value)
			{
				_isContextMenuOpen = value;
				OnPropertyChanged("IsContextMenuOpen");
			}
		}
	}

	public Visibility CustomerSupportVisibility
	{
		get
		{
			return _customerSupportVisibility;
		}
		set
		{
			if (_customerSupportVisibility != value)
			{
				_customerSupportVisibility = value;
				OnPropertyChanged("CustomerSupportVisibility");
			}
		}
	}

	public Visibility SignOutVisibility
	{
		get
		{
			return _signOutVisibility;
		}
		set
		{
			if (_signOutVisibility != value)
			{
				_signOutVisibility = value;
				OnPropertyChanged("SignOutVisibility");
			}
		}
	}

	public ICommand ToolsMouseDownCommand { get; set; }

	public ICommand QuitCommand { get; set; }

	public ICommand SignOutCommand { get; set; }

	public ICommand CustomerSupportCommand { get; set; }

	public Action OnSignOutAction { get; set; }

	public ToolsControlViewModel(SDKMonitor sdkMonitor)
	{
		_sdkMonitor = sdkMonitor;
		ToolsMouseDownCommand = new ActionCommand(ToolsMouseDownCommandExecute);
		QuitCommand = new ActionCommand(QuitCommandExecute);
		SignOutCommand = new ActionCommand(SignOutCommandExecute);
		CustomerSupportCommand = new ActionCommand(CustomerSupportCommandExecute);
		IsContextMenuOpen = true;
	}

	private void CustomerSupportCommandExecute()
	{
		_sdkMonitor.BrowserLinksOpener.OpenBrowserLink(3);
	}

	private void SignOutCommandExecute()
	{
		_sdkMonitor.TaskBarService.SignOutCommonFunction();
		if (_sdkMonitor.TaskBarService.SignOut)
		{
			OnSignOutAction?.Invoke();
		}
	}

	private void QuitCommandExecute()
	{
		_sdkMonitor.TaskBarService.QuitCommonFunction();
	}

	private void ToolsMouseDownCommandExecute()
	{
		IsContextMenuOpen = !IsContextMenuOpen;
	}

	public void SetVisibility(bool isMainScreen)
	{
		Visibility signOutVisibility = (CustomerSupportVisibility = (isMainScreen ? Visibility.Collapsed : Visibility.Visible));
		SignOutVisibility = signOutVisibility;
	}
}
