using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class MenuPrebuild
{
	static readonly Dictionary<Type, MenuType> m_MenuTypes = new Dictionary<Type, MenuType>()
	//// PREBUILD_START
	{
		{ typeof(UIAdminMenu), MenuType.AdminMenu },
		{ typeof(UIBannerMenu), MenuType.BannerMenu },
		{ typeof(UIBlockMenu), MenuType.BlockMenu },
		{ typeof(UIChestConfirmMenu), MenuType.ChestConfirmMenu },
		{ typeof(UIChestMenu), MenuType.ChestMenu },
		{ typeof(UICoinsMenu), MenuType.CoinsMenu },
		{ typeof(UIColorsEditMenu), MenuType.ColorsEditMenu },
		{ typeof(UIConfirmMenu), MenuType.ConfirmMenu },
		{ typeof(UIConsentMenu), MenuType.ConsentMenu },
		{ typeof(UIErrorMenu), MenuType.ErrorMenu },
		{ typeof(UIGameMenu), MenuType.GameMenu },
		{ typeof(UILanguageMenu), MenuType.LanguageMenu },
		{ typeof(UILatencyMenu), MenuType.LatencyMenu },
		{ typeof(UILoadingMenu), MenuType.LoadingMenu },
		{ typeof(UILoginMenu), MenuType.LoginMenu },
		{ typeof(UIMainMenu), MenuType.MainMenu },
		{ typeof(UIMapMenu), MenuType.MapMenu },
		{ typeof(UIMapsMenu), MenuType.MapsMenu },
		{ typeof(UIPauseMenu), MenuType.PauseMenu },
		{ typeof(UIPermissionMenu), MenuType.PermissionMenu },
		{ typeof(UIProcessingMenu), MenuType.ProcessingMenu },
		{ typeof(UIProductMenu), MenuType.ProductMenu },
		{ typeof(UIProfileMenu), MenuType.ProfileMenu },
		{ typeof(UIResultMenu), MenuType.ResultMenu },
		{ typeof(UIRetryDialog), MenuType.RetryMenu },
		{ typeof(UIReviewMenu), MenuType.ReviewMenu },
		{ typeof(UIReviveMenu), MenuType.ReviveMenu },
		{ typeof(UISongMenu), MenuType.SongMenu },
		{ typeof(UISplashMenu), MenuType.SplashMenu },
		{ typeof(UITransitionMenu), MenuType.TransitionMenu },
		{ typeof(UITutorialMenu), MenuType.TutorialMenu },
		{ typeof(UIVideoMenu), MenuType.VideoMenu },
	};

	//// PREBUILD_END

	public static bool TryGetMenuType<T>(out MenuType _MenuType) where T : UIMenu
	{
		return m_MenuTypes.TryGetValue(typeof(T), out _MenuType);
	}

	public static bool TryGetMenuType(Type _Type, out MenuType _MenuType)
	{
		return m_MenuTypes.TryGetValue(_Type, out _MenuType);
	}

	public static KeyValuePair<Type, MenuType>[] GetMenuTypes()
	{
		Dictionary<Type, MenuType> types = new Dictionary<Type, MenuType>();
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (Type type in assembly.GetTypes())
			{
				MenuAttribute attribute = type.GetCustomAttribute<MenuAttribute>();
				
				if (attribute == null)
					continue;
				
				types[type] = attribute.MenuType;
			}
		}
		return types.ToArray();
	}
}
