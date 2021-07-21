using System.Runtime.InteropServices;
using UnityEngine;

public class GameCenter : MonoBehaviour
{
	[DllImport("__Internal")]
	static extern void GameCenterAuth();

	public static void Auth()
	{
		GameCenterAuth();
	}
}
