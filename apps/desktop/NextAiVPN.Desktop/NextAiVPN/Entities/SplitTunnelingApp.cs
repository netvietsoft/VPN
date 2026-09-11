using System.IO;
using NextAiVPN.UI;

namespace NextAiVPN.Entities;

public class SplitTunnelingApp : ViewModelBase
{
	public string Name { get; set; }

	public string Path { get; set; }

	public SplitTunnelingApp Parse(string path)
	{
		Path = path;
		Name = System.IO.Path.GetFileName(path);
		return this;
	}
}
