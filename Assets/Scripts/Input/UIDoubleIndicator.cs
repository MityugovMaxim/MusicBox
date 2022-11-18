using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Animator))]
public class UIDoubleIndicator : UIIndicator
{
	[Preserve]
	public class Pool : UIIndicatorPool<UIDoubleIndicator> { }

	public override UIHandle Handle => m_Handle;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");

	[SerializeField] UIDoubleHandle m_Handle;

	public override void Restore()
	{
		m_Handle.Restore();
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Success(float _Progress)
	{
		Animator.SetTrigger(m_SuccessParameterID);
		
		FXProcessor.DoubleFX(Handle.GetWorldRect(), _Progress);
		
		HapticProcessor.Process(Haptic.Type.ImpactHeavy);
		
		ScoreController.DoubleHit(_Progress);
		
		InvokeCallback();
	}

	public void Fail(float _Progress)
	{
		Animator.SetTrigger(m_FailParameterID);
		
		FXProcessor.Fail();
		
		HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		ScoreController.DoubleFail();
		
		InvokeCallback();
	}
}
