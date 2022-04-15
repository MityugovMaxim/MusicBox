using AudioBox.ASF;
using UnityEngine;

public class UIColorTrack : ASFTrackContext<ASFColorClip>, IASFColorSampler
{
	static readonly int m_BackgroundPrimaryPropertyID   = Shader.PropertyToID("_BackgroundPrimaryColor");
	static readonly int m_BackgroundSecondaryPropertyID = Shader.PropertyToID("_BackgroundSecondaryColor");
	static readonly int m_ForegroundPrimaryPropertyID   = Shader.PropertyToID("_ForegroundPrimaryColor");
	static readonly int m_ForegroundSecondaryPropertyID = Shader.PropertyToID("_ForegroundSecondaryColor");

	public override void AddClip(ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect) { }

	public override void RemoveClip(ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect) { }

	public override void ProcessClip(ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect) { }

	public override void Clear() { }

	void IASFColorSampler.Sample(ASFColorClip _Source, ASFColorClip _Target, float _Phase)
	{
		SetColor(m_BackgroundPrimaryPropertyID, _Source.BackgroundPrimary, _Target.BackgroundPrimary, _Phase);
		SetColor(m_BackgroundSecondaryPropertyID, _Source.BackgroundSecondary, _Target.BackgroundSecondary, _Phase);
		SetColor(m_ForegroundPrimaryPropertyID, _Source.ForegroundPrimary, _Target.ForegroundPrimary, _Phase);
		SetColor(m_ForegroundSecondaryPropertyID, _Source.ForegroundSecondary, _Target.ForegroundSecondary, _Phase);
	}

	void SetColor(int _ColorPropertyID, Color _Source, Color _Target, float _Phase)
	{
		Color color = Color.Lerp(_Source, _Target, _Phase);
		
		Shader.SetGlobalColor(_ColorPropertyID, color);
	}
}