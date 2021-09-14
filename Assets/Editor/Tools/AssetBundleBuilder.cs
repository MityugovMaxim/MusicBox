using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilder
{
	[MenuItem("AssetBundle/Create")]
	public static void CreateAssetBundles()
	{
		Levels();
		
		string path = Path.Combine(Application.dataPath, "AssetBundles");
		
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.iOS);
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	static void Levels()
	{
		const string registryPath = "Assets/Database/level_registry.asset";
		
		LevelRegistry levelRegistry = AssetDatabase.LoadAssetAtPath<LevelRegistry>(registryPath);
		
		foreach (LevelInfo levelInfo in levelRegistry)
		{
			string tracksPath = $"Assets/Levels/{levelInfo.ID}/Tracks";
			
			AssetImporter importer = AssetImporter.GetAtPath(tracksPath);
			
			importer.assetBundleName = $"level.{levelInfo.ID}.unity3d";
			
			importer.SaveAndReimport();
		}
	}
}