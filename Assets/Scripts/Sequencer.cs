using UnityEngine;

[ExecuteAlways]
public class Sequencer : MonoBehaviour, IExposedPropertyTable
{
	public float Time
	{
		get => m_Time;
		set => m_Time = value;
	}

	public bool Playing { get; private set; }

	public Track[] Tracks => m_Tracks;

	[SerializeField] Track[] m_Tracks;

	[SerializeField] float m_Time;

	void Awake()
	{
		Initialize();
	}

	void LateUpdate()
	{
		if (Playing)
			Sample(m_Time + UnityEngine.Time.deltaTime);
	}

	public void Initialize()
	{
		foreach (Track track in m_Tracks)
			track.Initialize(this);
	}

	public void Play()
	{
		Playing = true;
	}

	public void Pause()
	{
		Playing = false;
	}

	public void Stop()
	{
		foreach (Track track in m_Tracks)
			track.Stop(m_Time);
		
		Playing = false;
		Time    = 0;
	}

	public void Sample(float _Time)
	{
		if (Mathf.Approximately(Time, _Time))
			return;
		
		foreach (Track track in m_Tracks)
			track.Sample(Time, _Time);
		
		Time = _Time;
	}

	public void SetReferenceValue(PropertyName _ID, Object _Value)
	{
		Debug.LogError("---> SET " + _ID);
	}

	public Object GetReferenceValue(PropertyName _ID, out bool _IDValid)
	{
		_IDValid = true;
		Debug.LogError("---> GET " + _ID);
		return null;
	}

	public void ClearReferenceValue(PropertyName _ID)
	{
		Debug.LogError("---> CLEAR " + _ID);
	}
}