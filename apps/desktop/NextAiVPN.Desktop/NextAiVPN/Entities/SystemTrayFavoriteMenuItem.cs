using System.Windows.Controls;

namespace NextAiVPN.Entities;

internal class SystemTrayFavoriteMenuItem
{
	public string ConnectionString { get; set; }

	public string Header { get; set; }

	public string Country { get; set; }

	public string City { get; set; }

	public MenuItem MenuItem { get; set; }
}
