using System.Collections.Generic;
using System.Linq;
using DynamicData;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class TrustedNetworkService : ITrustedNetworkService
{
	private readonly ITrustedNetworkRepository _trustedNetworkRepository;

	private IList<string> _trustedNetworks;

	public TrustedNetworkService(ITrustedNetworkRepository networkRepository)
	{
		_trustedNetworkRepository = networkRepository;
		_trustedNetworks = new List<string>();
	}

	public IEnumerable<string> GetNetworks()
	{
		_trustedNetworks = _trustedNetworkRepository.GetNetworks().ToList();
		return _trustedNetworks;
	}

	public bool Contain(string network)
	{
		return _trustedNetworks.Contains(network);
	}

	public void Add(string network)
	{
		_trustedNetworks.Add(network);
		Save();
	}

	public void AddRange(IList<string> networks)
	{
		_trustedNetworks.AddRange(networks);
		Save();
	}

	public void SetNetworks(IList<string> networks)
	{
		_trustedNetworks = networks;
		Save();
	}

	public void Remove(string network)
	{
		if (Contain(network))
		{
			_trustedNetworks.Remove(network);
			Save();
		}
	}

	public void RemoveAll()
	{
		_trustedNetworks.Clear();
		Save();
	}

	private void Save()
	{
		_trustedNetworkRepository.SetNetworks(_trustedNetworks);
	}
}
