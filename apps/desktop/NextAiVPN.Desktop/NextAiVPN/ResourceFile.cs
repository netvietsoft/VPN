using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace NextAiVPN;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class ResourceFile
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				string resName = "NextAiVPN.ResourceFile";
				var asm = typeof(ResourceFile).Assembly;
				var existing = System.Linq.Enumerable.FirstOrDefault(asm.GetManifestResourceNames(), n => n.EndsWith("ResourceFile.resources"));
				if (existing != null)
				{
					resName = existing.Substring(0, existing.Length - ".resources".Length);
				}
				resourceMan = new ResourceManager(resName, asm);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static Icon addSystemTray => (Icon)ResourceManager.GetObject("addSystemTray", resourceCulture);

	public static Icon DarkModeAdd => (Icon)ResourceManager.GetObject("DarkModeAdd", resourceCulture);

	public static Icon DarkModeDefault => (Icon)ResourceManager.GetObject("DarkModeDefault", resourceCulture);

	public static Icon DarkModeError => (Icon)ResourceManager.GetObject("DarkModeError", resourceCulture);

	public static Icon DarkModeFavorite => (Icon)ResourceManager.GetObject("DarkModeFavorite", resourceCulture);

	public static Icon DarkModeServerLocation => (Icon)ResourceManager.GetObject("DarkModeServerLocation", resourceCulture);

	public static Icon DarkModeSuccess => (Icon)ResourceManager.GetObject("DarkModeSuccess", resourceCulture);

	public static Icon DarkModeWarning => (Icon)ResourceManager.GetObject("DarkModeWarning", resourceCulture);

	public static Icon Favorite => (Icon)ResourceManager.GetObject("Favorite", resourceCulture);

	public static Icon icontray_connected => (Icon)ResourceManager.GetObject("icontray_connected", resourceCulture);

	public static Icon icontray_error => (Icon)ResourceManager.GetObject("icontray_error", resourceCulture);

	public static Icon icontray_nonetwork => (Icon)ResourceManager.GetObject("icontray_nonetwork", resourceCulture);

	public static Icon icontray_regular => (Icon)ResourceManager.GetObject("icontray_regular", resourceCulture);

	public static Icon icontray_warning => (Icon)ResourceManager.GetObject("icontray_warning", resourceCulture);

	public static Icon LightModeServerLocation => (Icon)ResourceManager.GetObject("LightModeServerLocation", resourceCulture);

	public static Icon NotificationsBlue => (Icon)ResourceManager.GetObject("NotificationsBlue", resourceCulture);

	public static Icon NotificationsOrangeDarkMode => (Icon)ResourceManager.GetObject("NotificationsOrangeDarkMode", resourceCulture);

	public static Icon NotificationsOrangeLightMode => (Icon)ResourceManager.GetObject("NotificationsOrangeLightMode", resourceCulture);

	public static Icon NotificationsRed => (Icon)ResourceManager.GetObject("NotificationsRed", resourceCulture);

	public static Icon start_darkmode => (Icon)ResourceManager.GetObject("start_darkmode", resourceCulture);

	public static Icon start_lightmode => (Icon)ResourceManager.GetObject("start_lightmode", resourceCulture);

	internal ResourceFile()
	{
	}
}
