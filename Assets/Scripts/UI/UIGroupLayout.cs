using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class UIGroupLayout : UIGroup
{
	public enum Mode
	{
		Both   = 0,
		Width  = 1,
		Height = 2,
	}

	LayoutElement LayoutElement
	{
		get
		{
			if (m_LayoutElement == null)
				m_LayoutElement = GetComponent<LayoutElement>();
			return m_LayoutElement;
		}
	}

	[SerializeField] float          m_ShowResizeDuration;
	[SerializeField] float          m_HideResizeDuration;
	[SerializeField] AnimationCurve m_ResizeCurve = AnimationCurve.Linear(0, 0, 1, 1);
	[SerializeField] Mode           m_ResizeMode;
	[SerializeField] Vector2        m_Size;

	CanvasGroup   m_CanvasGroup;
	LayoutElement m_LayoutElement;

	IEnumerator m_SizeRoutine;

	Action m_SizeFinished;

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false)
	{
		await ResizeAnimation(m_Size, m_ShowResizeDuration, _Instant);
		
		await base.ShowAnimation(_Duration, _Instant);
	}

	protected override async Task HideAnimation(float _Duration, bool _Instant = false)
	{
		await base.HideAnimation(_Duration, _Instant);
		
		await ResizeAnimation(Vector2.zero, m_HideResizeDuration, _Instant);
	}

	Task ResizeAnimation(Vector2 _Size, float _Duration, bool _Instant)
	{
		switch (m_ResizeMode)
		{
			case Mode.Both:
				return SizeAnimation(_Size, _Duration, m_ResizeCurve, _Instant);
			
			case Mode.Width:
				return WidthAnimation(_Size.x, _Duration, m_ResizeCurve, _Instant);
			
			case Mode.Height:
				return HeightAnimation(_Size.y, _Duration, m_ResizeCurve, _Instant);
			
			default:
				return null;
		}
	}

	Task WidthAnimation(float _Width, float _Duration, AnimationCurve _Curve, bool _Instant = false)
	{
		Vector2 size = new Vector2(_Width, LayoutElement.preferredHeight);
		
		return SizeAnimation(size, _Duration, _Curve, _Instant);
	}

	Task HeightAnimation(float _Height, float _Duration, AnimationCurve _Curve, bool _Instant = false)
	{
		Vector2 size = new Vector2(LayoutElement.preferredWidth, _Height);
		
		return SizeAnimation(size, _Duration, _Curve, _Instant);
	}

	Task SizeAnimation(Vector2 _Size, float _Duration, AnimationCurve _Curve, bool _Instant = false)
	{
		if (m_SizeRoutine != null)
			StopCoroutine(m_SizeRoutine);
		
		InvokeSizeFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_SizeFinished = () => completionSource.TrySetResult(true);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_SizeRoutine = SizeRoutine(LayoutElement, _Size, _Duration, _Curve, InvokeSizeFinished);
			
			StartCoroutine(m_SizeRoutine);
		}
		else
		{
			LayoutElement.preferredWidth  = _Size.x;
			LayoutElement.preferredHeight = _Size.y;
			
			InvokeSizeFinished();
		}
		
		return completionSource.Task;
	}

	static IEnumerator SizeRoutine(
		LayoutElement  _LayoutElement,
		Vector2        _Size,
		float          _Duration,
		AnimationCurve _Curve,
		Action         _Finished
	)
	{
		if (_LayoutElement == null)
		{
			_Finished?.Invoke();
			yield break;
		}
		
		Vector2 source = new Vector2(_LayoutElement.preferredWidth, _LayoutElement.preferredHeight);
		Vector2 target = _Size;
		if (source != target && _Duration > float.Epsilon)
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = _Curve.Evaluate(time / _Duration);
				
				Vector2 size = Vector2.Lerp(source, target, phase);
				
				_LayoutElement.preferredWidth  = size.x;
				_LayoutElement.preferredHeight = size.y;
			}
		}
		
		_LayoutElement.preferredWidth  = target.x;
		_LayoutElement.preferredHeight = target.y;
		
		_Finished?.Invoke();
	}

	void InvokeSizeFinished()
	{
		Action action = m_SizeFinished;
		m_SizeFinished = null;
		action?.Invoke();
	}
}