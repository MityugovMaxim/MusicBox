using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrackThumbnail : Thumbnail
{
	const float FADE_IN_DURATION  = 0.2f;
	const float FADE_OUT_DURATION = 0.2f;

	public override string ID => m_ID;

	AudioSource AudioSource
	{
		get
		{
			if (m_AudioSource == null)
				m_AudioSource = GetComponent<AudioSource>();
			return m_AudioSource;
		}
	}

	[SerializeField] string m_ID;

	AudioSource m_AudioSource;
	IEnumerator m_AudioRoutine;

	public override void OnShow()
	{
		Play();
	}

	public override void OnHide()
	{
		Stop();
	}

	void Play()
	{
		if (m_AudioRoutine != null)
			StopCoroutine(m_AudioRoutine);
		
		m_AudioRoutine = PlayRoutine(AudioSource, FADE_IN_DURATION);
		
		StartCoroutine(m_AudioRoutine);
	}

	void Stop()
	{
		if (m_AudioRoutine != null)
			StopCoroutine(m_AudioRoutine);
		
		m_AudioRoutine = StopRoutine(AudioSource, FADE_OUT_DURATION);
		
		StartCoroutine(m_AudioRoutine);
	}

	static IEnumerator PlayRoutine(AudioSource _AudioSource, float _Duration)
	{
		if (_AudioSource == null)
			yield break;
		
		float source = _AudioSource.volume;
		float target = 1;
		
		if (!_AudioSource.isPlaying)
			_AudioSource.Play();
		
		float time = 0;
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
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			_AudioSource.volume = Mathf.Lerp(source, target, time / _Duration);
		}
		
		_AudioSource.volume = target;
		_AudioSource.Stop();
	}
}