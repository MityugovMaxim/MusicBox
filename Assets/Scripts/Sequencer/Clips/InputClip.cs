public class InputClip : Clip
{
	public float Zone { get; private set; }
	public float ZoneMin  { get; private set; }
	public float ZoneMax  { get; private set; }

	InputReader m_InputReader;
	int         m_InputID;
	bool        m_Reading;

	public void Initialize(
		Sequencer   _Sequencer,
		InputReader _InputReader,
		int         _ID,
		float       _Duration,
		float       _Zone,
		float       _ZoneMin,
		float       _ZoneMax
	)
	{
		base.Initialize(_Sequencer);
		
		m_InputReader = _InputReader;
		m_InputID     = _ID;
		
		float sourceTime = MathUtility.Remap(Zone, 0, 1, MinTime, MaxTime);
		float targetTime = MathUtility.Remap(_Zone, 0, 1, MinTime, MaxTime);
		float deltaTime  = targetTime - sourceTime;
		float minTime    = targetTime - _Duration * _Zone;
		float maxTime    = targetTime + _Duration * (1 - _Zone);
		
		MinTime = minTime - deltaTime;
		MaxTime = maxTime - deltaTime;
		
		Zone    = _Zone;
		ZoneMin = _ZoneMin;
		ZoneMax = _ZoneMax;
	}

	public void Setup(float _Duration, float _Time, float _MinZone, float _MaxZone)
	{
		float sourceTime = MathUtility.Remap(Zone, 0, 1, MinTime, MaxTime);
		float targetTime = MathUtility.Remap(_Time, 0, 1, MinTime, MaxTime);
		float deltaTime  = targetTime - sourceTime;
		float minTime    = targetTime - _Duration * _Time;
		float maxTime    = targetTime + _Duration * (1 - _Time);
		
		MinTime = minTime - deltaTime;
		MaxTime = maxTime - deltaTime;
		
		Zone    = _Time;
		ZoneMin = _MinZone;
		ZoneMax = _MaxZone;
	}

	protected override void OnEnter(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		
		m_InputReader.StartProcessing(m_InputID, time);
	}

	protected override void OnUpdate(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		
		m_InputReader.UpdateProcessing(m_InputID, time);
		
		if (time >= ZoneMin && !m_Reading)
		{
			m_Reading = true;
			m_InputReader.StartInput(m_InputID);
		}
		
		if (time >= ZoneMax && m_Reading)
		{
			m_Reading = false;
			m_InputReader.FinishInput(m_InputID);
		}
	}

	protected override void OnExit(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		
		if (m_Reading)
		{
			m_Reading = false;
			m_InputReader.FinishInput(m_InputID);
		}
		
		m_InputReader.FinishProcessing(m_InputID, time);
	}
}