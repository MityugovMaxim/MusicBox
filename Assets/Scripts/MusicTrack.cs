using UnityEngine;

public class MusicTrack : Track<MusicClip>
{
	[SerializeField, Reference(typeof(AudioSource))] string m_AudioSourceReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		AudioSource audioSource = GetReference<AudioSource>(m_AudioSourceReference);
		
		if (audioSource == null)
			audioSource = AddReference<AudioSource>();
		
		foreach (MusicClip clip in Clips)
			clip.Initialize(audioSource);
	}

	static Texture2D CreateWavemap(AudioClip _AudioClip)
	{
		if (_AudioClip == null)
			return null;
		
		float[] data = new float[_AudioClip.samples];
		
		_AudioClip.GetData(data, 0);
		
		int size = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(data.Length)));
		
		Texture2D wavemap = new Texture2D(size, size, TextureFormat.Alpha8, false);
		
		for (int i = 0; i < data.Length; i++)
		{
			int x = i % size;
			int y = i / size;
			
			wavemap.SetPixel(x, y, new Color(0, 0, 0, data[i]));
		}
		
		wavemap.Apply();
		
		return wavemap;
	}
}