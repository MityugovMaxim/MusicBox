using System;
using AudioBox.Logging;
using UnityEngine;

public class UIProfileImage : UIEntity
{
	[SerializeField] WebGraphic m_Image;

	public void Setup(Uri _Uri)
	{
		Log.Error(this, "---> Old photo: {0}", m_Image.Path);
		Log.Error(this, "---> New photo: {0}", _Uri?.ToString() ?? string.Empty);
		
		m_Image.Path = _Uri?.ToString() ?? string.Empty;
	}
}