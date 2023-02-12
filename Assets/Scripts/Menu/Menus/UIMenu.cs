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

	[Inject] MenuProcessor m_MenuProcessor;

	[SerializeField] UIBlur m_Blur;

	public virtual void OnFocusGain() { }

	public virtual void OnFocusLose() { }

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_MenuProcessor.Register(this);
	}

	protected override void OnShowFinished()
	{
		base.OnShowFinished();
		
		NativeHandler.RegisterParameters(OnParameters);
		NativeHandler.RegisterEscape(OnEscape);
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		NativeHandler.UnregisterParameters(OnParameters);
		NativeHandler.UnregisterEscape(OnEscape);
		
		m_MenuProcessor.Unregister(this);
	}

	protected virtual bool OnParameters() => false;

	protected virtual bool OnEscape() => false;

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		if (m_Blur != null && Application.isPlaying)
			await m_Blur.BlurAsync();
		
		await base.ShowAnimation(_Duration, _Instant, _Token);
	}
}
