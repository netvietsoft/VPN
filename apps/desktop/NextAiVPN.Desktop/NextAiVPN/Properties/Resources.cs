using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace NextAiVPN.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
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
				string resName = "NextAiVPN.Properties.Resources";
				var asm = typeof(Resources).Assembly;
				var existing = System.Linq.Enumerable.FirstOrDefault(asm.GetManifestResourceNames(), n => n.EndsWith("Resources.resources") && !n.EndsWith("ResourceFile.resources") && !n.EndsWith("g.resources"));
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

	public static string DiagnosticFilePath => ResourceManager.GetString("DiagnosticFilePath", resourceCulture);

	public static string DiagnosticsFileName => ResourceManager.GetString("DiagnosticsFileName", resourceCulture);

	public static string DiagnosticTempFilePath => ResourceManager.GetString("DiagnosticTempFilePath", resourceCulture);

	public static string IconFolderPathDarkMode => ResourceManager.GetString("IconFolderPathDarkMode", resourceCulture);

	public static string IconFolderPathLightMode => ResourceManager.GetString("IconFolderPathLightMode", resourceCulture);

	public static string IncludeDialogCrashReportFileText => ResourceManager.GetString("IncludeDialogCrashReportFileText", resourceCulture);

	public static string IncludeDialogFileText => ResourceManager.GetString("IncludeDialogFileText", resourceCulture);

	internal Resources()
	{
	}
}
