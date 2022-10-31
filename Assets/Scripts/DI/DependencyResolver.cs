using Firebase;
using UnityEngine;
using Zenject;

public class DependencyResolver : MonoBehaviour
{
	[SerializeField] SceneContext m_Context;

	void Awake()
	{
		Resolve();
	}

	async void Resolve()
	{
		DependencyStatus state = await FirebaseApp.CheckDependenciesAsync();
		
		m_Context.Run();
	}
}
