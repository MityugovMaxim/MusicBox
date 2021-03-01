using UnityEditor;
using UnityEngine;

[ClipDrawer(typeof(MusicClip))]
public class MusicClipDrawer : ClipDrawer
{
	MusicClip MusicClip { get; }

	float[] m_AudioData;

	public MusicClipDrawer(Clip _Clip) : base(_Clip)
	{
		MusicClip = _Clip as MusicClip;
	}

	protected override void DrawBackground(Rect _Rect, Rect _ViewRect)
	{
		DrawWaveform(
			MusicClip.AudioClip,
			_Rect,
			_ViewRect
		);
	}

	void DrawWaveform(AudioClip _AudioClip, Rect _Rect, Rect _ViewRect)
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		if (_AudioClip == null)
			return;
		
		int channels = _AudioClip.channels;
		int samples  = _AudioClip.samples;
		
		if (m_AudioData == null || m_AudioData.Length != channels * samples)
		{
			m_AudioData = new float[_AudioClip.samples * _AudioClip.channels];
			_AudioClip.GetData(m_AudioData, 0);
		}
		
		Color color = Color.white;
		
		float min = Mathf.InverseLerp(_Rect.xMin, _Rect.xMax, _ViewRect.xMin);
		float max = Mathf.InverseLerp(_Rect.xMin, _Rect.xMax, _ViewRect.xMax);
		
		const int smooth = 10;
		
		int resolution = _AudioClip.samples / Mathf.FloorToInt(_Rect.width * (smooth + 1));
		
		void Evaluate(float _Phase, out Color _Color, out float _MinValue, out float _MaxValue)
		{
			_Color = color;
			
			float phase = MathUtility.Remap(_Phase, 0, 1, min, max);
			
			float time = Mathf.Clamp(
				phase * (samples / resolution - smooth - 1),
				smooth,
				samples / resolution - smooth - 1
			);
			
			int index = Mathf.FloorToInt(time);
			float value = Mathf.Abs(m_AudioData[index * resolution * channels]);
			for (int i = -smooth; i <= smooth; i++)
				value += Mathf.Abs(m_AudioData[(index + i) * resolution * channels]);
			value /= smooth * 2 + 1;
			
			_MinValue = -value;
			_MaxValue = value;
		}
		
		AudioCurveRendering.DrawMinMaxFilledCurve(
			new RectOffset(4, 4, 0, 0).Remove(_ViewRect),
			Evaluate
		);
	}
}