using UnityEngine;

public abstract class Clip : ScriptableObject
{
	public Sequencer Sequencer { get; private set; }

	public float MinTime
	{
		get => m_MinTime;
		protected set => m_MinTime = value;
	}

	public float MaxTime
	{
		get => m_MaxTime;
		protected set => m_MaxTime = value;
	}

	public bool Playing { get; private set; }

	[SerializeField] float m_MinTime;
	[SerializeField] float m_MaxTime;

	public virtual void Initialize(Sequencer _Sequencer)
	{
		Sequencer = _Sequencer;
	}

	public void Sample(float _Time)
	{
		if ((_Time >= MinTime || _Time <= MaxTime) && !Playing)
		{
			Playing = true;
			OnEnter(_Time);
		}
		
		if (Playing)
			OnUpdate(_Time);
		
		if ((_Time <= MinTime || _Time >= MaxTime) && Playing)
		{
			Playing = false;
			OnExit(_Time);
		}
	}

	protected abstract void OnEnter(float _Time);

	protected abstract void OnUpdate(float _Time);

	protected abstract void OnExit(float _Time);

	protected float GetNormalizedTime(float _Time)
	{
		return Mathf.InverseLerp(MinTime, MaxTime, _Time);
	}

	protected float GetLocalTime(float _Time)
	{
		return _Time - MinTime;
	}
}