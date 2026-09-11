using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace NextAiVPN.Streaming;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resource
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("NextAiVPN.Streaming.Resource", typeof(Resource).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
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

	internal static byte[] letsencrypt_stg_int_r12 => (byte[])ResourceManager.GetObject("letsencrypt_stg_int_r12", resourceCulture);

	internal static byte[] letsencrypt_stg_int_r13 => (byte[])ResourceManager.GetObject("letsencrypt_stg_int_r13", resourceCulture);

	internal static byte[] letsencrypt_stg_root_x1 => (byte[])ResourceManager.GetObject("letsencrypt_stg_root_x1", resourceCulture);

	internal static byte[] yonder_yam_root_yr => (byte[])ResourceManager.GetObject("yonder_yam_root_yr", resourceCulture);

	internal Resource()
	{
	}
}
