using System.Linq;
using DotRas;

namespace VpnSDK.Private.Ras.Extensions;

internal static class RasPhonebookExtensions
{
	internal static bool TryFindEntry(this RasPhoneBook phonebook, string entryName, out RasEntry entry)
	{
		entry = phonebook.Entries.FirstOrDefault((RasEntry x) => x.Name == entryName);
		return entry != null;
	}
}
