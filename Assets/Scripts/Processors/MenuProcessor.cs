using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MenuProcessor : IInitializable
{
	readonly Dictionary<MenuType, UIMenu>   m_MenuCache = new Dictionary<MenuType, UIMenu>();
	readonly Dictionary<MenuType, MenuInfo> m_MenuInfos = new Dictionary<MenuType, MenuInfo>();
	readonly List<MenuType>                 m_MenuOrder = new List<MenuType>();

	readonly Canvas         m_Canvas;
	readonly UIMenu.Factory m_MenuFactory;

	[Inject]
	public MenuProcessor(
		Canvas         _Canvas,
		UIMenu.Factory _MenuFactory
	)
	{
		m_Canvas      = _Canvas;
		m_MenuFactory = _MenuFactory;
	}

	void IInitializable.Initialize()
	{
		MenuRegistry menuRegistry = Registry.Load<MenuRegistry>("menu_registry");
		
		if (menuRegistry != null)
		{
			foreach (MenuInfo menuInfo in menuRegistry)
			{
				if (menuInfo == null || !menuInfo.Active)
					continue;
				
				m_MenuInfos[menuInfo.Type] = menuInfo;
				m_MenuOrder.Add(menuInfo.Type);
			}
		}
		
		Show(MenuType.LoginMenu, true);
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

	public Task<UIMenu> Show(MenuType _MenuType, bool _Instant = false)
	{
		UIMenu menu = GetMenu<UIMenu>(_MenuType);
		
		TaskCompletionSource<UIMenu> completionSource = new TaskCompletionSource<UIMenu>();
		
		menu.Show(
			_Instant,
			null,
			() => completionSource.TrySetResult(menu)
		);
		
		return completionSource.Task;
	}

	public Task<UIMenu> Hide(MenuType _MenuType, bool _Instant = false)
	{
		TaskCompletionSource<UIMenu> completionSource = new TaskCompletionSource<UIMenu>();
		
		UIMenu menu = GetMenu<UIMenu>(_MenuType, true);
		
		if (menu == null)
		{
			completionSource.TrySetResult(null);
			return completionSource.Task;
		}
		
		menu.Hide(
			_Instant,
			null,
			() => completionSource.TrySetResult(menu)
		);
		
		return completionSource.Task;
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