using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class LevelPreviewAudioSource : MonoBehaviour
{
	[SerializeField] float m_Volume             = 0.7f;
	[SerializeField] float m_TransitionDuration = 0.5f;

	StorageProcessor m_StorageProcessor;

	string      m_LevelID;
	AudioSource m_AudioSource;
	AudioClip   m_AudioClip;

	IEnumerator m_AudioRoutine;

	void Awake()
	{
		m_AudioSource = GetComponent<AudioSource>();
	}

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void Play(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		if (m_AudioSource == null)
			return;
		
		if (m_AudioRoutine != null)
			StopCoroutine(m_AudioRoutine);
		
		m_AudioClip = null;
		
		m_StorageProcessor.LoadPreview(
			_LevelID,
			_Preview =>
			{
				if (m_LevelID == _LevelID)
					m_AudioClip = _Preview;
			}
		);
		
		m_AudioRoutine = PlayRoutine(m_AudioSource, m_Volume, m_TransitionDuration);
		
		StartCoroutine(m_AudioRoutine);
	}

	public void Stop()
	{
		if (m_AudioSource == null)
			return;
		
		m_AudioClip = null;
		
		if (m_AudioRoutine != null)
			StopCoroutine(m_AudioRoutine);
		
		m_AudioRoutine = StopRoutine(m_AudioSource, m_TransitionDuration);
		
		StartCoroutine(m_AudioRoutine);
	}

	IEnumerator PlayRoutine(AudioSource _AudioSource, float _Volume, float _Duration)
	{
		if (_AudioSource == null)
			yield break;
		
		float source = _AudioSource.volume;
		float target = 0;
		float time   = 0;
		
		if (!Mathf.Approximately(source, target) && _AudioSource.isPlaying)
		{
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_AudioSource.volume = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_AudioSource.volume = target;
		_AudioSource.Stop();
		
		if (m_AudioClip == null)
			yield return new WaitWhile(() => m_AudioClip == null);
		
		if (m_AudioClip.loadState == AudioDataLoadState.Unloaded)
			m_AudioClip.LoadAudioData();
		
		yield return new WaitUntil(() => m_AudioClip.loadState == AudioDataLoadState.Loaded);
		
		_AudioSource.clip = m_AudioClip;
		_AudioSource.mute = false;
		_AudioSource.Play();
		
		source = _AudioSource.volume;
		target = _Volume;
		
		time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			_AudioSource.volume = Mathf.Lerp(source, target, time / _Duration);
		}
		
		_AudioSource.volume = target;
	}

	IEnumerator StopRoutine(AudioSource _AudioSource, float _Duration)
	{
		if (_AudioSource == null)
			yield break;
		
		float source = _AudioSource.volume;
		float target = 0;
		float time   = 0;
		
		if (!Mathf.Approximately(source, target) && _AudioSource.isPlaying)
		{
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_AudioSource.volume = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_AudioSource.volume = target;
		_AudioSource.Stop();
	}
}