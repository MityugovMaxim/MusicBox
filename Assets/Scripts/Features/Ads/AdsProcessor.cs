using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AdsProcessor : DataCollection<AdsProviderSnapshot>
{
	protected override string Path => "ads_providers";

	[Inject] IAdsProvider[]  m_AdsProviders;
	[Inject] AudioManager    m_AudioManager;

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
	{ }
}
