using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(MusicClip))]
public class MusicClipDrawer : ClipDrawer
{
	static GUIStyle ContentStyle
	{
		get
		{
			if (m_ContentStyle == null)
			{
				m_ContentStyle                  = new GUIStyle(GUI.skin.label);
				m_ContentStyle.alignment        = TextAnchor.UpperCenter;
				m_ContentStyle.contentOffset    = new Vector2(0, 5);
				m_ContentStyle.fontStyle        = FontStyle.Bold;
				m_ContentStyle.normal.textColor = Color.white;
			}
			return m_ContentStyle;
		}
	}

	AudioClip AudioClip => AudioClipProperty.objectReferenceValue as AudioClip;

	float MinOffset
	{
		get => MinOffsetProperty.floatValue;
		set => MinOffsetProperty.floatValue = value;
	}

	float MaxOffset
	{
		get => MaxOffsetProperty.floatValue;
		set => MaxOffsetProperty.floatValue = value;
	}

	SerializedProperty AudioClipProperty { get; }
	SerializedProperty MinOffsetProperty { get; }
	SerializedProperty MaxOffsetProperty { get; }

	static GUIStyle m_ContentStyle;

	float[] m_AudioData;

	public MusicClipDrawer(Clip _Clip) : base(_Clip)
	{
		AudioClipProperty = ClipObject.FindProperty("m_AudioClip");
		MinOffsetProperty = ClipObject.FindProperty("m_MinOffset");
		MaxOffsetProperty = ClipObject.FindProperty("m_MaxOffset");
	}

	protected override void DrawBackground()
	{
		DrawWaveform(AudioClip, ClipRect, ViewRect);
	}

	protected override void DrawContent()
	{
		if (AudioClip != null)
			EditorGUI.DropShadowLabel(ViewRect, AudioClip.name, ContentStyle);
	}

	protected override void Resize(float _MinTime, float _MaxTime)
	{
		if (AudioClip == null)
			return;
		
		float duration = AudioClip.length;
		
		if (!Mathf.Approximately(_MinTime, MinTime) && Mathf.Approximately(_MaxTime, MaxTime))
		{
			// Resize by left handle
			
			float length = Mathf.Clamp(_MaxTime - _MinTime, 0, duration - MaxOffset);
			float delta  = _MinTime - MinTime;
			
			MinOffset = Mathf.Max(0, MinOffset + delta);
			MinTime   = _MaxTime - length;
			
			ClipObject.ApplyModifiedProperties();
			
			return;
		}
		
		if (!Mathf.Approximately(_MaxTime, MaxTime) && Mathf.Approximately(_MinTime, MinTime))
		{
			// Resize by right handle
			
			float length = Mathf.Clamp(_MaxTime - _MinTime, 0, duration - MinOffset);
			float delta = _MaxTime - MaxTime;
			
			MaxOffset = Mathf.Max(0, MaxOffset - delta);
			MaxTime   = _MinTime + length;
			
			ClipObject.ApplyModifiedProperties();
			
			return;
		}
		
		base.Resize(_MinTime, _MaxTime);
	}

	void DrawWaveform(AudioClip _AudioClip, Rect _ClipRect, Rect _ViewRect)
	{
		if (Event.current.type != EventType.Repaint || Mathf.Approximately(_ClipRect.width, 0))
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
		
		Color color = new Color(1, 0.55f, 0, 1);
		
		float minLimit = MathUtility.Remap(MinTime - MinOffset, MinTime, MaxTime, _ClipRect.xMin, _ClipRect.xMax);
		float maxLimit = MathUtility.Remap(MaxTime + MaxOffset, MinTime, MaxTime, _ClipRect.xMin, _ClipRect.xMax);
		
		float min = Mathf.InverseLerp(minLimit, maxLimit, _ViewRect.xMin);
		float max = Mathf.InverseLerp(minLimit, maxLimit, _ViewRect.xMax);
		
		int resolution = _AudioClip.samples / (int)Mathf.Max(1, maxLimit - minLimit);
		
		float scale = Mathf.InverseLerp(0, 300, TrackMaxTime - TrackMinTime);
		
		int skip = (int)Mathf.Max(4, scale * 150);
		
		void Evaluate(float _Phase, out Color _Color, out float _MinValue, out float _MaxValue)
		{
			_Color = color;
			
			float phase = MathUtility.Remap(_Phase, 0, 1, min, max);
			
			int index = (int)(phase * (samples / resolution) * resolution);
			
			float value = 0;
			
			for (int i = 0; i < resolution; i += skip)
			{
				int j = (index + i) * channels;
				
				if (j >= m_AudioData.Length)
					break;
				
				float data = m_AudioData[(index + i) * channels];
				
				value = Mathf.Max(value, Mathf.Abs(data));
			}
			
			value *= 0.95f;
			
			_MaxValue = value;
			_MinValue = -value;
		}
		
		AudioCurveRendering.DrawCurveBackground(_ClipRect);
		
		GUI.BeginClip(new RectOffset(1, 1, 0, 0).Remove(_ViewRect));
		
		AudioCurveRendering.DrawMinMaxFilledCurve(new Rect(Vector2.zero, _ViewRect.size), Evaluate);
		
		GUI.EndClip();
		
		AudioCurveRendering.DrawCurveFrame(_ClipRect);
	}
}