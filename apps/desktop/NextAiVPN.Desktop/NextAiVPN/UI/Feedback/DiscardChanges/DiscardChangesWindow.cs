using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.Feedback.DiscardChanges;

public partial class DiscardChangesWindow : Window, IComponentConnector
{
	public DiscardChangesWindow()
	{
		InitializeComponent();
		base.DataContext = new DiscardChangesWindowViewModel(this);
	}
}
