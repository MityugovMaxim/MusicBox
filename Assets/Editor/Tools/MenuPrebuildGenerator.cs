using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public abstract class PrebuildGenerator : IPreprocessBuildWithReport
{
	const string SOURCE_ANCHOR = "//// PREBUILD_START";
	const string TARGET_ANCHOR = "//// PREBUILD_END";

	protected abstract string Name { get; }

	public int callbackOrder { get; }

	protected abstract string Prebuild();

	protected static string Find(string _Name)
	{
		return AssetDatabase
			.FindAssets($"t:Script {_Name}")
			.Select(AssetDatabase.GUIDToAssetPath)
			.FirstOrDefault(_Path => Path.GetFileNameWithoutExtension(_Path) == _Name);
	}

	protected static void Insert(string _Path, string _Prebuild)
	{
		string text = File.ReadAllText(_Path);
		
		int sourceIndex = text.IndexOf(SOURCE_ANCHOR, StringComparison.Ordinal);
		int targetIndex = text.IndexOf(TARGET_ANCHOR, StringComparison.Ordinal);
		
		if (sourceIndex < 0 || targetIndex < 0)
			return;
		
		string origin = text.Substring(sourceIndex, targetIndex - sourceIndex + TARGET_ANCHOR.Length);
		
		File.WriteAllText(_Path, text.Replace(origin, _Prebuild));
	}

	protected static string Wrap(string _Prebuild)
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine(SOURCE_ANCHOR);
		builder.AppendLine(_Prebuild);
		builder.Append("\t").Append(TARGET_ANCHOR);
		return builder.ToString();
	}

	public void OnPreprocessBuild(BuildReport _Report)
	{
		string path = Find(Name);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		string prebuild = Prebuild();
		
		Insert(path, Wrap(prebuild));
	}
}

public class SnapshotPrebuildGenerator : PrebuildGenerator
{
	[MenuItem("Prebuild/Snapshots")]
	public static void Generate()
	{
		string path = Find(NAME);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		string prebuild = PrebuildInternal();
		
		Insert(path, Wrap(prebuild));
	}

	const string NAME = nameof(SnapshotPrebuild);

	protected override string Name => NAME;

	protected override string Prebuild() => PrebuildInternal();

	static string PrebuildInternal()
	{
		Type[] data = SnapshotPrebuild.GetSnapshotTypes();
		
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("\t{");
		foreach (Type type in data)
			builder.AppendFormat("\t\t{{ typeof({0}), _Data => new {0}(_Data) }},", type).AppendLine();
		builder.AppendLine("\t};");
		
		return builder.ToString();
	}
}

public class MenuPrebuildGenerator : PrebuildGenerator
{
	[MenuItem("Prebuild/Menus")]
	public static void Generate()
	{
		string path = Find(NAME);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		string prebuild = PrebuildInternal();
		
		Insert(path, Wrap(prebuild));
	}

	const string NAME = nameof(MenuPrebuild);

	protected override string Name => NAME;

	protected override string Prebuild() => PrebuildInternal();

	static string PrebuildInternal()
	{
		KeyValuePair<Type, MenuType>[] data = MenuPrebuild.GetMenuTypes();
		
		StringBuilder builder = new StringBuilder();
		
		builder.AppendLine("\t{");
		foreach (var entry in data)
			builder.AppendFormat("\t\t{{ typeof({0}), MenuType.{1} }},", entry.Key.Name, entry.Value).AppendLine();
		builder.AppendLine("\t};");
		
		return builder.ToString();
	}
}
