using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;

public static class FirebaseAdmin
{
	static string m_Config;

	public static async Task Login()
	{
		if (string.IsNullOrEmpty(m_Config))
		{
			string path = EditorUtility.OpenFilePanel("Select Firebase config", Application.dataPath, "json");
			
			if (string.IsNullOrEmpty(path))
				return;
			
			m_Config = File.ReadAllText(path);
		}
		
		Dictionary<string, object> service = (Dictionary<string, object>)MiniJson.JsonDecode(m_Config);
		
		string email    = service["email"] as string;
		string password = service["password"] as string;
		
		await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(
			email,
			password
		);
	}

	public static void Logout()
	{
		FirebaseAuth.DefaultInstance.SignOut();
	}
}