using System;
using System.Windows;
using System.Windows.Controls;

namespace NextAiVPN;

/// <summary>
/// Residential Mesh Gateway and KikiLogin Hub UserControl.
/// Giao diện quản lý Cổng kết nối Residential Mesh và điều hướng Antidetect Browser (KikiLogin).
/// </summary>
public partial class ResidentialMeshControl : UserControl
{
	public ResidentialMeshControl()
	{
		InitializeComponent();
	}

	private void BtnCopyNode1_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Clipboard.SetText("127.0.0.1:10000:node-win_c0a8019b:nextai123");
			MessageBox.Show("Copied KikiLogin Proxy Syntax:\n127.0.0.1:10000:node-win_c0a8019b:nextai123", "NextAiVPN Mesh Hub", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		catch { }
	}

	private void BtnCopyNode2_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Clipboard.SetText("127.0.0.1:10000:node-win_89fe121a:nextai123");
			MessageBox.Show("Copied KikiLogin Proxy Syntax:\n127.0.0.1:10000:node-win_89fe121a:nextai123", "NextAiVPN Mesh Hub", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		catch { }
	}

	private void BtnCopyNode3_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Clipboard.SetText("127.0.0.1:10000:node-vn_hanoi_vnpt:nextai123");
			MessageBox.Show("Copied KikiLogin Proxy Syntax:\n127.0.0.1:10000:node-vn_hanoi_vnpt:nextai123", "NextAiVPN Mesh Hub", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		catch { }
	}
}
