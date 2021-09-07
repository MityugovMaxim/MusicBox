using UnityEngine;

public abstract class Clip : ScriptableObject
{
	public Sequencer Sequencer { get; private set; }

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

	public virtual float MinOffset => 0;
	public virtual float MaxOffset => 0;

	protected bool Playing { get; private set; }

	[SerializeField] float m_MinTime;
	[SerializeField] float m_MaxTime;

	public virtual void Initialize(Sequencer _Sequencer)
	{
		Sequencer = _Sequencer;
	}

	public void Sample(float _Time)
	{
		float minTime = MinTime + MinOffset;
		float maxTime = MaxTime + MaxOffset;
		
		if ((_Time >= minTime || _Time < maxTime) && Sequencer.Playing && !Playing)
		{
			Playing = true;
			OnEnter(_Time);
		}
		
		if (Playing)
			OnUpdate(_Time);
		
		if ((_Time < minTime || _Time >= maxTime) && Playing)
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