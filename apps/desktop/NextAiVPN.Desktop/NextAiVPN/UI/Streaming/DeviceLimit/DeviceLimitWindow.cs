using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.Streaming.DeviceLimit;

public partial class DeviceLimitWindow : Window, IComponentConnector
{
	public DeviceLimitWindow(DeviceLimitWindowViewModel deviceLimitWindowViewModel)
	{
		base.DataContext = deviceLimitWindowViewModel;
		InitializeComponent();
	}
}
