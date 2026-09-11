using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.Trial;

public partial class TrialFirstWindow : Window, IComponentConnector
{
	public TrialFirstWindow(TrialFirstWindowViewModel trialFirstWindowViewModel)
	{
		TrialFirstWindow trialFirstWindow = this;
		InitializeComponent();
		WindowHeader.GetParent(this);
		base.DataContext = trialFirstWindowViewModel;
		trialFirstWindowViewModel.RequestClose += OnRequestClose;
		base.Closing += delegate(object? s, CancelEventArgs e)
		{
			e.Cancel = true;
			trialFirstWindow.Hide();
		};
		base.Closed += delegate
		{
			trialFirstWindowViewModel.RequestClose -= trialFirstWindow.OnRequestClose;
		};
	}

	private void OnRequestClose()
	{
		Hide();
	}
}
