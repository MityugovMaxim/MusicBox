using UnityEngine;

public class MusicTrack : Track<MusicClip>
{
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