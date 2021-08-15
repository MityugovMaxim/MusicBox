using System;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ConfigProcessor : IInitializable, IDisposable
{
	const string ENVIRONMENT_ID = "production";

	public struct UserAttributes { }

	public struct AppAttributes { }

	public string PromoProductID { get; private set; }

	SignalBus m_SignalBus;

	public ConfigProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		ConfigManager.FetchCompleted += FetchCompleted;
		
		ConfigManager.SetCustomUserID(SystemInfo.deviceUniqueIdentifier);
		
		ConfigManager.SetEnvironmentID(ENVIRONMENT_ID);
		
		ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
	}

	void IDisposable.Dispose() { }

	void FetchCompleted(ConfigResponse _Response)
	{
		if (_Response.status == ConfigRequestStatus.Failed)
		{
			Debug.LogError("[ConfigProcessor] Fetch failed.");
			return;
		}
		
		ConfigOrigin configOrigin = _Response.requestOrigin;
		
		switch (configOrigin)
		{
			case ConfigOrigin.Default:
				Debug.Log("[ConfigProcessor] Config is empty. Using default values.");
				PromoProductID = string.Empty;
				return;
			case ConfigOrigin.Cached:
				Debug.Log("[ConfigProcessor] Config is empty. Using cached values.");
				PromoProductID = string.Empty;
				return;
			case ConfigOrigin.Remote:
				Debug.Log("[ConfigProcessor] Config loaded.");
				PromoProductID = ConfigManager.appConfig.GetString("promo_product_id");
				break;
		}
		
		m_SignalBus.Fire(new ConfigSignal());
	}
}