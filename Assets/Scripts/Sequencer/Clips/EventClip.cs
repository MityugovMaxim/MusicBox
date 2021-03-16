public interface IEventClipReceiver
{
	void Invoke();
}

public class EventClip : Clip
{
	IEventClipReceiver[] m_Receivers;

	public void Initialize(Sequencer _Sequencer, IEventClipReceiver[] _Receivers)
	{
		base.Initialize(_Sequencer);
		
		m_Receivers = _Receivers;
	}

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		if (m_Receivers != null)
		{
			foreach (IEventClipReceiver receiver in m_Receivers)
				receiver.Invoke();
		}
	}
}