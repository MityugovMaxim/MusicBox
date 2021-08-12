using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
public partial class Sequencer
{
	public float MinTime
	{
		get => m_MinTime;
		set => m_MinTime = value;
	}

	public float MaxTime
	{
		get => m_MaxTime;
		set => m_MaxTime = value;
	}

	public float TracksWidth
	{
		get => m_TracksWidth;
		set => m_TracksWidth = value;
	}

	public float ScrollPosition
	{
		get => m_ScrollPosition;
		set => m_ScrollPosition = value;
	}

	[SerializeField, HideInInspector] float m_MinTime        = 0;
	[SerializeField, HideInInspector] float m_MaxTime        = 60;
	[SerializeField, HideInInspector] float m_TracksWidth    = 120;
	[SerializeField, HideInInspector] float m_ScrollPosition = 0;

	[Obsolete]
	[ContextMenu("Normalize")]
	public void Normalize()
	{
		float minTime = float.MaxValue;
		float maxTime = float.MinValue;
		
		foreach (Track track in Tracks)
		foreach (Clip clip in track)
		{
			minTime = Mathf.Min(clip.MinTime, minTime);
			maxTime = Mathf.Max(clip.MaxTime, maxTime);
		}
		
		float offset = 60.0f / BPM;
		
		foreach (Track track in Tracks)
		foreach (Clip clip in track)
		{
			using (UnityEditor.SerializedObject clipObject = new UnityEditor.SerializedObject(clip))
			{
				UnityEditor.SerializedProperty minTimeProperty = clipObject.FindProperty("m_MinTime");
				UnityEditor.SerializedProperty maxTimeProperty = clipObject.FindProperty("m_MaxTime");
				
				minTimeProperty.floatValue += offset * 4 - minTime;
				maxTimeProperty.floatValue += offset * 4 - minTime;
				
				clipObject.ApplyModifiedProperties();
			}
		}
		
		Length = maxTime - minTime + offset * 6;
	}
}
#endif

[ExecuteInEditMode]
public partial class Sequencer : MonoBehaviour
{
	[Serializable]
	public class SampleEvent : UnityEvent<float, float> { }

	public float Time
	{
		get => m_Time;
		private set => m_Time = value;
	}

	public float BPM => m_BPM;

	public bool Playing { get; private set; }

	public Track[] Tracks => m_Tracks;

	public float Length
	{
		get => m_Length;
		set => m_Length = value;
	}

	[SerializeField] float m_Time;
	[SerializeField] float m_Length;
	[SerializeField] float m_BPM = 90;

	[SerializeField, HideInInspector] Track[] m_Tracks;

	Action            m_Finished;
	int               m_Frame;
	int               m_SampleFrame;
	ISampleReceiver[] m_SampleReceivers;

	void OnDisable()
	{
		if (!Application.isPlaying)
			return;
		
		Pause();
	}

	void LateUpdate()
	{
		if (Playing && m_SampleFrame != m_Frame)
		{
			m_SampleFrame = m_Frame;
			
			Sample(m_Time + UnityEngine.Time.deltaTime);
		}
		
		m_Frame++;
	}

	public void Initialize()
	{
		if (m_Tracks == null || m_Tracks.Length == 0)
			return;
		
		foreach (Track track in m_Tracks)
			track.Initialize(this);
	}

	public void Play(ISampleReceiver[] _SampleReceivers, Action _Finished = null)
	{
		Playing = true;
		
		m_SampleReceivers = _SampleReceivers;
		m_Finished        = _Finished;
		m_Frame           = 0;
		m_SampleFrame     = 0;
		
		Sample(Time);
	}

	public void Pause()
	{
		Playing = false;
		
		m_Frame       = 0;
		m_SampleFrame = 0;
		
		Sample(Time);
	}

	public void Stop()
	{
		Playing = false;
		
		m_Frame       = 0;
		m_SampleFrame = 0;
		
		Sample(0);
	}

	public void Sample(float _Time)
	{
		foreach (Track track in m_Tracks)
			track.Sample(Time, _Time);
		
		Time = _Time;
		
		if (m_SampleReceivers != null)
		{
			foreach (ISampleReceiver sampleReceiver in m_SampleReceivers)
				sampleReceiver.Sample(Time, Length);
		}
		
		if (Time >= m_Length)
		{
			Playing = false;
			
			foreach (Track track in m_Tracks)
				track.Sample(Time, m_Length);
			
			Time = m_Length;
			
			Action action = m_Finished;
			m_Finished = null;
			action?.Invoke();
		}
	}
}