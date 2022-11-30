using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIChestElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIChestElement> { }

	static bool Processing { get; set; }

	[SerializeField] UIChestBody m_Body;
	[SerializeField] UIChestTime m_Time;

	[Inject] ChestsManager m_ChestsManager;

	string m_ChestID;

	public void Setup(string _ChestID)
	{
		m_ChestID = _ChestID;
		
		m_Body.ChestID = m_ChestID;
		m_Time.ChestID = m_ChestID;
	}

	protected override async void OnClick()
	{
		base.OnClick();
		
		if (Processing)
			return;
		
		Processing = true;
		
		await m_ChestsManager.Select(m_ChestID);
		
		Processing = false;
	}
}