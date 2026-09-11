using System.Reflection;
using System.Windows.Controls;

namespace NextAiVPN;

public static class RemoveJSErrors
{
	public static void SuppressScriptErrors(this WebBrowser webBrowser, bool hide)
	{
		FieldInfo field = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
		if (!(field == null))
		{
			object value = field.GetValue(webBrowser);
			value?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, value, new object[1] { hide });
		}
	}
}
