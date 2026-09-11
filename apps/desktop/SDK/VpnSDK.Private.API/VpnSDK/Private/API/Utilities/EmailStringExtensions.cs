namespace VpnSDK.Private.API.Utilities;

internal static class EmailStringExtensions
{
	public static bool IsEmail(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return false;
		}
		string[] array = input.Split('@');
		if (array.Length == 2 && array[1].Contains("."))
		{
			return true;
		}
		return false;
	}
}
