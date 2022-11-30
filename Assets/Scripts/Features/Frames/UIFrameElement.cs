using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIFrameElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIFrameElement> { }

	static bool Processing { get; set; }

	[SerializeField] UIFrameImage m_Image;

	[Inject] FramesManager m_FramesManager;

	string m_FrameID;

	public void Setup(string _FrameID)
	{
		m_FrameID = _FrameID;
		
		m_Image.FrameID = m_FrameID;
	}

	protected override async void OnClick()
	{
		base.OnClick();
		
		if (Processing)
			return;
		
		Processing = true;
		
		await m_FramesManager.Select(m_FrameID);
		
		Processing = false;
	}
}