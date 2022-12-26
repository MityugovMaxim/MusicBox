using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class MenuProcessor : IInitializable
{
	readonly Localization                   m_Localization;
	readonly Canvas                         m_Canvas;
	readonly UIMenu.Factory                 m_MenuFactory;
	readonly Dictionary<MenuType, UIMenu>   m_MenuCache;
	readonly Dictionary<MenuType, MenuInfo> m_MenuInfos;
	readonly List<MenuType>                 m_MenuOrder;
	readonly List<MenuType>                 m_MenuFocus;

	MenuType m_FocusedMenu;

	[Inject]
	public MenuProcessor(
		Localization   _Localization,
		Canvas         _Canvas,
		MenuInfo[]     _MenuInfos,
		UIMenu.Factory _MenuFactory
	)
	{
		m_Localization = _Localization;
		m_Canvas       = _Canvas;
		m_MenuFactory  = _MenuFactory;
		m_MenuCache    = new Dictionary<MenuType, UIMenu>();
		m_MenuInfos    = _MenuInfos.ToDictionary(_MenuInfo => _MenuInfo.Type, _MenuInfo => _MenuInfo);
		m_MenuOrder    = _MenuInfos.Select(_MenuInfo => _MenuInfo.Type).ToList();
		m_MenuFocus    = _MenuInfos.Where(_MenuInfo => _MenuInfo.Focusable).Select(_MenuInfo => _MenuInfo.Type).ToList();
	}

	async void IInitializable.Initialize()
	{
		await Show(MenuType.SplashMenu, true);
	}

	public void ProcessFocus()
	{
		MenuType menuType = m_MenuFocus.FirstOrDefault(_MenuType => m_MenuCache.ContainsKey(_MenuType) && m_MenuCache[_MenuType].Shown);
		
		if (menuType == m_FocusedMenu)
			return;
		
		UIMenu source = GetMenu<UIMenu>(m_FocusedMenu, true);
		if (source != null)
			source.OnFocusLose();
		
		UIMenu target = GetMenu<UIMenu>(menuType, true);
		if (target != null)
			target.OnFocusGain();
		
		m_FocusedMenu = menuType;
	}

	public Task ExceptionAsync(Exception _Exception)
	{
		if (_Exception == null)
			return Task.CompletedTask;
		
		Exception exception = _Exception.GetBaseException();
		
		return ErrorAsync(
			"exception",
			m_Localization.Get("COMMON_ERROR_TITLE"),
			exception.Message
		);
	}

	public Task ErrorAsync(string _ID)
	{
		UIErrorMenu errorMenu = GetMenu<UIErrorMenu>();
		
		if (errorMenu == null)
			return Task.CompletedTask;
		
		errorMenu.Setup(_ID);
		
		return errorMenu.ShowAsync();
	}

	public Task ErrorAsync(string _ID, string _Title, string _Message)
	{
		UIErrorMenu errorMenu = GetMenu<UIErrorMenu>();
		
		if (errorMenu == null)
			return Task.CompletedTask;
		
		errorMenu.Setup(_ID, _Title, _Message);
		
		return errorMenu.ShowAsync();
	}

	public Task ConfirmLocalizedAsync(string _ID, string _TitleKey, string _MessageKey, Action _Confirm, Action _Cancel = null)
	{
		return ConfirmAsync(
			_ID,
			m_Localization.Get(_TitleKey),
			m_Localization.Get(_MessageKey),
			_Confirm,
			_Cancel
		);
	}

	public Task ConfirmAsync(string _ID, string _Title, string _Message, Action _Confirm, Action _Cancel = null)
	{
		UIConfirmMenu confirmMenu = GetMenu<UIConfirmMenu>();
		
		if (confirmMenu == null)
			return Task.CompletedTask;
		
		confirmMenu.Setup(_ID, _Title, _Message, _Confirm, _Cancel);
		
		return confirmMenu.ShowAsync();
	}

	public Task<bool> ConfirmAsync(string _ID, string _Title, string _Message)
	{
		UIConfirmMenu confirmMenu = GetMenu<UIConfirmMenu>();
		
		if (confirmMenu == null)
			return Task.FromResult(false);
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		confirmMenu.Setup(
			_ID,
			_Title,
			_Message,
			() => source.TrySetResult(true),
			() => source.TrySetResult(false)
		);
		
		confirmMenu.Show();
		
		return source.Task;
	}

	public Task<bool> CoinsAsync(string _ID, string _Title, string _Message, long _Coins)
	{
		UICoinsMenu coinsMenu = GetMenu<UICoinsMenu>();
		
		if (coinsMenu == null)
			return Task.FromResult(false);
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		coinsMenu.Setup(
			_ID,
			_Title,
			_Message,
			_Coins,
			() => source.TrySetResult(true),
			() => source.TrySetResult(false)
		);
		
		coinsMenu.Show();
		
		return source.Task;
	}

	public Task RetryAsync(string _ID, Action _Retry = null, Action _Cancel = null)
	{
		UIRetryMenu retryMenu = GetMenu<UIRetryMenu>();
		
		if (retryMenu == null)
			return Task.CompletedTask;
		
		retryMenu.Setup(_ID, _Retry, _Cancel);
		
		return retryMenu.ShowAsync();
	}

	public Task RetryAsync(string _ID, string _Title, string _Message, Action _Retry = null, Action _Cancel = null)
	{
		UIRetryMenu retryMenu = GetMenu<UIRetryMenu>();
		
		if (retryMenu == null)
			return Task.CompletedTask;
		
		retryMenu.Setup(_ID, _Title, _Message, _Retry, _Cancel);
		
		return retryMenu.ShowAsync();
	}

	public T GetMenu<T>(bool _Cache = false) where T : UIMenu
	{
		if (!MenuPrebuild.TryGetMenuType<T>(out MenuType menuType))
		{
			Log.Error(this, "Get menu failed. Menu type '{0}' not found.", typeof(T).Name);
			return null;
		}
		
		return GetMenu<T>(menuType, _Cache);
	}

	T GetMenu<T>(MenuType _MenuType, bool _Cache = false) where T : UIMenu
	{
		if (m_MenuCache.ContainsKey(_MenuType) && m_MenuCache[_MenuType] is T menuCache)
			return menuCache;
		
		if (_Cache)
			return null;
		
		if (!m_MenuInfos.ContainsKey(_MenuType))
		{
			Log.Error(this, "Get menu failed. Menu '{0}' not found.", _MenuType);
			return null;
		}
		
		MenuInfo menuInfo = m_MenuInfos[_MenuType];
		
		if (menuInfo == null)
		{
			Log.Error(this, "Get menu failed. Menu info for '{0}' is null.", _MenuType);
			return null;
		}
		
		T prefab = Resources.Load<T>(menuInfo.Path);
		
		T menu = m_MenuFactory.Create(prefab) as T;
		
		m_MenuCache[_MenuType] = menu;
		
		if (menu == null)
			return null;
		
		menu.RectTransform.SetParent(m_Canvas.transform, false);
		
		Reorder();
		
		return menu;
	}

	public bool RemoveMenu(MenuType _MenuType)
	{
		if (!m_MenuCache.ContainsKey(_MenuType))
			return false;
		
		UIMenu menu = m_MenuCache[_MenuType];
		
		m_MenuCache.Remove(_MenuType);
		
		if (menu == null)
			return false;
		
		Object.Destroy(menu.gameObject);
		
		Reorder();
		
		return true;
	}

	public async Task<UIMenu> Show(MenuType _MenuType, bool _Instant = false)
	{
		UIMenu menu = GetMenu<UIMenu>(_MenuType);
		
		if (menu == null)
			return null;
		
		await menu.ShowAsync(_Instant);
		
		return menu;
	}

	public async Task<UIMenu> Hide(MenuType _MenuType, bool _Instant = false)
	{
		UIMenu menu = GetMenu<UIMenu>(_MenuType, true);
		
		if (menu == null)
			return null;
		
		await menu.HideAsync(_Instant);
		
		return menu;
	}

	void Reorder()
	{
		foreach (MenuType menuType in m_MenuOrder)
		{
			if (!m_MenuCache.ContainsKey(menuType))
				continue;
			
			UIMenu menu = m_MenuCache[menuType];
			
			if (menu == null)
				continue;
			
			menu.RectTransform.SetAsFirstSibling();
		}
	}
}
