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

	[SerializeField] UIBlur m_Blur;

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		if (m_Blur != null)
			await m_Blur.BlurAsync();
		
		await base.ShowAnimation(_Duration, _Instant, _Token);
	}
}
