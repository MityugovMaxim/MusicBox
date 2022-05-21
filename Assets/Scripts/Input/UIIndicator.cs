using System;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public abstract class UIIndicator : UIEntity
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

	public float InputError  { get; private set; }
	public float InputOffset { get; private set; }

	protected Animator    Animator    => m_Animator;
	protected SignalBus   SignalBus   => m_SignalBus;
	protected FXProcessor FXProcessor => m_FXProcessor;

	[SerializeField] Animator m_Animator;

	[Inject] SignalBus       m_SignalBus;
	[Inject] FXProcessor     m_FXProcessor;
	[Inject] ConfigProcessor m_ConfigProcessor;

	Rect m_ViewRect;

	ASFClip         m_Clip;
	Action<ASFClip> m_Callback;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		InputError  = m_ConfigProcessor.InputError;
		InputOffset = m_ConfigProcessor.InputOffset;
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