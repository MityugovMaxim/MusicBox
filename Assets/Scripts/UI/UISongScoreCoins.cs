using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UISongScoreCoins : UIGroup
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_NextParameterID    = Animator.StringToHash("Next");

	public long Value
	{
		get => m_Value;
		set
		{
			if (m_Value == value)
				return;
			
			m_Value = value;
			
			ProcessValue();
		}
	}

	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] float       m_Duration;

	long m_Value;

	CancellationTokenSource m_TokenSource;

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Cancel();
	}

	public void Next()
	{
		m_Animator.SetTrigger(m_NextParameterID);
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		Cancel();
		
		m_Animator.ResetTrigger(m_NextParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	async void ProcessValue()
	{
		Cancel();
		
		try
		{
			long source = (long)m_Coins.Value;
			long target = m_Value;
			await UnityTask.Phase(
				_Phase => m_Coins.Value = MathUtility.Lerp(source, target, _Phase),
				m_Duration,
				EaseFunction.EaseIn
			);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		
		Complete();
	}

	void Cancel()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Complete()
	{
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}