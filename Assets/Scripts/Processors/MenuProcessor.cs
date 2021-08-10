using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MenuOperation
{
	readonly MenuProcessor m_MenuProcessor;

	bool   m_Resolved;
	Action m_Finished;

	public MenuOperation(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public MenuOperation ThenShow(MenuType _MenuType, bool _Instant = false)
	{
		MenuOperation menuOperation = new MenuOperation(m_MenuProcessor);
		
		void Show()
		{
			UIMenu menu = m_MenuProcessor.GetMenu<UIMenu>(_MenuType);
			
			menu.Show(
				_Instant,
				null,
				menuOperation.InvokeFinished
			);
		}
		
		if (m_Resolved)
			Show();
		else
			m_Finished = Show;
		
		return menuOperation;
	}

	public MenuOperation ThenHide(MenuType _MenuType, bool _Instant = false)
	{
		MenuOperation menuOperation = new MenuOperation(m_MenuProcessor);
			
		void Hide()
		{
			UIMenu menu = m_MenuProcessor.GetMenu<UIMenu>(_MenuType);
				
			menu.Hide(
				_Instant,
				null,
				menuOperation.InvokeFinished
			);
		}
		
		if (m_Resolved)
			Hide();
		else
			m_Finished = Hide;
		
		return menuOperation;
	}

	public void InvokeFinished()
	{
		m_Resolved = true;
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}

public class MenuProcessor : IInitializable
{
	readonly Dictionary<MenuType, UIMenu>   m_MenuCache = new Dictionary<MenuType, UIMenu>();
	readonly Dictionary<MenuType, MenuInfo> m_MenuInfos = new Dictionary<MenuType, MenuInfo>();
	readonly List<MenuType>                 m_MenuOrder = new List<MenuType>();

	Canvas         m_Canvas;
	UIMenu.Factory m_MenuFactory;

	[Inject]
	public void Construct(Canvas _Canvas, UIMenu.Factory _MenuFactory)
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
		
		Show(MenuType.MainMenu, true);
	}

	public T GetMenu<T>(MenuType _MenuType) where T : UIMenu
	{
		if (m_MenuCache.ContainsKey(_MenuType) && m_MenuCache[_MenuType] is T menuCache)
			return menuCache;
		
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

	public MenuOperation Show(MenuType _MenuType, bool _Instant = false)
	{
		UIMenu menu = GetMenu<UIMenu>(_MenuType);
		
		MenuOperation menuOperation = new MenuOperation(this);
		
		menu.Show(
			_Instant,
			null,
			menuOperation.InvokeFinished
		);
		
		return menuOperation;
	}

	public MenuOperation Hide(MenuType _MenuType, bool _Instant = false)
	{
		UIMenu menu = GetMenu<UIMenu>(_MenuType);
		
		MenuOperation menuOperation = new MenuOperation(this);
		
		menu.Hide(
			_Instant,
			null,
			menuOperation.InvokeFinished
		);
		
		return menuOperation;
	}

	void Reorder()
	{
		for (var i = m_MenuOrder.Count - 1; i >= 0; i--)
		{
			MenuType menuType = m_MenuOrder[i];
			
			if (!m_MenuCache.ContainsKey(menuType))
				continue;
			
			UIMenu menu = m_MenuCache[menuType];
			
			if (menu == null)
				continue;
			
			menu.RectTransform.SetAsLastSibling();
		}
	}
}