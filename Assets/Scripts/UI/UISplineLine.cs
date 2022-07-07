using UnityEngine;
using UnityEngine.UI;

public class UISplineLine : MaskableGraphic
{
	public float Min
	{
		get => m_Min;
		set
		{
			if (Mathf.Approximately(m_Min, value))
				return;
			
			m_Min = value;
			
			SetVerticesDirty();
		}
	}

	public float Max
	{
		get => m_Max;
		set
		{
			if (Mathf.Approximately(m_Max, value))
				return;
			
			m_Max = value;
			
			SetVerticesDirty();
		}
	}

	public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : base.mainTexture;

	[SerializeField] UISpline m_Spline;
	[SerializeField] Sprite   m_Sprite;
	[SerializeField] float    m_Width;

	[SerializeField, Range(0, 1)] float m_Min;
	[SerializeField, Range(0, 1)] float m_Max;

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		float size = m_Width * 0.5f;
		
		Rect uv = MeshUtility.GetUV(m_Sprite);
		
		Color32 color = this.color;
		
		float   centerUV = Mathf.Lerp(uv.yMin, uv.yMax, 0.5f);
		Vector4 minUV    = new Vector4(uv.xMin, centerUV);
		Vector4 maxUV    = new Vector4(uv.xMax, centerUV);
		
		void AddPoint(UISpline.Point _Point)
		{
			Vector2 width = _Point.Normal * size;
			_VertexHelper.AddVert(_Point.Position + width, color, minUV);
			_VertexHelper.AddVert(_Point.Position - width, color, maxUV);
		}
		
		// Min Cap
		GenerateMinCap(uv, color, _VertexHelper);
		
		UISpline.Point a = m_Spline.GetPoint(Min);
		AddPoint(a);
		
		foreach (UISpline.Point point in m_Spline)
		{
			if (point.Phase < Min || point.Phase > Max)
				continue;
			
			AddPoint(point);
		}
		
		UISpline.Point b = m_Spline.GetPoint(Max);
		AddPoint(b);
		
		GenerateMaxCap(uv, color, _VertexHelper);
		
		int quads = _VertexHelper.currentVertCount / 2 - 1;
		for (int i = 0; i < quads; i++)
		{
			_VertexHelper.AddTriangle(
				i * 2 + 0,
				i * 2 + 1,
				i * 2 + 2
			);
			
			_VertexHelper.AddTriangle(
				i * 2 + 2,
				i * 2 + 1,
				i * 2 + 3
			);
		}
	}

	void GenerateMinCap(Rect _UV, Color32 _Color, VertexHelper _VertexHelper)
	{
		float height = m_Width / (_UV.width / _UV.height) * 0.5f;
		
		UISpline.Point point = m_Spline.GetPoint(Min);
		
		Vector2 width  = point.Normal * m_Width * 0.5f;
		Vector2 normal = point.Normal.Rotate90();
		
		_VertexHelper.AddVert(
			point.Position + width - normal * height,
			_Color,
			new Vector2(_UV.xMin, _UV.yMin)
		);
		
		_VertexHelper.AddVert(
			point.Position - width - normal * height,
			_Color,
			new Vector2(_UV.xMax, _UV.yMin)
		);
	}

	void GenerateMaxCap(Rect _UV, Color32 _Color, VertexHelper _VertexHelper)
	{
		float height = m_Width / (_UV.width / _UV.height) * 0.5f;
		
		UISpline.Point point = m_Spline.GetPoint(Max);
		
		Vector2 width  = point.Normal * m_Width * 0.5f;
		Vector2 normal = point.Normal.Rotate90();
		
		_VertexHelper.AddVert(
			point.Position + width + normal * height,
			_Color,
			new Vector2(_UV.xMin, _UV.yMax)
		);
		
		_VertexHelper.AddVert(
			point.Position - width + normal * height,
			_Color,
			new Vector2(_UV.xMax, _UV.yMax)
		);
	}
}