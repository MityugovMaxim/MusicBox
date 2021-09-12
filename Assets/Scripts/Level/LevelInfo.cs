using UnityEngine;

[CreateAssetMenu(fileName = "Level Info", menuName = "Registry/Level Info")]
public class LevelInfo : RegistryEntry
{
	public string    Artist => m_Artist;
	public string    Title  => m_Title;
	public string    ID     => m_ID;
	public LevelMode Mode   => m_Mode;
	public bool      Locked => m_Locked;
	public long      Payout => m_Payout;
	public long      Price  => m_Price;
	public float     Length => m_Length;
	public float     BPM    => m_BPM;
	public float     Speed  => m_Speed;
	public string    Skin   => m_Skin;

	[SerializeField, HideInInspector] string    m_Artist;
	[SerializeField, HideInInspector] string    m_Title;
	[SerializeField, HideInInspector] string    m_ID;
	[SerializeField, HideInInspector] LevelMode m_Mode;
	[SerializeField, HideInInspector] bool      m_Locked;
	[SerializeField, HideInInspector] long      m_Payout;
	[SerializeField, HideInInspector] long      m_Price;
	[SerializeField, HideInInspector] float     m_Length;
	[SerializeField, HideInInspector] float     m_BPM;
	[SerializeField, HideInInspector] float     m_Speed;
	[SerializeField, HideInInspector] string    m_Skin;
}