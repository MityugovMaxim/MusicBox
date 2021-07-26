using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Sequencer))]
public class Level : MonoBehaviour
{
	Sequencer      m_Sequencer;
	ScoreProcessor m_ScoreProcessor;
	AudioProcessor m_AudioProcessor;
	ColorProcessor m_ColorProcessor;

	[Inject]
	public void Construct(
		Sequencer             _Sequencer,
		ScoreProcessor        _ScoreProcessor,
		AudioProcessor        _AudioProcessor,
		ColorProcessor        _ColorProcessor,
		List<ISampleReceiver> _SampleReceivers
	)
	{
		m_Sequencer      = _Sequencer;
		m_ScoreProcessor = _ScoreProcessor;
		m_AudioProcessor = _AudioProcessor;
		m_ColorProcessor = _ColorProcessor;
		
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Initialize level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		if (m_ScoreProcessor != null)
			m_ScoreProcessor.Restore();
		
		if (m_AudioProcessor != null)
			m_AudioProcessor.Restore();
		
		if (m_ColorProcessor != null)
			m_ColorProcessor.Restore();
		
		m_Sequencer.Initialize();
	}

	public void Play()
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Play level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Play();
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
	public class Factory : PlaceholderFactory<string, Level> { }
}