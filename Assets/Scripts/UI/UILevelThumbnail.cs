using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UILevelThumbnail : UIEntity
{
	[SerializeField] UIRemoteImage m_Image;

	StorageProcessor m_StorageProcessor;

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void Setup(string _LevelID)
	{
		Task<Sprite> spriteTask = m_StorageProcessor.LoadLevelThumbnail(_LevelID);
		
		m_Image.Load(spriteTask);
	}
}
