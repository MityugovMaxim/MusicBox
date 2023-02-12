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
	readonly UIMenu.Factory                 m_Factory;
	readonly Dictionary<MenuType, UIMenu>   m_Cache;
	readonly Dictionary<MenuType, MenuInfo> m_Data;
	readonly Dictionary<MenuType, MenuMode> m_Modes;
	readonly Dictionary<MenuType, int>      m_Orders;
	readonly SortedList<int, MenuType>      m_Menus;

	[Inject]
	public MenuProcessor(
		Localization   _Localization,
		Canvas         _Canvas,
		MenuInfo[]     _MenuInfos,
		UIMenu.Factory _Factory
	)
	{
		m_Localization = _Localization;
		m_Canvas       = _Canvas;
		m_Factory      = _Factory;
		m_Cache        = new Dictionary<MenuType, UIMenu>();
		m_Menus        = new SortedList<int, MenuType>();
		m_Data         = _MenuInfos.ToDictionary(_MenuInfo => _MenuInfo.Type, _MenuInfo => _MenuInfo);
		m_Modes        = _MenuInfos.ToDictionary(_MenuInfo => _MenuInfo.Type, _MenuInfo => _MenuInfo.Mode);
		m_Orders       = _MenuInfos.Select((_MenuInfo, _Index) => new KeyValuePair<MenuType, int>(_MenuInfo.Type, _MenuInfos.Length - _Index)).ToDictionary(_Entry => _Entry.Key, _Entry => _Entry.Value);
	}

	async void IInitializable.Initialize()
	{
		await Show(MenuType.SplashMenu, true);
	}

	MenuMode GetMenuMode(MenuType _MenuType) => m_Modes.TryGetValue(_MenuType, out MenuMode menuMode) ? menuMode : MenuMode.Menu;

	int GetMenuOrder(MenuType _MenuType) => m_Orders.TryGetValue(_MenuType, out int menuOrder) ? menuOrder : int.MinValue;

	MenuType GetMenuType(UIMenu _Menu) => MenuPrebuild.TryGetMenuType(_Menu.GetType(), out MenuType menuType) ? menuType : MenuType.Invalid;

	public void Register(UIMenu _Menu)
	{
		if (_Menu == null)
			return;
		
		MenuType menuType  = GetMenuType(_Menu);
		MenuMode menuMode  = GetMenuMode(menuType);
		int      menuOrder = GetMenuOrder(menuType);
		
		MenuType current = GetLastMenuType(menuMode);
		
		m_Menus.Add(menuOrder, menuType);
		
		ProcessOrder();
		
		ProcessFocus(menuType, current);
	}

	public void Unregister(UIMenu _Menu)
	{
		MenuType menuType  = GetMenuType(_Menu);
		MenuMode menuMode  = GetMenuMode(menuType);
		int      menuOrder = GetMenuOrder(menuType);
		
		m_Menus.Remove(menuOrder);
		
		MenuType current = GetLastMenuType(menuMode);
		
		ProcessOrder();
		
		RestoreFocus(menuType, current);
	}

	void ProcessFocus(MenuType _SourceType, MenuType _TargetType)
	{
		if (_SourceType == _TargetType)
			return;
		
		int sourceOrder = GetMenuOrder(_SourceType);
		int targetOrder = GetMenuOrder(_TargetType);
		
		if (sourceOrder < targetOrder)
			return;
		
		UIMenu sourceMenu = GetMenu<UIMenu>(_SourceType);
		if (sourceMenu != null)
			sourceMenu.OnFocusGain();
		
		UIMenu targetMenu = GetMenu<UIMenu>(_TargetType);
		if (targetMenu != null)
			targetMenu.OnFocusLose();
	}

	void RestoreFocus(MenuType _SourceType, MenuType _TargetType)
	{
		if (_SourceType == _TargetType)
			return;
		
		int sourceOrder = GetMenuOrder(_SourceType);
		int targetOrder = GetMenuOrder(_TargetType);
		
		if (sourceOrder < targetOrder)
			return;
		
		UIMenu sourceMenu = GetMenu<UIMenu>(_SourceType);
		if (sourceMenu != null)
			sourceMenu.OnFocusLose();
		
		UIMenu targetMenu = GetMenu<UIMenu>(_TargetType);
		if (targetMenu != null)
			targetMenu.OnFocusGain();
	}

	MenuType GetLastMenuType(MenuMode _MenuMode)
	{
		return m_Menus.Values.LastOrDefault(_MenuType => GetMenuMode(_MenuType) == _MenuMode);
	}

	void ProcessOrder()
	{
		foreach (MenuType menuType in m_Menus.Values)
		{
			if (m_Cache.TryGetValue(menuType, out UIMenu menu) && menu == null)
				menu.RectTransform.SetAsLastSibling();
		}
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

	public Task<bool> ConfirmAsync(string _ID, string _Title, string _Message)
	{
		UIConfirmMenu confirmMenu = GetMenu<UIConfirmMenu>();
		
		if (confirmMenu == null)
			return Task.FromResult(false);
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		confirmMenu.Setup(
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
		UIRetryDialog retryDialog = GetMenu<UIRetryDialog>();
		
		if (retryDialog == null)
			return Task.CompletedTask;
		
		retryDialog.Setup(_ID, _Retry, _Cancel);
		
		return retryDialog.ShowAsync();
	}

	public Task RetryAsync(string _ID, string _Title, string _Message, Action _Retry = null, Action _Cancel = null)
	{
		UIRetryDialog retryDialog = GetMenu<UIRetryDialog>();
		
		if (retryDialog == null)
			return Task.CompletedTask;
		
		retryDialog.Setup(_ID, _Title, _Message, _Retry, _Cancel);
		
		return retryDialog.ShowAsync();
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
		if (_MenuType == MenuType.Invalid)
			return null;
		
		if (m_Cache.ContainsKey(_MenuType) && m_Cache[_MenuType] is T menuCache)
			return menuCache;
		
		if (_Cache)
			return null;
		
		if (!m_Data.ContainsKey(_MenuType))
		{
			Log.Error(this, "Get menu failed. Menu '{0}' not found.", _MenuType);
			return null;
		}
		
		MenuInfo menuInfo = m_Data[_MenuType];
		
		if (menuInfo == null)
		{
			Log.Error(this, "Get menu failed. Menu info for '{0}' is null.", _MenuType);
			return null;
		}
		
		T prefab = Resources.Load<T>(menuInfo.Path);
		
		T menu = m_Factory.Create(prefab) as T;
		
		m_Cache[_MenuType] = menu;
		
		if (menu == null)
			return null;
		
		menu.RectTransform.SetParent(m_Canvas.transform, false);
		
		return menu;
	}

	public bool RemoveMenu(MenuType _MenuType)
	{
		if (!m_Cache.ContainsKey(_MenuType))
			return false;
		
		UIMenu menu = m_Cache[_MenuType];
		
		m_Cache.Remove(_MenuType);
		
		if (menu == null)
			return false;
		
		Object.Destroy(menu.gameObject);
		
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
}
