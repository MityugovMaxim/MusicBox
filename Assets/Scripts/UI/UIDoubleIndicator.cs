using System;
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

	public override UIHandle Handle => m_Handle;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");

	[SerializeField] UIDoubleHandle m_Handle;

	DoubleClip         m_Clip;
	Action<DoubleClip> m_Use;

	public void Setup(DoubleClip _Clip, Action<DoubleClip> _Use = null)
	{
		m_Clip = _Clip;
		m_Use  = _Use;
		
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
		
		FXProcessor.DoubleFX(Handle.GetWorldRect());
		
		Animator.SetTrigger(m_SuccessParameterID);
		
		InvokeUse();
	}

	public void Fail(float _Progress)
	{
		SignalBus.Fire(new DoubleFailSignal(_Progress));
		
		Animator.SetTrigger(m_FailParameterID);
		
		InvokeUse();
	}

	void InvokeUse()
	{
		Action<DoubleClip> action = m_Use;
		DoubleClip         clip   = m_Clip;
		m_Use  = null;
		m_Clip = null;
		action?.Invoke(clip);
	}
}