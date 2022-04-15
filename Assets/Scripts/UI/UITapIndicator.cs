using AudioBox.ASF;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Animator))]
public class UITapIndicator : UIIndicator
{
	[Preserve]
	public class Pool : UIIndicatorPool<UITapIndicator>
	{
		protected override void OnSpawned(UITapIndicator _Item)
		{
			base.OnSpawned(_Item);
			
			_Item.Restore();
		}

		protected override void OnDespawned(UITapIndicator _Item)
		{
			_Item.Restore();
			
			base.OnDespawned(_Item);
		}
	}

	public override UIHandle Handle => m_Handle;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");

	[SerializeField] UITapHandle m_Handle;

	public override void Restore()
	{
		m_Handle.Restore();
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
		
		Animator.Update(0);
	}

	public void Success(float _Progress)
	{
		Animator.SetTrigger(m_SuccessParameterID);
		
		FXProcessor.TapFX(Handle.GetWorldRect());
		
		InvokeCallback();
		
		SignalBus.Fire(new TapSuccessSignal(_Progress));
	}

	public void Fail(float _Progress)
	{
		Animator.SetTrigger(m_FailParameterID);
		
		FXProcessor.Fail();
		
		InvokeCallback();
		
		SignalBus.Fire(new TapFailSignal(_Progress));
	}
}