using System;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public abstract class UIIndicator : UIOrder
{
	public abstract UIHandle Handle { get; }

	public RectTransform Container { get; private set; }

	public Rect ClipRect { get; set; }

	public Rect ViewRect
	{
		get => m_ViewRect;
		set
		{
			if (m_ViewRect.position != value.position)
				Transform.localPosition = value.center;
			
			if (m_ViewRect.size != value.size)
				RectTransform.sizeDelta = value.size;
			
			m_ViewRect = value;
		}
	}

	protected Animator        Animator        => m_Animator;
	protected ScoreController ScoreController => m_ScoreController;
	protected FXProcessor     FXProcessor     => m_FXProcessor;
	protected HapticProcessor HapticProcessor => m_HapticProcessor;

	[SerializeField] Animator m_Animator;

	[Inject] ScoreController m_ScoreController;
	[Inject] FXProcessor     m_FXProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	Rect m_ViewRect;

	ASFClip         m_Clip;
	Action<ASFClip> m_Callback;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Setup(RectTransform _Container, ASFClip _Clip, Action<ASFClip> _Callback)
	{
		Container  = _Container;
		m_Clip     = _Clip;
		m_Callback = _Callback;
	}

	public abstract void Restore();

	protected void InvokeCallback()
	{
		m_Callback?.Invoke(m_Clip);
	}
}
