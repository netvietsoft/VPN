using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.Streaming.InfoWindow;

public partial class StreamingInfoWindow : Window, IComponentConnector
{
	public StreamingInfoWindow(StreamingInfoWindowViewModel viewModel)
	{
		InitializeComponent();
		WindowHeader.GetParent(this);
		base.Closing += viewModel.OnClosing;
		base.DataContext = viewModel;
	}
}
