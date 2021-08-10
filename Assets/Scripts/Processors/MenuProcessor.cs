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

public class MenuProcessor : IInitializable, IDisposable
{
	readonly Dictionary<MenuType, UIMenu>   m_MenuCache = new Dictionary<MenuType, UIMenu>();
	readonly Dictionary<MenuType, MenuInfo> m_MenuInfos = new Dictionary<MenuType, MenuInfo>();
	readonly List<MenuType>                 m_MenuOrder = new List<MenuType>();

	readonly SignalBus      m_SignalBus;
	readonly Canvas         m_Canvas;
	readonly UIMenu.Factory m_MenuFactory;

	[Inject]
	public MenuProcessor(
		SignalBus      _SignalBus,
		Canvas         _Canvas,
		UIMenu.Factory _MenuFactory
	)
	{
		m_SignalBus   = _SignalBus;
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
		
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		
		Show(MenuType.MainMenu, true);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		UIPauseMenu pauseMenu = GetMenu<UIPauseMenu>(MenuType.PauseMenu);
		if (pauseMenu != null)
			pauseMenu.Setup(_Signal.LevelID);
		
		Hide(MenuType.MainMenu, true);
		Hide(MenuType.LevelMenu, true);
		Hide(MenuType.ResultMenu, true);
		Hide(MenuType.TutorialMenu, true);
		
		Hide(MenuType.PauseMenu, true);
		Show(MenuType.GameMenu, true);
		
		Hide(MenuType.LoadingMenu);
	}

	void RegisterLevelFinish(LevelFinishSignal _Signal)
	{
		UIResultMenu resultMenu = GetMenu<UIResultMenu>(MenuType.ResultMenu);
		if (resultMenu != null)
			resultMenu.Setup(_Signal.LevelID);
		
		Show(MenuType.ResultMenu);
	}

	void RegisterLevelRestart(LevelRestartSignal _Signal)
	{
		Hide(MenuType.MainMenu, true);
		Hide(MenuType.LevelMenu, true);
		Hide(MenuType.TutorialMenu, true);
		
		Show(MenuType.GameMenu, true);
		
		Hide(MenuType.ResultMenu);
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
		MenuOperation menuOperation = new MenuOperation(this);
		
		if (!m_MenuCache.ContainsKey(_MenuType))
		{
			menuOperation.InvokeFinished();
			return menuOperation;
		}
		
		UIMenu menu = GetMenu<UIMenu>(_MenuType);
		
		menu.Hide(
			_Instant,
			null,
			menuOperation.InvokeFinished
		);
		
		return menuOperation;
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