using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIProductThumbnail : UIEntity
{
	[SerializeField] UIRemoteImage m_Image;

	StorageProcessor m_StorageProcessor;

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void Setup(string _ProductID)
	{
		Task<Sprite> spriteTask = m_StorageProcessor.LoadProductThumbnail(_ProductID);
		
		m_Image.Load(spriteTask);
	}
}