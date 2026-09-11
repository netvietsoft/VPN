using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NextAiVPN.Services.Persistence;

internal static class UrlFinder
{
	private const string Pattern = "((http|ftp|https):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?)";

	private const string Separator = "♥";

	public static MatchCollection FindUrl(string searchingString)
	{
		return Regex.Matches(searchingString, "((http|ftp|https):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?)");
	}

	public static List<string> FindUrlList(string searchingString)
	{
		MatchCollection matchCollection = Regex.Matches(searchingString, "((http|ftp|https):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?)");
		string text = new Regex("((http|ftp|https):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?)").Replace(searchingString, "♥");
		int num = 0;
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i].ToString().Equals("♥"))
			{
				if (i == 0)
				{
					list.Add(matchCollection[num].Value);
				}
				else
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
					list.Add(matchCollection[num].Value);
				}
				num++;
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		return list;
	}
}
