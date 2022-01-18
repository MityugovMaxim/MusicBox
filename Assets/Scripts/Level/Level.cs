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
	}

	public void RegisterSampleReceivers(ISampleReceiver[] _SampleReceivers)
	{
		m_Sequencer.RegisterSampleReceivers(_SampleReceivers);
	}

	public void Setup(
		float   _Length,
		float   _BPM,
		float   _Speed,
		Track[] _Tracks
	)
	{
		if (m_Sequencer == null)
			return;
		
		m_Sequencer.Setup(_Length, _BPM, _Speed, _Tracks);
		m_Sequencer.Initialize();
	}

	public void Play(Action _Finished = null)
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Play level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Play(_Finished);
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
	public class Factory : PlaceholderFactory<Level, Level> { }
}