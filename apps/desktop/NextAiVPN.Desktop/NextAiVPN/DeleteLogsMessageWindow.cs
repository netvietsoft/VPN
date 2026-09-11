using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class DeleteLogsMessageWindow : Window, IComponentConnector
{
	public bool Action { get; set; }

	public DeleteLogsMessageWindow()
	{
		InitializeComponent();
		reset.Click += Reset_Click;
		cancel.Click += Cancel_Click;
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		SetAction(value: false);
	}

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		SetAction(value: true);
	}

	private void SetAction(bool value)
	{
		Action = value;
		Close();
	}

	private void BtnCross_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		SetAction(value: false);
	}
}
