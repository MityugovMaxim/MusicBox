using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AdsProcessor : DataCollection<AdsProviderSnapshot>
{
	protected override string Path => "ads_providers";

	[Inject] IAdsProvider[]  m_AdsProviders;
	[Inject] AudioManager    m_AudioManager;
	[Inject] ConfigProcessor m_ConfigProcessor;

	float m_Time;

	async void InitializeAdsProviders()
	{
		List<Task<bool>> tasks = new List<Task<bool>>();
		
		List<string> adsProviderIDs = GetAdsProviderIDs();
		
		foreach (string adsProviderID in adsProviderIDs)
		{
			IAdsProvider adsProvider = GetAdsProvider(adsProviderID);
			
			if (adsProvider == null)
				continue;
			
			string interstitialID = GetInterstitialID(adsProviderID);
			string rewardedID     = GetRewardedID(adsProviderID);
			
			tasks.Add(adsProvider.Initialize(interstitialID, rewardedID));
		}
		
		await Task.WhenAll(tasks);
		
		Log.Info(this, "Ads providers initialized.");
	}

	List<string> GetAdsProviderIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	string GetInterstitialID(string _AdsProviderID)
	{
		AdsProviderSnapshot snapshot = GetSnapshot(_AdsProviderID);
		
		#if UNITY_IOS
		return snapshot?.iOSInterstitialID ?? string.Empty;
		#elif UNITY_ANDROID
		return snapshot?.AndroidInterstitialID ?? string.Empty;
		#else
		return string.Empty;
		#endif
	}

	string GetRewardedID(string _AdsProviderID)
	{
		AdsProviderSnapshot snapshot = GetSnapshot(_AdsProviderID);
		
		#if UNITY_IOS
		return snapshot?.iOSRewardedID ?? string.Empty;
		#elif UNITY_ANDROID
		return snapshot?.AndroidRewardedID ?? string.Empty;
		#else
		return string.Empty;
		#endif
	}

	IAdsProvider GetAdsProvider(string _AdsProviderID)
	{
		if (string.IsNullOrEmpty(_AdsProviderID))
			return null;
		
		return m_AdsProviders.FirstOrDefault(_AdsProvider => _AdsProvider.ID == _AdsProviderID);
	}

	public bool CheckAvailable() => Time.realtimeSinceStartup >= m_Time;

	public bool CheckUnavailable() => Time.realtimeSinceStartup < m_Time;

	public async Task<bool> Interstitial(string _Place)
	{
		if (CheckUnavailable())
			return true;
		
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Interstitial(_Place))
				continue;
			
			ProcessCooldown();
			
			AudioListener.volume = 1;
			AudioListener.pause  = false;
			
			m_AudioManager.SetAudioActive(true);
			
			return true;
		}
		
		AudioListener.volume = 1;
		AudioListener.pause  = false;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	public async Task<bool> Rewarded(string _Place)
	{
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Rewarded(_Place))
				continue;
			
			ProcessCooldown();
			
			AudioListener.volume = 1;
			AudioListener.pause  = false;
			
			m_AudioManager.SetAudioActive(true);
			
			return true;
		}
		
		AudioListener.volume = 1;
		AudioListener.pause  = false;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	void ProcessCooldown()
	{
		m_Time = Time.realtimeSinceStartup + m_ConfigProcessor.AdsCooldown;
	}
}
