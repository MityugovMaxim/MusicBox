using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Storage;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilder
{
	[MenuItem("Tools/Create asset bundles")]
	public static async void CreateAssetBundles()
	{
		EditorUtility.DisplayProgressBar("Creating asset bundles...", "Fetching level IDs", 0.1f);
		
		await ProcessLevels();
		
		EditorUtility.ClearProgressBar();
		
		string path = Path.Combine(Application.dataPath, "AssetBundles");
		
		EditorUtility.DisplayProgressBar("Creating asset bundles...", "Building asset bundles", 0.1f);
		
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		
		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.iOS);
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		await UploadAssetBundles(path);
	}

	static async Task ProcessLevels()
	{
		await FirebaseAdmin.Login();
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child("levels");
		
		DataSnapshot data = await reference.GetValueAsync();
		
		string[] levelIDs = data.Children.Select(_Snapshot => _Snapshot.Key).ToArray();
		
		foreach (string levelID in levelIDs)
		{
			string tracksPath = $"Assets/Levels/{levelID}/Tracks";
			
			AssetImporter importer = AssetImporter.GetAtPath(tracksPath);
			
			importer.assetBundleName = $"level.{levelID}.unity3d";
			
			importer.SaveAndReimport();
		}
	}

	static async Task UploadAssetBundles(string _Path)
	{
		await FirebaseAdmin.Login();
		
		string[] paths = Directory.GetFiles(_Path, "*.asset");
		
		foreach (string path in paths)
			await FirebaseStorage.DefaultInstance.RootReference.Child("Levels").PutFileAsync(path);
	}
}