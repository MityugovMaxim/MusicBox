using UnityEngine;
using Zenject;

public class UISongQRCode : UIGroup
{
	[SerializeField] UISongImage m_Image;
	[SerializeField] UIQRCode    m_QRCode;
	[SerializeField] UIGroup     m_Loader;

	[Inject] SongsProcessor m_SongsProcessor;
	[Inject] UrlProcessor   m_UrlProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
	}

	protected override async void OnShowStarted()
	{
		string hash = m_SongsProcessor.GetSongHash(m_SongID);
		
		m_Loader.Show(true);
		
		string url = await m_UrlProcessor.GenerateDynamicLink(hash);
		
		GUIUtility.systemCopyBuffer = url;
		
		m_QRCode.Message = url;
		
		m_Loader.Hide();
	}
}
