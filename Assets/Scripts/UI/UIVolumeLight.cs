using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIVolumeLight : MaskableGraphic
{
	float Min => m_Arc * 0.5f - m_Angle * (m_Flip ? -1 : 1);
	float Max => m_Arc * 0.5f + m_Angle * (m_Flip ? -1 : 1);

	[SerializeField]              RectTransform m_MinBeam;
	[SerializeField]              RectTransform m_MaxBeam;
	[SerializeField]              float         m_Radius = 100;
	[SerializeField, Range(0, 1)] float         m_Spread = 0.4f;
	[SerializeField]              float         m_Arc    = 20;
	[SerializeField]              bool          m_Flip;
	[SerializeField]              float         m_Angle;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		UnityEditor.EditorApplication.delayCall += () =>
		{
			if (m_MinBeam != null)
			{
				m_MinBeam.localRotation = Quaternion.Euler(0, 0, -Min);
				m_MinBeam.sizeDelta     = new Vector2(m_MinBeam.sizeDelta.x, m_Radius);
			}
			
			if (m_MaxBeam != null)
			{
				m_MaxBeam.localRotation = Quaternion.Euler(0, 0, Max);
				m_MaxBeam.sizeDelta     = new Vector2(m_MaxBeam.sizeDelta.x, m_Radius);
			}
		};
	}
	#endif

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessBeams();
	}

	void ProcessBeams()
	{
		m_MinBeam.localRotation = Quaternion.Euler(0, 0, -Min);
		m_MinBeam.sizeDelta = new Vector2(m_MinBeam.sizeDelta.x, m_Radius);
		
		m_MaxBeam.localRotation = Quaternion.Euler(0, 0, Max);
		m_MaxBeam.sizeDelta = new Vector2(m_MaxBeam.sizeDelta.x, m_Radius);
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		Color32 source = color;
		Color32 target = color;
		
		target.a = 0;
		
		Vector3 normal = Vector3.down;
		
		Vector3 left   = Quaternion.Euler(0, 0, -Min) * normal * m_Radius;
		Vector3 right  = Quaternion.Euler(0, 0, Max) * normal * m_Radius;
		Vector3 center = (left + right) * 0.5f;
		
		UIVertex a = new UIVertex()
		{
			position = Vector3.zero,
			color    = source,
			uv0      = new Vector4(m_Spread, 1)
		};
		
		UIVertex b = new UIVertex()
		{
			position = left,
			color    = target,
			uv0      = new Vector4(0, 0),
		};
		
		UIVertex c = new UIVertex()
		{
			position = center,
			color    = target,
			uv0      = new Vector4(0.5f, 0)
		};
		
		_VertexHelper.AddVert(a);
		_VertexHelper.AddVert(b);
		_VertexHelper.AddVert(c);
		
		_VertexHelper.AddTriangle(1, 0, 2);
		
		UIVertex d = new UIVertex()
		{
			position = Vector3.zero,
			color    = source,
			uv0      = new Vector4(1.0f - m_Spread, 1)
		};
		
		UIVertex e = new UIVertex()
		{
			position = right,
			color    = target,
			uv0      = new Vector4(1, 0),
		};
		
		UIVertex f = new UIVertex()
		{
			position = center,
			color    = target,
			uv0      = new Vector4(0.5f, 0)
		};
		
		_VertexHelper.AddVert(d);
		_VertexHelper.AddVert(e);
		_VertexHelper.AddVert(f);
		
		_VertexHelper.AddTriangle(5, 3, 4);
	}
}