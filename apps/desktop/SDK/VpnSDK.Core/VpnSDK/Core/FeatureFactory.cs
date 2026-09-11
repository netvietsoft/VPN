using System;
using System.Collections.Generic;
using System.Linq;
using VpnSDK.Core.Interfaces;

namespace VpnSDK.Core;

internal class FeatureFactory
{
	private readonly List<IFeature> _features;

	private static readonly Lazy<FeatureFactory> Instance = new Lazy<FeatureFactory>(() => new FeatureFactory());

	private static readonly object ConcurrentLock = new object();

	private FeatureFactory()
	{
		_features = new List<IFeature>();
	}

	internal static T GetFeature<T>() where T : class, IFeature
	{
		T val = null;
		lock (ConcurrentLock)
		{
			return Instance.Value._features?.FirstOrDefault((IFeature feature) => feature.GetType() == typeof(T)) as T;
		}
	}

	internal static bool? IsRegistered(Type type)
	{
		bool? flag = null;
		lock (ConcurrentLock)
		{
			return Instance.Value._features?.Any((IFeature feature) => feature.GetType() == type);
		}
	}

	internal static void Register(IFeature feature)
	{
		if (feature == null)
		{
			throw new ArgumentNullException("feature");
		}
		if (IsRegistered(feature.GetType()) == true)
		{
			throw new ArgumentException($"Type {feature.GetType()} already registered");
		}
		lock (ConcurrentLock)
		{
			Instance.Value._features?.Add(feature);
		}
	}

	internal static void Unregister(IFeature feature)
	{
		if (feature == null)
		{
			throw new ArgumentNullException("feature");
		}
		lock (ConcurrentLock)
		{
			Instance.Value._features?.Remove(feature);
		}
	}
}
