using System;
using UnityEngine;

#if UNITY_EDITOR
public partial class ColorTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Color Track", menuName = "Tracks/Color Track")]
public partial class ColorTrack : Track<ColorClip>
{
	ColorProcessor ColorProcessor
	{
		get
		{
			if (m_ColorProcessorReference == null || m_ColorProcessorCache != m_ColorProcessor)
			{
				m_ColorProcessorReference = GetReference<ColorProcessor>(m_ColorProcessor);
				m_ColorProcessorCache     = m_ColorProcessor;
			}
			return m_ColorProcessorReference;
		}
	}

	[SerializeField, Reference(typeof(ColorProcessor))] string m_ColorProcessor;

	[NonSerialized] string         m_ColorProcessorCache;
	[NonSerialized] ColorProcessor m_ColorProcessorReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		if (ColorProcessor == null)
			return;
		
		ColorScheme colorScheme = ColorProcessor.DefaultColorScheme;
		
		foreach (ColorClip clip in Clips)
		{
			clip.Initialize(
				_Sequencer,
				this,
				ColorProcessor,
				colorScheme
			);
			
			colorScheme = clip.ColorScheme;
		}
	}

	#if UNITY_EDITOR
	public override void Sort()
	{
		base.Sort();
		
		Initialize(Sequencer);
	}
	#endif
}