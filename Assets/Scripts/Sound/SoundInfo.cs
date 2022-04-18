using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Sound Info", menuName = "Registry/Sound Info")]
public class SoundInfo : ScriptableObject
{
	public enum Mode
	{
		None,
		Random,
		Ascend,
		Descend,
	}

	public string ID         => name;
	public string Path       => m_Path;
	public float  Pitch      => GetPitch(0, 0);
	public float  Volume     => GetVolume(0, 0);
	public bool   Persistent => m_Persistent;
 
	[SerializeField, Path(typeof(AudioClip))] string m_Path;
	[SerializeField]                          Mode   m_PitchMode;
	[SerializeField, Range(0, 2)]             float  m_MinPitch = 1;
	[SerializeField, Range(0, 2)]             float  m_MaxPitch = 1;
	[SerializeField]                          Mode   m_VolumeMode;
	[SerializeField, Range(0, 1)]             float  m_MinVolume = 1;
	[SerializeField, Range(0, 1)]             float  m_MaxVolume = 1;
	[SerializeField]                          bool   m_Persistent;

	public float GetPitch(int _Index, int _Count)
	{
		switch (m_PitchMode)
		{
			case Mode.None:
				return GetAveragePitch();
			case Mode.Random:
				return GetRandomPitch();
			case Mode.Ascend:
				return GetAscendPitch(_Index, _Count);
			case Mode.Descend:
				return GetDescendPitch(_Index, _Count);
			default:
				return GetAveragePitch();
		}
	}

	public float GetVolume(int _Index, int _Count)
	{
		switch (m_VolumeMode)
		{
			case Mode.None:
				return GetAverageVolume();
			case Mode.Random:
				return GetRandomVolume();
			case Mode.Ascend:
				return GetAscendVolume(_Index, _Count);
			case Mode.Descend:
				return GetDescendVolume(_Index, _Count);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	float GetAveragePitch()
	{
		return (m_MinPitch + m_MaxPitch) * 0.5f;
	}

	public float GetRandomPitch()
	{
		return Random.Range(m_MinPitch, m_MaxPitch);
	}

	public float GetAscendPitch(int _Index, int _Count)
	{
		float phase = Mathf.InverseLerp(0, _Count - 1, _Index);
		
		return Mathf.Lerp(m_MinPitch, m_MaxPitch, phase);
	}

	public float GetDescendPitch(int _Index, int _Count)
	{
		float phase = Mathf.InverseLerp(0, _Count - 1, _Index);
		
		return Mathf.Lerp(m_MaxPitch, m_MinPitch, phase);
	}

	public float GetAverageVolume()
	{
		return (m_MinVolume + m_MaxVolume) * 0.5f;
	}

	public float GetRandomVolume()
	{
		return Random.Range(m_MinVolume, m_MaxVolume);
	}

	public float GetAscendVolume(int _Index, int _Count)
	{
		float phase = Mathf.InverseLerp(0, _Count - 1, _Index);
		
		return Mathf.Lerp(m_MinVolume, m_MaxVolume, phase);
	}

	public float GetDescendVolume(int _Index, int _Count)
	{
		float phase = Mathf.InverseLerp(0, _Count - 1, _Index);
		
		return Mathf.Lerp(m_MaxVolume, m_MinVolume, phase);
	}
}