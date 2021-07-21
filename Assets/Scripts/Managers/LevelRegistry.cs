using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "Level Info", menuName = "Level Info")]
public class LevelInfo : ScriptableObject
{
	public string Title  => m_Title;
	public string Artist => m_Artist;

	[SerializeField] string         m_Title;
	[SerializeField] string         m_Artist;
	[SerializeField] AssetReference m_Level;
	[SerializeField] AssetReference m_Preview;

	public void InstantiateLevel(Transform _Parent, Action<Level> _Callback)
	{
		if (m_Level == null)
		{
			Debug.LogErrorFormat(this, "[LevelInfo] Instantiate level failed. Level asset is not assigned at '{0} - {1}'", Artist, Title);
			return;
		}
		
		AsyncOperationHandle<GameObject> levelLoader = m_Level.InstantiateAsync(_Parent);
		
		levelLoader.Completed += _LevelLoader =>
		{
			GameObject levelObject = levelLoader.Result;
			
			if (levelObject == null)
			{
				Debug.LogErrorFormat(this, "[LevelInfo] Instantiate level failed. Level asset not found at '{0} - {1}'", Artist, Title);
				return;
			}
			
			Level level = levelObject.GetComponent<Level>();
			
			_Callback?.Invoke(level);
		};
	}

	public void InstantiatePreview(Transform _Parent, Action<Preview> _Callback = null)
	{
		if (m_Preview == null)
		{
			Debug.LogErrorFormat(this, "[LevelInfo] Instantiate preview failed. Preview asset is not assigned at '{0} - {1}'", Artist, Title);
			return;
		}
		
		AsyncOperationHandle<GameObject> previewLoader = m_Preview.InstantiateAsync(_Parent);
		
		previewLoader.Completed += _PreviewLoader =>
		{
			GameObject levelObject = previewLoader.Result;
			
			if (levelObject == null)
			{
				Debug.LogErrorFormat(this, "[LevelInfo] Instantiate preview failed. Preview asset not found at '{0} - {1}'", Artist, Title);
				return;
			}
			
			Preview level = levelObject.GetComponent<Preview>();
			
			_Callback?.Invoke(level);
		};
	}
}