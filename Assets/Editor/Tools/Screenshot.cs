using System;
using UnityEditor;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
	[MenuItem("Tools/Capture screenshot")]
	public static void CaptureScreenshot()
	{
		DateTime date = DateTime.Now;
		
		ScreenCapture.CaptureScreenshot($"screenshot_{date.Day}_{date.Month}_{date.Year}_{date.Hour}_{date.Minute}_{date.Second}.png");
	}
}
