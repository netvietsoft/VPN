using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class FeedbackEmojis : UserControl, IComponentConnector
{
	private VPNWindowExpanded _expandedWindow;

	public FeedbackEmojis()
	{
		InitializeComponent();
	}

	public void GetExpandedWindow(VPNWindowExpanded expanded)
	{
		_expandedWindow = expanded;
	}

	private void Smile_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Mah_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Mad_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void BtnCross_MouseDown(object sender, MouseButtonEventArgs e)
	{
		Utils.FeedbackService.SaveLastFeedbackClosed(reset: false);
	}
}
