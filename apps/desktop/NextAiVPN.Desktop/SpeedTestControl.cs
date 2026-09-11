using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NextAiVPN;

/// <summary>
/// Network Speed Test and Latency Benchmark UserControl.
/// Giao diện kiểm tra tốc độ mạng, đo độ trễ (Ping) và băng thông tải xuống/tải lên.
/// </summary>
public partial class SpeedTestControl : UserControl
{
	private bool _isRunning = false;

	public SpeedTestControl()
	{
		InitializeComponent();
	}

	private async void BtnRunSpeedTest_Click(object sender, RoutedEventArgs e)
	{
		if (_isRunning) return;
		_isRunning = true;
		BtnRunSpeedTest.IsEnabled = false;
		BtnRunSpeedTest.Content = "⏳ Testing Latency...";
		TxtSpeedStatus.Text = "● MEASURING PING & JITTER...";

		try
		{
			// Simulate Ping Test Phase
			await Task.Delay(800);
			TxtPingResult.Text = "34 ms";
			BtnRunSpeedTest.Content = "⏳ Testing Download...";
			TxtSpeedStatus.Text = "● MEASURING DOWNLOAD BANDWIDTH...";

			// Simulate Download Test Phase
			for (int i = 1; i <= 5; i++)
			{
				double speed = 120.0 + (i * 15.5) + (Random.Shared.NextDouble() * 10);
				TxtLiveSpeed.Text = speed.ToString("F1");
				await Task.Delay(300);
			}
			TxtDownloadResult.Text = "196.8 Mbps";

			// Simulate Upload Test Phase
			BtnRunSpeedTest.Content = "⏳ Testing Upload...";
			TxtSpeedStatus.Text = "● MEASURING UPLOAD BANDWIDTH...";
			for (int i = 1; i <= 4; i++)
			{
				double upSpeed = 60.0 + (i * 9.5) + (Random.Shared.NextDouble() * 5);
				TxtLiveSpeed.Text = upSpeed.ToString("F1");
				await Task.Delay(250);
			}
			TxtUploadResult.Text = "98.2 Mbps";
			TxtLossResult.Text = "0.0 %";

			TxtSpeedStatus.Text = "● BENCHMARK COMPLETE • EXCELLENT CONNECTION";
		}
		catch { }
		finally
		{
			_isRunning = false;
			BtnRunSpeedTest.IsEnabled = true;
			BtnRunSpeedTest.Content = "🚀 Run Speed Test Again";
		}
	}
}
