namespace VpnSDK.TrafficOptimizer.DTO;

internal class TrafficStatistics
{
	public SpeedStatistics Download = new SpeedStatistics();

	public SpeedStatistics Upload = new SpeedStatistics();

	public void Store(ulong outSpeed, ulong inSpeed)
	{
		Calculate(ref Upload, outSpeed);
		Calculate(ref Download, inSpeed);
	}

	public void Clear()
	{
		Download = new SpeedStatistics();
		Upload = new SpeedStatistics();
	}

	private void Calculate(ref SpeedStatistics speed, ulong currentSpeed)
	{
		if (currentSpeed > 131072)
		{
			if (speed.Maximum == 0L || speed.MaxAverage == 0L)
			{
				speed.Maximum = currentSpeed / 2;
				speed.MaxAverage = speed.Maximum;
			}
			else if (currentSpeed > speed.MaxAverage)
			{
				if (currentSpeed > speed.Maximum)
				{
					speed.Maximum = currentSpeed;
				}
				speed.MaxAverage = (speed.MaxAverage + currentSpeed) / 2;
			}
		}
		speed.Current = currentSpeed;
	}
}
