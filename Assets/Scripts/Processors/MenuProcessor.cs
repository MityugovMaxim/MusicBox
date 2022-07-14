using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class MenuProcessor : IInitializable
{
	readonly LocalizationProcessor          m_LocalizationProcessor;
	readonly Canvas                         m_Canvas;
	readonly UIMenu.Factory                 m_MenuFactory;
	readonly Dictionary<MenuType, UIMenu>   m_MenuCache;
	readonly Dictionary<MenuType, MenuInfo> m_MenuInfos;
	readonly List<MenuType>                 m_MenuOrder;

	[Inject]
	public MenuProcessor(
		LocalizationProcessor _LocalizationProcessor,
		Canvas                _Canvas,
		MenuInfo[]            _MenuInfos,
		UIMenu.Factory        _MenuFactory
	)
	{
		m_LocalizationProcessor = _LocalizationProcessor;
		m_Canvas                = _Canvas;
		m_MenuFactory           = _MenuFactory;
		m_MenuCache             = new Dictionary<MenuType, UIMenu>();
		m_MenuInfos             = _MenuInfos.ToDictionary(_MenuInfo => _MenuInfo.Type, _MenuInfo => _MenuInfo);
		m_MenuOrder             = _MenuInfos.Select(_MenuInfo => _MenuInfo.Type).ToList();
	}

	async void IInitializable.Initialize()
	{
		await Show(MenuType.SplashMenu, true);
	}

	public async void ErrorLocalized(string _ID, string _Place, string _TitleKey, string _MessageKey)
	{
		await ErrorLocalizedAsync(_ID, _Place, _TitleKey, _MessageKey);
	}

	public async void Error(string _ID, string _Place, string _Title, string _Message)
	{
		await ErrorAsync(_ID, _Place, _Title, _Message);
	}

	public Task ErrorLocalizedAsync(string _ID, string _Place, string _TitleKey, string _MessageKey)
	{
		return ErrorAsync(
			_ID,
			_Place,
			m_LocalizationProcessor.Get(_TitleKey),
			m_LocalizationProcessor.Get(_MessageKey)
		);
	}

	public Task ErrorAsync(string _ID, string _Place, string _Title, string _Message)
	{
		UIErrorMenu errorMenu = GetMenu<UIErrorMenu>();
		
		if (errorMenu != null)
			errorMenu.Setup(_ID, _Place, _Title, _Message);
		
		return Show(MenuType.ErrorMenu);
	}

	public Task ConfirmLocalizedAsync(string _ID, string _TitleKey, string _MessageKey, Action _Confirm, Action _Cancel = null)
	{
		return ConfirmAsync(
			_ID,
			m_LocalizationProcessor.Get(_TitleKey),
			m_LocalizationProcessor.Get(_MessageKey),
			_Confirm,
			_Cancel
		);
	}

	public Task ConfirmAsync(string _ID, string _Title, string _Message, Action _Confirm, Action _Cancel = null)
	{
		UIConfirmMenu confirmMenu = GetMenu<UIConfirmMenu>();
		
		if (confirmMenu != null)
			confirmMenu.Setup(_ID, _Title, _Message, _Confirm, _Cancel);
		
		return Show(MenuType.ConfirmMenu);
	}

	public Task<bool> ConfirmAsync(string _ID, string _Title, string _Message)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		UIConfirmMenu confirmMenu = GetMenu<UIConfirmMenu>();
		
		if (confirmMenu == null)
			return Task.FromResult(false);
		
		confirmMenu.Setup(
			_ID,
			_Title,
			_Message,
			() => completionSource.TrySetResult(true),
			() => completionSource.TrySetResult(false)
		);
		
		confirmMenu.Show();
		
		return completionSource.Task;
	}

	public async void RetryLocalized(string _ID, string _Place, string _TitleKey, string _MessageKey, Action _Retry, Action _Cancel = null)
	{
		await RetryLocalizedAsync(_ID, _Place, _TitleKey, _MessageKey, _Retry, _Cancel);
	}

	public async void Retry(string _ID, string _Place, string _Title, string _Message, Action _Retry, Action _Cancel = null)
	{
		await RetryAsync(_ID, _Place, _Title, _Message, _Retry, _Cancel);
	}

	public Task RetryLocalizedAsync(string _ID, string _Place, string _TitleKey, string _MessageKey, Action _Retry, Action _Cancel = null)
	{
		return RetryAsync(
			_ID,
			_Place,
			m_LocalizationProcessor.Get(_TitleKey),
			m_LocalizationProcessor.Get(_MessageKey),
			_Retry,
			_Cancel
		);
	}

	public Task RetryAsync(string _ID, string _Place, string _Title, string _Message, Action _Retry, Action _Cancel = null)
	{
		UIRetryMenu retryMenu = GetMenu<UIRetryMenu>();
		
		if (retryMenu != null)
			retryMenu.Setup(_ID, _Place, _Title, _Message, _Retry, _Cancel);
		
		return Show(MenuType.RetryMenu);
	}

	public T GetMenu<T>(bool _Cache = false) where T : UIMenu
	{
		if (!MenuPrebuild.TryGetMenuType<T>(out MenuType menuType))
		{
			Debug.LogErrorFormat("[MenuProcessor] Get menu failed. Menu type '{0}' not found.", typeof(T).Name);
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
			Debug.LogErrorFormat("[MenuProcessor] Get menu failed. Menu '{0}' not found.", _MenuType);
			return null;
		}
		
		MenuInfo menuInfo = m_MenuInfos[_MenuType];
		
		if (menuInfo == null)
		{
			Debug.LogErrorFormat("[MenuProcessor] Get menu failed. Menu info for '{0}' is null.", _MenuType);
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
