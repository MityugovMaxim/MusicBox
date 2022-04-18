using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class SoundSource : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<SoundSource> { }

	[SerializeField] AudioSource m_AudioSource;

	CancellationTokenSource m_TokenSource;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Stop();
	}

	public async Task Play(AudioClip _Sound, float _Pitch, float _Volume)
	{
		if (_Sound == null)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_AudioSource.pitch  = _Pitch;
		m_AudioSource.volume = _Volume;
		m_AudioSource.PlayOneShot(_Sound);
		
		try
		{
			await UnityTask.Delay(_Sound.length / _Pitch + 0.5f, token);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public void Stop()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_AudioSource.Stop();
	}

	public void StartSound(AudioClip _Sound)
	{
		Stop();
		
		m_AudioSource.loop = true;
		m_AudioSource.clip = _Sound;
		m_AudioSource.Play();
	}

	public void StopSound()
	{
		Stop();
		
		m_AudioSource.Stop();
		m_AudioSource.loop = false;
		m_AudioSource.clip = null;
	}
}