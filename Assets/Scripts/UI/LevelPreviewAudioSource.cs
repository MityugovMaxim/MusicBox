using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class LevelPreviewAudioSource : MonoBehaviour
{
	[SerializeField] float m_Volume             = 0.7f;
	[SerializeField] float m_TransitionDuration = 0.5f;

	LevelProcessor m_LevelProcessor;

	AudioSource m_AudioSource;

	IEnumerator m_AudioRoutine;

	void Awake()
	{
		m_AudioSource = GetComponent<AudioSource>();
	}

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor)
	{
		m_LevelProcessor = _LevelProcessor;
	}

	public void Play(string _LevelID)
	{
		AudioClip previewClip = m_LevelProcessor.GetPreviewClip(_LevelID);
		
		if (m_AudioRoutine != null)
			StopCoroutine(m_AudioRoutine);
		
		m_AudioRoutine = PlayRoutine(m_AudioSource, previewClip, m_Volume, m_TransitionDuration);
		
		StartCoroutine(m_AudioRoutine);
	}

	public void Stop()
	{
		if (m_AudioRoutine != null)
			StopCoroutine(m_AudioRoutine);
		
		m_AudioRoutine = StopRoutine(m_AudioSource, m_TransitionDuration);
		
		StartCoroutine(m_AudioRoutine);
	}

	static IEnumerator PlayRoutine(AudioSource _AudioSource, AudioClip _AudioClip, float _Volume, float _Duration)
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
		_AudioSource.clip = _AudioClip;
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

	static IEnumerator StopRoutine(AudioSource _AudioSource, float _Duration)
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