using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UINewsItem : UIGroupLayout
{
	[Preserve]
	public class Pool : MonoMemoryPool<UINewsItem> { }

	[SerializeField] UIRemoteImage m_Image;
	[SerializeField] TMP_Text      m_Title;
	[SerializeField] TMP_Text      m_Text;

	NewsProcessor    m_NewsProcessor;
	StorageProcessor m_StorageProcessor;
	UrlProcessor     m_UrlProcessor;

	string m_NewsID;
	string m_URL;

	[Inject]
	public void Construct(
		NewsProcessor    _NewsProcessor,
		StorageProcessor _StorageProcessor,
		UrlProcessor     _UrlProcessor
	)
	{
		m_NewsProcessor    = _NewsProcessor;
		m_StorageProcessor = _StorageProcessor;
		m_UrlProcessor     = _UrlProcessor;
	}

	public void Setup(string _NewsID)
	{
		m_NewsID = _NewsID;
		
		m_Image.Load(m_StorageProcessor.LoadNewsThumbnail(m_NewsID));
		
		m_Title.text = m_NewsProcessor.GetTitle(m_NewsID);
		m_Text.text  = m_NewsProcessor.GetText(m_NewsID);
		m_URL        = m_NewsProcessor.GetURL(m_NewsID);
	}

	public async void Open()
	{
		await m_UrlProcessor.ProcessURL(m_URL);
	}
}