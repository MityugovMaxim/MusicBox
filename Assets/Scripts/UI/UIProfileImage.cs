using System;
using UnityEngine;

public class UIProfileImage : UIEntity
{
	[SerializeField] WebGraphic m_Image;

	public void Setup(Uri _Uri)
	{
		m_Image.Path = _Uri?.ToString() ?? string.Empty;
	}
}