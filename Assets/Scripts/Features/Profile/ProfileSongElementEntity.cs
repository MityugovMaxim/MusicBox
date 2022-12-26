using UnityEngine;

public class ProfileSongElementEntity : LayoutEntity
{
	public override string  ID   => m_SongID;
	public override Vector2 Size => m_Pool.Size;

	readonly string                    m_SongID;
	readonly ProfileSongMode           m_Mode;
	readonly UIProfileSongElement.Pool m_Pool;

	UIProfileSongElement m_Item;

	public ProfileSongElementEntity(string _SongID, ProfileSongMode _Mode, UIProfileSongElement.Pool _Pool)
	{
		m_SongID = _SongID;
		m_Mode   = _Mode;
		m_Pool   = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_SongID, m_Mode);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}