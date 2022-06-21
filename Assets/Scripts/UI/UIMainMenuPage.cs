using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public abstract class UIMainMenuPage : UIPage<MainMenuPageType>
{
	[Inject] LocalizationProcessor m_LocalizationProcessor;

	float m_Direction;

	static float GetDirection(MainMenuPageType _Source, MainMenuPageType _Target)
	{
		MainMenuPageType[] order =
		{
			MainMenuPageType.News,
			MainMenuPageType.Offers,
			MainMenuPageType.Songs,
			MainMenuPageType.Store,
			MainMenuPageType.Profile
		};
		
		int source = Array.IndexOf(order, _Source);
		int target = Array.IndexOf(order, _Target);
		
		if (source == target)
			return 0;
		
		return source < target ? 1 : -1;
	}

	public void Show(MainMenuPageType _Source, bool _Instant)
	{
		m_Direction = GetDirection(_Source, Type) * 0.25f;
		
		if (!Shown)
		{
			RectTransform.anchorMin = new Vector2(0 + m_Direction, 0);
			RectTransform.anchorMax = new Vector2(1 + m_Direction, 1);
		}
		
		base.Show(_Instant);
	}

	public void Hide(MainMenuPageType _Target, bool _Instant)
	{
		m_Direction = GetDirection(_Target, Type) * 0.25f;
		
		if (!Shown)
		{
			RectTransform.anchorMin = new Vector2(0 + m_Direction, 0);
			RectTransform.anchorMax = new Vector2(1 + m_Direction, 1);
		}
		
		base.Hide(_Instant);
	}

	protected override Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return Task.WhenAll(
			base.ShowAnimation(_Duration, _Instant, _Token),
			MoveAsync(0, _Duration, _Instant, EaseFunction.EaseOutQuad, _Token)
		);
	}

	protected override Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return Task.WhenAll(
			base.HideAnimation(_Duration, _Instant, _Token),
			MoveAsync(m_Direction, _Duration, _Instant, EaseFunction.EaseOutQuad, _Token)
		);
	}

	protected string GetLocalization(string _Key)
	{
		return m_LocalizationProcessor.Get(_Key);
	}

	protected string GetLocalization(string _Key, params object[] _Args)
	{
		if (_Args == null)
			return m_LocalizationProcessor.Get(_Key);
		else if (_Args.Length == 1)
			return m_LocalizationProcessor.Format(_Key, _Args[0]);
		else if (_Args.Length == 2)
			return m_LocalizationProcessor.Format(_Key, _Args[0], _Args[1]);
		else if (_Args.Length == 3)
			return m_LocalizationProcessor.Format(_Key, _Args[0], _Args[1], _Args[2]);
		else
			return m_LocalizationProcessor.Format(_Key, _Args);
	}

	Task MoveAsync(float _Position, float _Duration, bool _Instant, EaseFunction _Function, CancellationToken _Token = default)
	{
		Vector2 sourceMin = RectTransform.anchorMin;
		Vector2 sourceMax = RectTransform.anchorMax;
		Vector2 targetMin = new Vector2(0 + _Position, 0);
		Vector2 targetMax = new Vector2(1 + _Position, 1);
		
		void Process(float _Phase)
		{
			RectTransform.anchorMin = Vector2.LerpUnclamped(sourceMin, targetMin, _Phase);
			RectTransform.anchorMax = Vector2.LerpUnclamped(sourceMax, targetMax, _Phase);
		}
		
		if (_Instant)
		{
			Process(1);
			return Task.CompletedTask;
		}
		
		return UnityTask.Phase(
			Process,
			0,
			_Duration,
			_Function,
			_Token
		);
	}
}