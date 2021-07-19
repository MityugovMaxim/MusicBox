using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(ColorClip))]
public class ColorClipDrawer : ClipDrawer
{
	SerializedProperty ColorSchemeProperty { get; }

	public ColorClipDrawer(Clip _Clip) : base(_Clip)
	{
		ColorSchemeProperty = ClipObject.FindProperty("m_ColorScheme");
	}

	protected override void DrawContent()
	{
		ColorSchemeAsset colorScheme = ColorSchemeProperty.objectReferenceValue as ColorSchemeAsset;
		
		if (colorScheme == null)
			return;
		
		Rect rect = new RectOffset(2, 2, 2, 2).Remove(ViewRect);
		
		float step = rect.height / 4;
		
		Rect aRect = new Rect(rect.x, rect.y + step * 0, rect.width, step);
		Rect bRect = new Rect(rect.x, rect.y + step * 1, rect.width, step);
		Rect cRect = new Rect(rect.x, rect.y + step * 2, rect.width, step);
		Rect dRect = new Rect(rect.x, rect.y + step * 3, rect.width, step);
		
		EditorGUI.DrawRect(aRect, colorScheme.ColorScheme.BackgroundPrimary);
		EditorGUI.DrawRect(bRect, colorScheme.ColorScheme.BackgroundSecondary);
		EditorGUI.DrawRect(cRect, colorScheme.ColorScheme.ForegroundPrimary);
		EditorGUI.DrawRect(dRect, colorScheme.ColorScheme.ForegroundSecondary);
	}
}