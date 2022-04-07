using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class SoundSource : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<SoundSource> { }

	[SerializeField] AudioSource m_AudioSource;

	public async Task Play(AudioClip _Sound)
	{
		m_AudioSource.PlayOneShot(_Sound);
		
		await UnityTask.Delay(_Sound.length + 0.5f);
	}

	public void StartSound(AudioClip _Sound)
	{
		m_AudioSource.loop = true;
		m_AudioSource.clip = _Sound;
		m_AudioSource.Play();
	}

	public void StopSound()
	{
		m_AudioSource.Stop();
		m_AudioSource.loop = false;
		m_AudioSource.clip = null;
	}
}