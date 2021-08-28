public class GotoClip : Clip
{
	GotoResolver m_Resolver;

	public void Initialize(Sequencer _Sequencer, GotoResolver _Resolver)
	{
		base.Initialize(_Sequencer);
		
		m_Resolver = _Resolver;
	}

	protected override void OnEnter(float _Time)
	{
		if (!Sequencer.Playing || !Playing)
			return;
		
		m_Resolver.Show();
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Sequencer.Playing && Playing)
			m_Resolver.Hide();
		else if (Sequencer.Playing && Playing && _Time < MaxTime)
			m_Resolver.Show();
	}

	protected override void OnExit(float _Time)
	{
		if (m_Resolver.Resolve())
			m_Resolver.Hide();
		else if (_Time > MinTime)
			Sequencer.Goto(MinTime);
	}
}