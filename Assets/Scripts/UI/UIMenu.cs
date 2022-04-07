using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class MenuAttribute : Attribute
{
	public MenuType MenuType { get; }

	public MenuAttribute(MenuType _MenuType)
	{
		MenuType = _MenuType;
	}
}

public class UIMenu : UIGroup
{
	[Preserve]
	public class Factory : PlaceholderFactory<UIMenu, UIMenu> { }

	[Inject] LocalizationProcessor m_LocalizationProcessor;

	[SerializeField] UIBlur m_Blur;

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

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		if (m_Blur != null)
			await m_Blur.BlurAsync();
		
		await base.ShowAnimation(_Duration, _Instant, _Token);
	}
}
