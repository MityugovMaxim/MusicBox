using UnityEngine;
using Zenject;

public class UISongQRCode : UIGroup
{
	[SerializeField] UISongImage m_Image;
	[SerializeField] UIQRCode    m_QRCode;

	[Inject] SongsProcessor m_SongsProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		
		string songHash = m_SongsProcessor.GetSongHash(m_SongID);
		
		string url = $"audiobox://{songHash}";
		
		GUIUtility.systemCopyBuffer = url;
		
		m_QRCode.Message = url;
	}
}