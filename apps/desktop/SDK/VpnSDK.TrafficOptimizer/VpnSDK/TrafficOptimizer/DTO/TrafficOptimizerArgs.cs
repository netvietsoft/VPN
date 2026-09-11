using System;

namespace VpnSDK.TrafficOptimizer.DTO;

public class TrafficOptimizerArgs : EventArgs
{
	public TrafficSpeedData Download { get; set; }

	public TrafficSpeedData Upload { get; set; }

	public TrafficSpeedData PrioritizedAppDownload { get; set; }

	public TrafficSpeedData PrioritizedAppUpload { get; set; }
}
