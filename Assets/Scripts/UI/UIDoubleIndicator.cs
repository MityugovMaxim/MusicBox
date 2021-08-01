using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIDoubleIndicator : UIIndicator
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIDoubleIndicator>
	{
		protected override void Reinitialize(UIDoubleIndicator _Item)
		{
			_Item.Restore();
		}
	}

	public override UIHandle Handle     => m_Handle;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");

	[SerializeField] UIDoubleHandle m_Handle;

	public void Setup()
	{
		if (m_Handle != null)
			m_Handle.Setup(this);
	}

	public void Restore()
	{
		if (m_Handle != null)
			m_Handle.StopReceiveInput();
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
		
		if (gameObject.activeInHierarchy)
			Animator.Update(0);
	}

	public void Success(float _Progress)
	{
		SignalBus.Fire(new DoubleSuccessSignal(_Progress));
		
		Animator.SetTrigger(m_SuccessParameterID);
	}

	public void Fail(float _Progress)
	{
		SignalBus.Fire(new DoubleFailSignal(_Progress));
		
		Animator.SetTrigger(m_FailParameterID);
	}
}