using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class MenuPrebuildGenerator : IPreprocessBuildWithReport
{
	public int callbackOrder { get; }

	public void OnPreprocessBuild(BuildReport _Report)
	{
		const string source = "/// PREBUILD_START";
		const string target = "/// PREBUILD_END";
		const string name   = nameof(MenuPrebuild);
		
		string path = AssetDatabase
			.FindAssets($"t:Script {name}")
			.Select(AssetDatabase.GUIDToAssetPath)
			.FirstOrDefault(_Path => Path.GetFileNameWithoutExtension(_Path) == name);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		string text = File.ReadAllText(path);
		
		int sourceIndex = text.IndexOf(source, StringComparison.Ordinal);
		int targetIndex = text.IndexOf(target, StringComparison.Ordinal);
		
		if (sourceIndex < 0 || targetIndex < 0)
			return;
		
		MenuPrebuild.Initialize();
		
		KeyValuePair<Type, MenuType>[] data = MenuPrebuild.GetMenuTypes();
		
		StringBuilder builder = new StringBuilder();
		builder.AppendLine(source);
		builder.AppendLine("\t{");
		foreach (var entry in data)
			builder.AppendFormat("\t\t{{ typeof({0}), MenuType.{1} }},", entry.Key.Name, entry.Value).AppendLine();
		builder.AppendLine("\t};");
		builder.Append("\t").Append(target);
		
		string prebuild = builder.ToString();
		
		string origin = text.Substring(sourceIndex, targetIndex - sourceIndex + target.Length);
		
		File.WriteAllText(path, text.Replace(origin, prebuild));
	}
}