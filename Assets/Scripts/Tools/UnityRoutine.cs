using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UnityRoutine
{
	public static IEnumerator UnitRoutine(
		UIUnitLabel  _Label,
		long         _Value,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		return UnitRoutine(
			_Label,
			(long)_Label.Value,
			_Value,
			_Duration,
			_Function,
			_Finished
		);
	}

	public static IEnumerator UnitRoutine(
		UIUnitLabel  _Label,
		long         _Source,
		long         _Target,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		if (_Label == null)
			yield break;
		
		if (_Source != _Target && _Duration > float.Epsilon)
		{
			_Function ??= EaseFunction.Linear;
			
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Label.Value = MathUtility.Lerp(_Source, _Target, _Function.Get(time / _Duration));
			}
		}
		
		_Label.Value = _Target;
		
		_Finished?.Invoke();
	}

	public static IEnumerator AlphaRoutine(
		Graphic      _Graphic,
		float        _Alpha,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		return AlphaRoutine(
			_Graphic,
			_Graphic.color.a,
			_Alpha,
			_Duration,
			_Function,
			_Finished
		);
	}

	public static IEnumerator AlphaRoutine(
		Graphic      _Graphic,
		float        _Source,
		float        _Target,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		if (_Graphic == null)
			yield break;
		
		Color color = _Graphic.color;
		
		if (!Mathf.Approximately(_Source, _Target) && _Duration > float.Epsilon)
		{
			_Function ??= EaseFunction.Linear;
			
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				color.a = _Function.Get(_Source, _Target, time / _Duration);
				
				_Graphic.color = color;
			}
		}
		
		color.a = _Target;
		
		_Graphic.color = color;
		
		_Finished?.Invoke();
	}

	public static IEnumerator AlphaRoutine(
		CanvasGroup  _CanvasGroup,
		float        _Alpha,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		return AlphaRoutine(
			_CanvasGroup,
			_CanvasGroup.alpha,
			_Alpha,
			_Duration,
			_Function,
			_Finished
		);
	}

	public static IEnumerator AlphaRoutine(
		CanvasGroup  _CanvasGroup,
		float        _Source,
		float        _Target,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		if (_CanvasGroup == null)
			yield break;
		
		if (!Mathf.Approximately(_Source, _Target) && _Duration > float.Epsilon)
		{
			_Function ??= EaseFunction.Linear;
			
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = _Function.Get(_Source, _Target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = _Target;
		
		_Finished?.Invoke();
	}

	public static IEnumerator ColorRoutine(
		Graphic      _Graphic,
		Color        _Color,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		return ColorRoutine(
			_Graphic,
			_Graphic.color,
			_Color,
			_Duration,
			_Function,
			_Finished
		);
	}

	public static IEnumerator ColorRoutine(
		Graphic      _Graphic,
		Color        _Source,
		Color        _Target,
		float        _Duration,
		EaseFunction _Function,
		Action       _Finished = null
	)
	{
		if (_Graphic == null)
			yield break;
		
		if (_Source != _Target && _Duration > float.Epsilon)
		{
			_Function ??= EaseFunction.Linear;
			
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Graphic.color = _Function.Get(_Source, _Target, time / _Duration);
			}
		}
		
		_Graphic.color = _Target;
		
		_Finished?.Invoke();
	}

	public static IEnumerator WidthRoutine(
		RectTransform _Transform,
		float         _Source,
		float         _Target,
		float         _Duration,
		EaseFunction  _Function,
		Action        _Finished = null
	)
	{
		if (_Transform == null)
			yield break;
		
		Vector2 position = _Transform.sizeDelta;
		
		if (!Mathf.Approximately(_Source, _Target) && _Duration > float.Epsilon)
		{
			_Function ??= EaseFunction.Linear;
			
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				position.x = _Function.Get(_Source, _Target, time / _Duration);
				
				_Transform.sizeDelta = position;
			}
		}
		
		position.x = _Target;
		
		_Transform.sizeDelta = position;
		
		_Finished?.Invoke();
	}

	public static IEnumerator PositionRoutine(
		RectTransform _Transform,
		Vector2       _Position,
		float         _Duration,
		EaseFunction  _Function,
		Action        _Finished = null
	)
	{
		return PositionRoutine(
			_Transform,
			_Transform.anchoredPosition,
			_Position,
			_Duration,
			_Function,
			_Finished
		);
	}

	public static IEnumerator PositionRoutine(
		RectTransform _Transform,
		Vector2       _Source,
		Vector2       _Target,
		float         _Duration,
		EaseFunction  _Function,
		Action        _Finished = null
	)
	{
		if (_Transform == null)
			yield break;
		
		if (_Source != _Target && _Duration > float.Epsilon)
		{
			_Function ??= EaseFunction.Linear;
			
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Transform.anchoredPosition = _Function.Get(_Source, _Target, time / _Duration);
			}
		}
		
		_Transform.anchoredPosition = _Target;
		
		_Finished?.Invoke();
	}
}