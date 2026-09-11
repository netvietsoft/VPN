using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.Streaming.FavoriteLocations;

public partial class FavoriteStreamingLocationsControl : UserControl, IComponentConnector
{
	public FavoriteStreamingLocationsControlViewModel ViewModel { get; set; }

	public FavoriteStreamingLocationsControl()
	{
		InitializeComponent();
	}
}
