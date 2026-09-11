using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VpnSDK.Private.API.Wireguard.DTO;

public class Interface
{
	[ConfigurationEntry]
	public string[] DNS { get; set; }

	[ConfigurationEntry]
	public string Address { get; set; }

	[ConfigurationEntry]
	public string PrivateKey { get; set; }

	[ConfigurationEntry]
	public int? MTU { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Interface]" + Environment.NewLine);
		foreach (PropertyInfo item in from x in typeof(Interface).GetProperties()
			where x.GetCustomAttributes(typeof(ConfigurationEntryAttribute)).Any()
			select x)
		{
			string text = "";
			if (item.PropertyType.IsArray)
			{
				foreach (object item2 in (Array)item.GetValue(this))
				{
					text = text + item2.ToString() + ",";
				}
				text = text.TrimEnd(',');
			}
			else
			{
				object value = item.GetValue(this);
				if (value != null)
				{
					text = value.ToString();
				}
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				stringBuilder.AppendLine(item.Name + " = " + text);
			}
		}
		return stringBuilder.ToString();
	}
}
