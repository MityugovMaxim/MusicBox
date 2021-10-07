using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class MenuPrebuildProcessor : IPreprocessBuildWithReport
{
	public int callbackOrder => 0;

	public void OnPreprocessBuild(BuildReport _Report)
	{
		const string prebuildStart = "/// PREBUILD_START";
		const string prebuildEnd   = "/// PREBUILD_END";
		
		string path = AssetDatabase
			.FindAssets("t:Script MenuPrebuild")
			.Select(AssetDatabase.GUIDToAssetPath)
			.FirstOrDefault(_Path => Path.GetFileNameWithoutExtension(_Path) == "MenuPrebuild");
		
		if (string.IsNullOrEmpty(path))
			return;
		
		MenuPrebuild.Initialize();
		
		TextAsset script = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
		
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
				foreach (var entry in MenuPrebuild.GetMenuTypes())
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
		
		File.WriteAllText(path, data.ToString().TrimEnd('\n'));
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}

