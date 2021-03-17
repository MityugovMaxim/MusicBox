#if UNITY_EDITOR
public partial class InputClip
{
	public void Setup(float _Duration, float _Time, float _MinZone, float _MaxZone)
	{
		float sourceTime = MathUtility.Remap(ZoneTime, 0, 1, MinTime, MaxTime);
		float targetTime = MathUtility.Remap(_Time, 0, 1, MinTime, MaxTime);
		float deltaTime  = targetTime - sourceTime;
		float minTime    = targetTime - _Duration * _Time;
		float maxTime    = targetTime + _Duration * (1 - _Time);
		
		MinTime = minTime - deltaTime;
		MaxTime = maxTime - deltaTime;
		
		ZoneTime = _Time;
		MinZone  = _MinZone;
		MaxZone  = _MaxZone;
	}
}
#endif

public partial class InputClip : Clip
{
	public float ZoneTime { get; private set; }
	public float MinZone  { get; private set; }
	public float MaxZone  { get; private set; }

	int         m_ID;
	InputReader m_InputReader;
	bool        m_Reading;

	public void Initialize(Sequencer _Sequencer, int _ID, InputReader _InputReader, float _Time, float _MinZone, float _MaxZone)
	{
		base.Initialize(_Sequencer);
		
		m_ID          = _ID;
		m_InputReader = _InputReader;
		ZoneTime    = _Time;
		MinZone     = _MinZone;
		MaxZone     = _MaxZone;
	}

	protected override void OnEnter(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		
		m_InputReader.StartProcessing(m_ID, time);
	}

	protected override void OnUpdate(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		
		m_InputReader.UpdateProcessing(m_ID, time);
		
		if (time >= MinZone && !m_Reading)
		{
			m_Reading = true;
			m_InputReader.StartInput(m_ID);
		}
		
		if (time >= MaxZone && m_Reading)
		{
			m_Reading = false;
			m_InputReader.FinishInput(m_ID);
		}
	}

	protected override void OnExit(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		
		if (m_Reading)
		{
			m_Reading = false;
			m_InputReader.FinishInput(m_ID);
		}
		
		m_InputReader.FinishProcessing(m_ID, time);
	}
}