using System;

namespace NextAiVPN.Services.Persistence;

public static class DateFormatter
{
	public static string GetRelativeDateText(DateTime date)
	{
		DateTime today = DateTime.Today;
		DateTime date2 = date.Date;
		return (date2 - today).Days switch
		{
			0 => "today", 
			1 => "tomorrow", 
			_ => $"{date2:d}", 
		};
	}
}
