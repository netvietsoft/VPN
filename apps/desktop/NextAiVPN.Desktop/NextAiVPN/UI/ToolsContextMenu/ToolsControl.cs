using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.ToolsContextMenu;

public partial class ToolsControl : UserControl, IComponentConnector
{
	private IStyleService _styleService;

	public ToolsControl()
	{
		InitializeComponent();
		GlobalEvents.StyleChanged += StyleChanged;
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
	}

	public void SetDependencies(IStyleService styleService)
	{
		_styleService = styleService;
		_styleService.SetContextMenuStyle(StyleModeDefiner.DefineAppStyle(), IconContextMenu);
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		_styleService.SetContextMenuStyle(arg1, IconContextMenu);
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += StyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= StyleChanged;
	}

	private void Tools_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		EmulateRightClick();
	}

	private void EmulateRightClick()
	{
		IconContextMenu.PlacementTarget = Tools;
		IconContextMenu.Placement = PlacementMode.MousePoint;
		IconContextMenu.IsOpen = true;
	}
}
