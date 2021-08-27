using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Animator))]
public class UITapIndicator : UIIndicator
{
	[Preserve]
	public class Pool : FXPool<UITapIndicator>
	{
		protected override void Reinitialize(UITapIndicator _Item)
		{
			_Item.Restore();
		}
	}

	public override UIHandle Handle     => m_Handle;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");

	[SerializeField] UITapHandle m_Handle;

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
		SignalBus.Fire(new TapSuccessSignal(_Progress));
		
		FXProcessor.TapFX(Handle.GetWorldRect());
		
		Animator.SetTrigger(m_SuccessParameterID);
	}

	public void Fail(float _Progress)
	{
		SignalBus.Fire(new TapFailSignal(_Progress));
		
		Animator.SetTrigger(m_FailParameterID);
	}
}