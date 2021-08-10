using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Sequencer))]
public class Level : MonoBehaviour
{
	Sequencer      m_Sequencer;
	AudioProcessor m_AudioProcessor;
	ColorProcessor m_ColorProcessor;

	[Inject]
	public void Construct(
		Sequencer      _Sequencer,
		AudioProcessor _AudioProcessor,
		ColorProcessor _ColorProcessor
	)
	{
		m_Sequencer      = _Sequencer;
		m_AudioProcessor = _AudioProcessor;
		m_ColorProcessor = _ColorProcessor;
		
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Initialize level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		if (m_AudioProcessor != null)
			m_AudioProcessor.Restore();
		
		if (m_ColorProcessor != null)
			m_ColorProcessor.Restore();
		
		m_Sequencer.Initialize();
	}

	public void Play(ISampleReceiver[] _SampleReceivers, Action _Finished = null)
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Play level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Play(_SampleReceivers, _Finished);
	}

	public void Pause()
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Pause level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Pause();
	}

	public void Stop()
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Restart level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Stop();
		
		if (m_AudioProcessor != null)
			m_AudioProcessor.Restore();
		
		if (m_ColorProcessor != null)
			m_ColorProcessor.Restore();
	}

	[Preserve]
	public class Factory : PlaceholderFactory<string, Action<Level>, ResourceRequest> { }
}