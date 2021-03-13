public interface IEventClipReceiver
{
	void Invoke();
}

public class EventClip : Clip
{
	IEventClipReceiver[] m_Receivers;

	public void Initialize(IEventClipReceiver[] _Receivers)
	{
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

	protected override void OnStop(float _Time) { }
}