using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MenuProcessor : IInitializable
{
	readonly Canvas                         m_Canvas;
	readonly UIMenu.Factory                 m_MenuFactory;
	readonly Dictionary<MenuType, UIMenu>   m_MenuCache;
	readonly Dictionary<MenuType, MenuInfo> m_MenuInfos;
	readonly List<MenuType>                 m_MenuOrder;


	[Inject]
	public MenuProcessor(
		Canvas         _Canvas,
		MenuInfo[]     _MenuInfos,
		UIMenu.Factory _MenuFactory
	)
	{
		m_Canvas      = _Canvas;
		m_MenuFactory = _MenuFactory;
		m_MenuCache   = new Dictionary<MenuType, UIMenu>();
		m_MenuInfos   = _MenuInfos.ToDictionary(_MenuInfo => _MenuInfo.Type, _MenuInfo => _MenuInfo);
		m_MenuOrder   = _MenuInfos.Select(_MenuInfo => _MenuInfo.Type).ToList();
	}

	async void IInitializable.Initialize()
	{
		await Show(MenuType.LoginMenu, true);
		
		UILoginMenu loginMenu = GetMenu<UILoginMenu>();
		if (loginMenu != null)
			await loginMenu.Login();
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
