using UnityEngine;
using UnityEngine.Audio;

public class AudioProcessor : MonoBehaviour
{
	[SerializeField] AudioMixerSnapshot m_HitSnapshot;
	[SerializeField] AudioMixerSnapshot m_MissSnapshot;

	public void Restore()
	{
		m_HitSnapshot.TransitionTo(0);
	}

	public void RegisterHit()
	{
		m_HitSnapshot.TransitionTo(0.1f);
	}

	public void RegisterMiss()
	{
		m_MissSnapshot.TransitionTo(0.1f);
	}
}
