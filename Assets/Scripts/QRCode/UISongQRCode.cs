using UnityEngine;
using Zenject;

public class UISongQRCode : UIGroup
{
	[SerializeField] UISongImage m_Image;
	[SerializeField] UIQRCode    m_QRCode;
	[SerializeField] UIGroup     m_Loader;

	[Inject] LinkProcessor m_LinkProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
	}

	protected override async void OnShowStarted()
	{
		m_Loader.Show(true);
		
		m_QRCode.gameObject.SetActive(false);
		
		string url = await m_LinkProcessor.GenerateSongLink(m_SongID);
		
		GUIUtility.systemCopyBuffer = url;
		
		m_QRCode.gameObject.SetActive(true);
		
		m_Image.Setup(m_SongID);
		
		m_QRCode.Message = url;
		
		m_Loader.Hide();
	}
}
