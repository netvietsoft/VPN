using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.TrafficOptimizer;

public partial class TrafficOptimizerWindow : Window, IComponentConnector
{
	public TrafficOptimizerWindow(TrafficOptimizerWindowViewModel trafficOptimizerWindowViewModel)
	{
		base.DataContext = trafficOptimizerWindowViewModel;
		base.Closing += trafficOptimizerWindowViewModel.OnWindowClosing;
		InitializeComponent();
	}
}
