using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class MenuPrebuild
{
	#if UNITY_EDITOR
	static readonly Dictionary<Type, MenuType> m_MenuTypes = new Dictionary<Type, MenuType>();
	#else
	/// PREBUILD_START
	static readonly Dictionary<Type, MenuType> m_MenuTypes = new Dictionary<Type, MenuType>()
	{
		{ typeof(UIBlockMenu), MenuType.BlockMenu },
		{ typeof(UIErrorMenu), MenuType.ErrorMenu },
		{ typeof(UIGameMenu), MenuType.GameMenu },
		{ typeof(UILatencyMenu), MenuType.LatencyMenu },
		{ typeof(UILevelMenu), MenuType.LevelMenu },
		{ typeof(UILoadingMenu), MenuType.LoadingMenu },
		{ typeof(UILoginMenu), MenuType.LoginMenu },
		{ typeof(UIMainMenu), MenuType.MainMenu },
		{ typeof(UIPauseMenu), MenuType.PauseMenu },
		{ typeof(UIProcessingMenu), MenuType.ProcessingMenu },
		{ typeof(UIProductMenu), MenuType.ProductMenu },
		{ typeof(UIResultMenu), MenuType.ResultMenu },
		{ typeof(UIReviveMenu), MenuType.ReviveMenu },
		{ typeof(UIRewardMenu), MenuType.RewardMenu },
	};
	/// PREBUILD_END
	#endif

	public static KeyValuePair<Type, MenuType>[] GetMenuTypes()
	{
		return m_MenuTypes.ToArray();
	}

	public static bool TryGetMenuType<T>(out MenuType _MenuType) where T : UIMenu
	{
		#if UNITY_EDITOR
		if (m_MenuTypes.Count == 0)
			Initialize();
		#endif
		return m_MenuTypes.TryGetValue(typeof(T), out _MenuType);
	}

	public static bool TryGetMenuType(Type _Type, out MenuType _MenuType)
	{
		#if UNITY_EDITOR
		if (m_MenuTypes.Count == 0)
			Initialize();
		#endif
		return m_MenuTypes.TryGetValue(_Type, out _MenuType);
	}

	public static void Initialize()
	{
		m_MenuTypes.Clear();
		
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (Type type in assembly.GetTypes())
			{
				MenuAttribute attribute = type.GetCustomAttribute<MenuAttribute>();
				
				if (attribute == null)
					continue;
				
				m_MenuTypes[type] = attribute.MenuType;
			}
		}
	}
}