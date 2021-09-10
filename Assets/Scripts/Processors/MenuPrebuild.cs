using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MenuPrebuild
{
	#if UNITY_EDITOR
	static readonly Dictionary<Type, MenuType> m_MenuTypes = new Dictionary<Type, MenuType>();
	#else
	/// PREBUILD_START
	/// PREBUILD_END
	#endif

	public static bool TryGetMenuType<T>(out MenuType _MenuType) where T : UIMenu
	{
		#if UNITY_EDITOR
		if (m_MenuTypes.Count == 0)
			Initialize();
		#endif
		return m_MenuTypes.TryGetValue(typeof(T), out _MenuType);
	}

	static void Initialize()
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

	#if UNITY_EDITOR
	public static void Generate()
	{
		const string prebuildStart = "/// PREBUILD_START";
		const string prebuildEnd   = "/// PREBUILD_END";
		
		string path = UnityEditor.AssetDatabase.FindAssets("t:Script MenuPrebuild")
			.Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
			.FirstOrDefault();
		
		if (string.IsNullOrEmpty(path))
			return;
		
		Initialize();
		
		TextAsset script = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
		
		string[] lines = script.text.Split('\n');
		
		bool remove = false;
		
		StringBuilder data = new StringBuilder();
		
		foreach (string line in lines)
		{
			if (line.Contains(prebuildStart))
			{
				remove = true;
				data.AppendLine(line);
				
				data.AppendLine("\tstatic readonly Dictionary<Type, MenuType> m_MenuTypes = new Dictionary<Type, MenuType>()");
				data.AppendLine("\t{");
				foreach (var entry in m_MenuTypes)
				{
					data.AppendFormat("\t\t{{ typeof({0}), MenuType.{1} }},", entry.Key.Name, entry.Value.ToString());
					data.AppendLine();
				}
				data.AppendLine("\t};");
			}
			else if (line.Contains(prebuildEnd))
			{
				remove = false;
				data.AppendLine(line);
			}
			else if (!remove)
			{
				data.AppendLine(line);
			}
		}
		
		File.WriteAllText(path, data.ToString());
		
		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();
	}
	#endif
}