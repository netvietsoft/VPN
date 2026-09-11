using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN.UI.Settings;

public partial class ExpandedGeneralSettingsControl : UserControl, IComponentConnector
{
	public ExpandedGeneralSettingsControl()
	{
		InitializeComponent();
	}

	private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer obj = (ScrollViewer)sender;
		obj.ScrollToVerticalOffset(obj.VerticalOffset - (double)e.Delta);
		e.Handled = true;
	}
}
