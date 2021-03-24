public class InputClip : Clip
{
	public override float MinTime => base.MinTime - m_Duration * Zone;
	public override float MaxTime => base.MaxTime + m_Duration * (1 - Zone);

	public float Zone { get; private set; }
	public float ZoneMin  { get; private set; }
	public float ZoneMax  { get; private set; }

	InputReader m_InputReader;
	int         m_InputID;
	float       m_Duration;
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
		m_Duration    = _Duration;
		
		Zone    = _Zone;
		ZoneMin = _ZoneMin;
		ZoneMax = _ZoneMax;
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
		
		if (time >= ZoneMin && time <= ZoneMax && !m_Reading)
		{
			m_Reading = true;
			m_InputReader.StartInput(m_InputID);
		}
		
		if ((time < ZoneMin || time > ZoneMax) && m_Reading)
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