using UnityEngine;
using Zenject;

public class UIProfileImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	[Inject] SocialProcessor m_SocialProcessor;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessImage();
		
		m_SocialProcessor.OnPhotoChange += ProcessImage;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_SocialProcessor.OnPhotoChange -= ProcessImage;
	}

	void ProcessImage()
	{
		m_Image.Path = m_SocialProcessor.Photo?.ToString() ?? string.Empty;
	}
}
