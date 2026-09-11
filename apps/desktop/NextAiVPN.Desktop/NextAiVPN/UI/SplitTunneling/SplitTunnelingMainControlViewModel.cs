using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services;
using NextAiVPN.UI.Controls.CircleCounter;

namespace NextAiVPN.UI.SplitTunneling;

public class SplitTunnelingMainControlViewModel : ViewModelBase
{
	private readonly ISplitTunnelingService _splitTunnelingAppService;

	private readonly ISplitTunnelingService _splitTunnelingDomainService;

	private readonly SplitTunnelingAppWindow _appWindow;

	private readonly SplitTunnelingDomainWindow _domainWindow;

	public CircleCounterControlViewModel AppCounterControlViewModel { get; private set; }

	public CircleCounterControlViewModel DomainCounterControlViewModel { get; private set; }

	public bool IsNeededToShowPopUp { get; set; }

	public string Title { get; private set; } = "Split Tunneling";

	public string Description { get; private set; } = "Select the apps or hostnames to bypass VPN";

	public string AppTitle { get; private set; } = "Apps";

	public string AppDescription { get; private set; } = "Select the apps you’d like to route through";

	public ICommand AppClickCommand { get; private set; }

	public string HostnameTitle { get; private set; } = "Hostnames";

	public string HostnameDescription { get; private set; } = "Add the hostname you want to bypass VPN";

	public ICommand HostnameClickCommand { get; private set; }

	public ICommand CloseButtonClickCommand { get; private set; }

	public SplitTunnelingMainWindowViewModel SplitTunnelingMainWindowViewModel { get; set; }

	public SplitTunnelingMainControlViewModel(ISplitTunnelingService splitTunnelingAppService, ISplitTunnelingService splitTunnelingDomainService, SplitTunnelingAppWindow appWindow, SplitTunnelingDomainWindow domainWindow, CircleCounterControlViewModel appCounterControlViewModel, CircleCounterControlViewModel domainCounterControlViewModel)
	{
		AppCounterControlViewModel = appCounterControlViewModel;
		DomainCounterControlViewModel = domainCounterControlViewModel;
		_splitTunnelingDomainService = splitTunnelingDomainService;
		_splitTunnelingAppService = splitTunnelingAppService;
		_appWindow = appWindow;
		_domainWindow = domainWindow;
		CloseButtonClickCommand = new ActionCommand(CloseButtonClickCommandExecute);
		AppClickCommand = new ActionCommand(AppClickCommandExecute);
		HostnameClickCommand = new ActionCommand(HostnameClickCommandExecute);
	}

	public void SetCounters()
	{
		AppCounterControlViewModel.CountText = _splitTunnelingAppService.GetList().Count().ToString();
		DomainCounterControlViewModel.CountText = _splitTunnelingDomainService.GetList().Count().ToString();
	}

	public void OnDisconnect()
	{
		IsNeededToShowPopUp = false;
	}

	private void HostnameClickCommandExecute()
	{
		IEnumerable<string> list = _splitTunnelingDomainService.GetList();
		_domainWindow.ShowDialog();
		IEnumerable<string> list2 = _splitTunnelingDomainService.GetList();
		if (!list.SequenceEqual(list2))
		{
			IsNeededToShowPopUp = true;
		}
	}

	private void AppClickCommandExecute()
	{
		IEnumerable<string> list = _splitTunnelingAppService.GetList();
		_appWindow.ShowDialog();
		IEnumerable<string> list2 = _splitTunnelingAppService.GetList();
		if (!list.SequenceEqual(list2))
		{
			IsNeededToShowPopUp = true;
		}
	}

	private void CloseButtonClickCommandExecute()
	{
		SplitTunnelingMainWindowViewModel.ParentWindowVisibility = Visibility.Hidden;
	}
}
