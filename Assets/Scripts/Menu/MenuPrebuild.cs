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
	static readonly Dictionary<Type, MenuType> m_MenuTypes = new Dictionary<Type, MenuType>()
	/// PREBUILD_START
	{
		{ typeof(UIBannerMenu), MenuType.BannerMenu },
		{ typeof(UIBlockMenu), MenuType.BlockMenu },
		{ typeof(UIColorMenu), MenuType.ColorMenu },
		{ typeof(UIConfirmMenu), MenuType.ConfirmMenu },
		{ typeof(UIErrorMenu), MenuType.ErrorMenu },
		{ typeof(UIGameMenu), MenuType.GameMenu },
		{ typeof(UILanguageMenu), MenuType.LanguageMenu },
		{ typeof(UILanguagesMenu), MenuType.LanguagesMenu },
		{ typeof(UILatencyMenu), MenuType.LatencyMenu },
		{ typeof(UILoadingMenu), MenuType.LoadingMenu },
		{ typeof(UILocalizationEditMenu), MenuType.LocalizationEditMenu },
		{ typeof(UILocalizationMenu), MenuType.LocalizationMenu },
		{ typeof(UILoginMenu), MenuType.LoginMenu },
		{ typeof(UIMainMenu), MenuType.MainMenu },
		{ typeof(UIMapsMenu), MenuType.MapsMenu },
		{ typeof(UIPauseMenu), MenuType.PauseMenu },
		{ typeof(UIProcessingMenu), MenuType.ProcessingMenu },
		{ typeof(UIProductMenu), MenuType.ProductMenu },
		{ typeof(UIResultMenu), MenuType.ResultMenu },
		{ typeof(UIRetryMenu), MenuType.RetryMenu },
		{ typeof(UIReviveMenu), MenuType.ReviveMenu },
		{ typeof(UISetupMenu), MenuType.SetupMenu },
		{ typeof(UISnapshotMenu), MenuType.SnapshotMenu },
		{ typeof(UISnapshotsMenu), MenuType.SnapshotsMenu },
		{ typeof(UISocialMenu), MenuType.SocialMenu },
		{ typeof(UISongEditMenu), MenuType.SongEditMenu },
		{ typeof(UISongMenu), MenuType.SongMenu },
		{ typeof(UISplashMenu), MenuType.SplashMenu },
		{ typeof(UITutorialMenu), MenuType.TutorialMenu },
		{ typeof(UIVideoMenu), MenuType.VideoMenu },
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