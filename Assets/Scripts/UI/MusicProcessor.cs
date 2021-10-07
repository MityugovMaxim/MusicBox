using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MusicProcessor : MonoBehaviour
{
	[SerializeField] AudioSource m_MusicSource;
	[SerializeField] AudioSource m_AmbientSource;
	[SerializeField] AudioClip   m_Ambient;

	readonly Dictionary<AudioSource, IEnumerator> m_PlayRoutines  = new Dictionary<AudioSource, IEnumerator>();
	readonly Dictionary<AudioSource, IEnumerator> m_StopRoutines  = new Dictionary<AudioSource, IEnumerator>();
	readonly Dictionary<AudioSource, IEnumerator> m_PauseRoutines = new Dictionary<AudioSource, IEnumerator>();

	void Awake()
	{
		PlayAmbient();
	}

	public Task StopAmbient(CancellationToken _Token = default)
	{
		return Stop(m_AmbientSource, _Token);
	}

	public Task PlayAmbient(CancellationToken _Token = default)
	{
		return Play(m_AmbientSource, m_Ambient, 0.5f, _Token);
	}

	public Task PauseAmbient(CancellationToken _Token = default)
	{
		return Pause(m_AmbientSource, _Token);
	}

	public Task StopMusic(CancellationToken _Token = default)
	{
		return Stop(m_MusicSource, _Token);
	}

	public Task PlayMusic(AudioClip _AudioClip, CancellationToken _Token = default)
	{
		return Play(m_MusicSource, _AudioClip, 1, _Token);
	}

	public Task PauseMusic(CancellationToken _Token = default)
	{
		return Pause(m_MusicSource, _Token);
	}

	Task Stop(AudioSource _AudioSource, CancellationToken _Token = default)
	{
		StopAudioRoutines(_AudioSource);
		
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			taskSource.TrySetCanceled();
			return taskSource.Task;
		}
		
		_Token.Register(
			() =>
			{
				StopAudioRoutines(_AudioSource);
				
				taskSource.TrySetCanceled();
			}
		);
		
		IEnumerator routine = StopRoutine(_AudioSource, 0.4f, taskSource);
		
		m_StopRoutines[_AudioSource] = routine;
		
		StartCoroutine(routine);
		
		return taskSource.Task;
	}

	Task Play(AudioSource _AudioSource, AudioClip _AudioClip, float _Volume, CancellationToken _Token = default)
	{
		StopAudioRoutines(_AudioSource);
		
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			taskSource.TrySetCanceled();
			return taskSource.Task;
		}
		
		_Token.Register(
			() =>
			{
				StopAudioRoutines(_AudioSource);
				
				taskSource.TrySetCanceled();
			}
		);
		
		IEnumerator routine = PlayRoutine(_AudioSource, 0.4f, _AudioClip, _Volume, taskSource);
		
		m_PlayRoutines[_AudioSource] = routine;
		
		StartCoroutine(routine);
		
		return taskSource.Task;
	}

	Task Pause(AudioSource _AudioSource, CancellationToken _Token = default)
	{
		StopAudioRoutines(_AudioSource);
		
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			taskSource.TrySetCanceled();
			return taskSource.Task;
		}
		
		_Token.Register(
			() =>
			{
				StopAudioRoutines(_AudioSource);
				
				taskSource.TrySetCanceled();
			}
		);
		
		IEnumerator routine = PauseRoutine(_AudioSource, 0.4f, taskSource);
		
		m_PauseRoutines[_AudioSource] = routine;
		
		StartCoroutine(routine);
		
		return taskSource.Task;
	}

	void StopAudioRoutines(AudioSource _AudioSource)
	{
		if (m_StopRoutines.TryGetValue(_AudioSource, out IEnumerator stopRoutine))
			StopCoroutine(stopRoutine);
		m_StopRoutines.Remove(_AudioSource);
		
		if (m_PlayRoutines.TryGetValue(_AudioSource, out IEnumerator playRoutine))
			StopCoroutine(playRoutine);
		m_PlayRoutines.Remove(_AudioSource);
		
		if (m_PauseRoutines.TryGetValue(_AudioSource, out IEnumerator pauseRoutine))
			StopCoroutine(pauseRoutine);
		m_PauseRoutines.Remove(_AudioSource);
	}

	static IEnumerator StopRoutine(AudioSource _AudioSource, float _Duration, TaskCompletionSource<bool> _TaskSource)
	{
		if (_AudioSource == null)
		{
			_TaskSource?.SetException(new NullReferenceException("[MusicProcessor] AudioSource is null."));
			yield break;
		}
		
		yield return VolumeRoutine(_AudioSource, 0, _Duration);
		
		_AudioSource.Stop();
		_AudioSource.clip   = null;
		
		if (!_TaskSource.Task.IsCompleted)
			_TaskSource.TrySetResult(true);
	}

	static IEnumerator PlayRoutine(AudioSource _AudioSource, float _Duration, AudioClip _AudioClip, float _Volume, TaskCompletionSource<bool> _TaskSource)
	{
		if (_AudioSource == null)
		{
			_TaskSource?.SetException(new NullReferenceException("[MusicProcessor] AudioSource is null."));
			yield break;
		}
		
		_AudioSource.clip = _AudioClip;
		_AudioSource.Play();
		
		yield return VolumeRoutine(_AudioSource, _Volume, _Duration);
		
		if (!_TaskSource.Task.IsCompleted)
			_TaskSource.TrySetResult(true);
	}

	static IEnumerator PauseRoutine(AudioSource _AudioSource, float _Duration, TaskCompletionSource<bool> _TaskSource)
	{
		if (_AudioSource == null)
		{
			_TaskSource?.SetException(new NullReferenceException("[MusicProcessor] AudioSource is null."));
			yield break;
		}
		
		yield return VolumeRoutine(_AudioSource, 0, _Duration);
		
		_AudioSource.Pause();
		
		if (!_TaskSource.Task.IsCompleted)
			_TaskSource.SetResult(true);
	}

	static IEnumerator VolumeRoutine(AudioSource _AudioSource, float _Volume, float _Duration)
	{
		if (_AudioSource == null)
			yield break;
		
		float source = _AudioSource.volume;
		float target = Mathf.Clamp01(_Volume);
		
		if (!Mathf.Approximately(source, target))
		{
			float time     = 0;
			float duration = _Duration * Mathf.Abs(target - source);
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_AudioSource.volume = Mathf.Lerp(source, target, time / duration);
			}
		}
		
		_AudioSource.volume = target;
	}
}