using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.Errors.ConnectionError;

public partial class ConnectionErrorControl : UserControl, IComponentConnector
{
	public ConnectionErrorControlViewModel ViewModel { get; set; }

	public ConnectionErrorControl()
	{
		InitializeComponent();
	}
}
