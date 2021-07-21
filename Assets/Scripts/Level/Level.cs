using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Sequencer))]
public class Level : MonoBehaviour
{
	public event UnityAction<float, float> OnSample;
	public event UnityAction               OnComplete;

	Sequencer m_Sequencer;

	void Awake()
	{
		m_Sequencer = GetComponent<Sequencer>();
	}

	public void Initialize()
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Initialize level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Initialize();
		
		m_Sequencer.OnSample   += OnSample;
		m_Sequencer.OnComplete += OnComplete;
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

	public void Restart()
	{
		if (m_Sequencer == null)
		{
			Debug.LogErrorFormat(gameObject, "[Level] Restart level failed. Sequencer is not found at level '{0}'", name);
			return;
		}
		
		m_Sequencer.Stop();
	}
}
