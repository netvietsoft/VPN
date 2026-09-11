using System.Threading.Tasks;
using System.Windows;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.UI.MessageBoxWindows;

namespace NextAiVPN.Services.Persistence;

internal class NextAiTechnologySubscriptionProlongationHandler : INextAiTechnologySubscriptionProlongationHandler
{
	private readonly ISubscriptionInfo _subscriptionInfo;

	private readonly IAppLogger _logger;

	private readonly IBrowserLinksOpener _browserLinksOpener;

	public NextAiTechnologySubscriptionProlongationHandler(ISubscriptionInfo subscriptionInfo, IAppLogger logger, IBrowserLinksOpener browserLinksOpener)
	{
		_subscriptionInfo = subscriptionInfo;
		_logger = logger;
		_browserLinksOpener = browserLinksOpener;
	}

	public async Task Prolongate()
	{
		NextAiTechnologyProlongationResponse nextaitechnologyProlongationResponse = await _subscriptionInfo.NextAiTechnologyProlongationActionAsync();
		if (nextaitechnologyProlongationResponse != null)
		{
			if (!string.IsNullOrEmpty(nextaitechnologyProlongationResponse.Message))
			{
				_logger?.Error(nextaitechnologyProlongationResponse.Message, "Prolongate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiTechnologySubscriptionProlongationHandler.cs", 50);
				MessageBoxWindowViewModel messageBoxWindowViewModel = new MessageBoxWindowViewModel();
				messageBoxWindowViewModel.Header = "Error";
				messageBoxWindowViewModel.Description = nextaitechnologyProlongationResponse.Message;
				messageBoxWindowViewModel.OkButtonText = "Ok";
				messageBoxWindowViewModel.CancelButtonVisibility = Visibility.Collapsed;
				new MessageBoxWindow(messageBoxWindowViewModel).ShowDialog();
			}
			else if (!string.IsNullOrEmpty(nextaitechnologyProlongationResponse.Url))
			{
				_browserLinksOpener.OpenBrowserLink(nextaitechnologyProlongationResponse.Url);
			}
		}
	}
}
