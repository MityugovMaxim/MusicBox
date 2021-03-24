using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class InputFXView : MonoBehaviour
{
	ParticleSystem ParticleSystem
	{
		get
		{
			if (m_ParticleSystem == null)
				m_ParticleSystem = GetComponent<ParticleSystem>();
			return m_ParticleSystem;
		}
	}

	ParticleSystem m_ParticleSystem;
	Action         m_PlayCallback;

	public void Play(Action _Callback = null)
	{
		StopAllCoroutines();
		
		InvokePlayCallback();
		
		m_PlayCallback = _Callback;
		
		StartCoroutine(PlayRoutine());
	}

	IEnumerator PlayRoutine()
	{
		ParticleSystem.Play();
		
		yield return new WaitWhile(() => ParticleSystem.isEmitting);
		
		ParticleSystem.Stop();
		
		InvokePlayCallback();
	}

	void InvokePlayCallback()
	{
		Action callback = m_PlayCallback;
		m_PlayCallback = null;
		callback?.Invoke();
	}
}