#if UNITY_ANDROID
using Firebase;
#endif
using UnityEngine;
using Zenject;

public class DependencyResolver : MonoBehaviour
{
	[SerializeField] SceneContext m_Context;

	void Awake()
	{
		Resolve();
	}

	#if UNITY_ANDROID
	async void Resolve()
	{
		await FirebaseApp.CheckDependenciesAsync();
		
		m_Context.Run();
	}
	#else
	void Resolve()
	{
		m_Context.Run();
	}
	#endif
}
