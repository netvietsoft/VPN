using System;
using System.Windows;
using System.Windows.Media;

namespace NextAiVPN.Entities;

public class Notification
{
	public string Header { get; set; }

	public string Id { get; set; }

	public string Title { get; set; }

	public string Description { get; set; }

	public DateTime Date { get; set; }

	public Visibility UpdateCTAVisibility { get; set; } = Visibility.Collapsed;

	public Visibility GoToAccountCTAVisibility { get; set; } = Visibility.Collapsed;

	public Visibility DotsMenuVisibility { get; set; } = Visibility.Collapsed;

	public SolidColorBrush ItemBackground { get; set; }

	public FontFamily TitleFont { get; set; }

	public Brush TitleFontColor { get; set; }

	public bool IsRead { get; set; }

	public string RowHeight { get; set; } = "auto";

	public string ImageSource { get; set; }

	public string Type { get; set; }
}
