using UnityEngine;

[CreateAssetMenu(fileName = "Sound Info", menuName = "Registry/Sound Info")]
public class SoundInfo : ScriptableObject
{
	public string ID   => System.IO.Path.GetFileNameWithoutExtension(Path);
	public string Path => m_Path;
 
	[SerializeField, Path(typeof(AudioClip))] string m_Path;
}