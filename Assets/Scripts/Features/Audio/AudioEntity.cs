using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class AudioEntity : MonoBehaviour
{
	[Preserve]
	public class Pool : MonoMemoryPool<AudioClip, AudioEntity>
	{
		protected override void Reinitialize(AudioClip _Clip, AudioEntity _Item)
		{
			_Item.Setup(_Clip);
		}

		protected override void OnDespawned(AudioEntity _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Setup(null);
		}
	}

	public int Time
	{
		get => (int)((double)m_AudioSource.timeSamples / m_AudioSource.clip.frequency * 1000);
		set => m_AudioSource.timeSamples = value / 1000 * m_AudioSource.clip.frequency;
	}

	public bool Loop
	{
		get => m_AudioSource.loop;
		set => m_AudioSource.loop = value;
	}

	int Length   => (int)((double)m_AudioSource.clip.samples / m_AudioSource.clip.frequency * 1000);
	int Duration => Length - Time;

	string ID => GetInstanceID().ToString();

	bool Pause { get; set; }

	[SerializeField] AudioSource m_AudioSource;
	[SerializeField] float       m_FadeInDuration  = 0.5f;
	[SerializeField] float       m_FadeOutDuration = 0.5f;

	IEnumerator m_VolumeRoutine;

	void Setup(AudioClip _Clip)
	{
		name  = _Clip != null ? _Clip.name : "audio_entity";
		Pause = false;
		m_AudioSource.Stop();
		m_AudioSource.clip = _Clip;
	}

	public async Task PlayAsync(CancellationToken _Token = default)
	{
		TokenProvider.CancelToken(this, ID);
		
		CancellationToken token = TokenProvider.CreateToken(this, ID, _Token);
		
		await VolumeAsync(0, m_FadeOutDuration, true, token);
		
		if (!m_AudioSource.isPlaying)
		{
			if (Pause)
				m_AudioSource.UnPause();
			else
				m_AudioSource.Play();
		}
		
		Pause = false;
		
		await VolumeAsync(1, m_FadeInDuration, false, token);
		
		if (Mathf.Approximately(Duration, 0))
			return;
		
		while (Loop || Duration > 0)
			await Task.Delay(Duration, token);
		
		TokenProvider.RemoveToken(this, ID);
	}

	public async Task PauseAsync(CancellationToken _Token = default)
	{
		TokenProvider.CancelToken(this, ID);
		
		CancellationToken token = TokenProvider.CreateToken(this, ID, _Token);
		
		await VolumeAsync(0, m_FadeOutDuration, false, token);
		
		m_AudioSource.Pause();
		
		Pause = true;
		
		TokenProvider.RemoveToken(this, ID);
	}

	public async Task StopAsync(CancellationToken _Token = default)
	{
		TokenProvider.CancelToken(this, ID);
		
		CancellationToken token = TokenProvider.CreateToken(this, ID, _Token);
		
		await VolumeAsync(0, m_FadeOutDuration, false, token);
		
		m_AudioSource.Stop();
		
		Pause = false;
		
		TokenProvider.RemoveToken(this, ID);
	}

	Task VolumeAsync(float _Volume, float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		void Cancel()
		{
			if (m_VolumeRoutine == null)
				return;
			StopCoroutine(m_VolumeRoutine);
			m_VolumeRoutine = null;
		}
		
		Cancel();
		
		if (gameObject.activeInHierarchy && !_Instant)
		{
			TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
			
			_Token.Register(Cancel);
			
			m_VolumeRoutine = VolumeRoutine(_Volume, _Duration, () => task.TrySetResult(true));
			
			StartCoroutine(m_VolumeRoutine);
			
			return task.Task;
		}
		
		m_AudioSource.volume = _Volume;
		
		return Task.CompletedTask;
	}

	IEnumerator VolumeRoutine(float _Volume, float _Duration, Action _Finished)
	{
		float source = m_AudioSource.volume;
		float target = Mathf.Clamp01(_Volume);
		
		if (!Mathf.Approximately(source, target) && _Duration > float.Epsilon)
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += UnityEngine.Time.deltaTime;
				
				m_AudioSource.volume = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		m_AudioSource.volume = target;
		
		_Finished?.Invoke();
	}
}
