using System.IO;
using UnityEditor;
using UnityEngine;

public static class ScreenshotTool
{
	[MenuItem("Tools/Screenshot")]
	public static void Screenshot()
	{
		string path = Path.Combine(Application.dataPath, "screenshot.jpg");
		
		path = AssetDatabase.GenerateUniqueAssetPath(path);
		
		ScreenCapture.CaptureScreenshot(path);
	}
}