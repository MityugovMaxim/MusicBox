using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public abstract class UIImage : MaskableGraphic
{
	static readonly int m_BlendSrcPropertyID = Shader.PropertyToID("_BlendSrc");
	static readonly int m_BlendDstPropertyID = Shader.PropertyToID("_BlendDst");

	public enum BlendType
	{
		Blend    = 0,
		Additive = 1,
	}

	public enum BlendScheme
	{
		None       = 0,
		Background = 1,
		Foreground = 2,
	}

	public override Material defaultMaterial => GetMaterial();

	public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : base.mainTexture;

	protected BlendType   Type   => m_Type;
	protected BlendScheme Scheme => m_Scheme;

	[SerializeField] Sprite      m_Sprite;
	[SerializeField] BlendType   m_Type;
	[SerializeField] BlendScheme m_Scheme;

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		Rect    rect = GetPixelAdjustedRect();
		Vector4 mask = GetMask();
		Vector4 uv   = m_Sprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(m_Sprite) : Vector4.zero;
		
		Color32 color32 = color;
		_VertexHelper.Clear();
		
		_VertexHelper.AddVert(
			new Vector3(rect.xMin, rect.yMin),
			color32,
			new Vector2(uv.x, uv.y),
			new Vector2(mask.x, mask.y),
			GetUV2(),
			GetUV3(),
			GetNormal(),
			GetTangent()
		);
		
		_VertexHelper.AddVert(
			new Vector3(rect.xMin, rect.yMax),
			color32,
			new Vector2(uv.x, uv.w),
			new Vector2(mask.x, mask.w),
			GetUV2(),
			GetUV3(),
			GetNormal(),
			GetTangent()
		);
		
		_VertexHelper.AddVert(
			new Vector3(rect.xMax, rect.yMax),
			color32,
			new Vector2(uv.z, uv.w),
			new Vector2(mask.z, mask.w),
			GetUV2(),
			GetUV3(),
			GetNormal(),
			GetTangent()
		);
		
		_VertexHelper.AddVert(
			new Vector3(rect.xMax, rect.yMin),
			color32,
			new Vector2(uv.z, uv.y),
			new Vector2(mask.z, mask.y),
			GetUV2(),
			GetUV3(),
			GetNormal(),
			GetTangent()
		);
		
		_VertexHelper.AddTriangle(0, 1, 2);
		_VertexHelper.AddTriangle(2, 3, 0);
	}

	protected virtual Vector4 GetMask()
	{
		return new Vector4(0, 0, 1, 1);
	}

	protected abstract Material GetMaterial();
	protected abstract Vector2 GetUV2();
	protected abstract Vector2 GetUV3();
	protected abstract Vector3 GetNormal();
	protected abstract Vector4 GetTangent();

	protected static Material CreateMaterial(string _Shader, params string[] _Keywords)
	{
		Material material = new Material(Shader.Find(_Shader));
		foreach (string keyword in _Keywords)
		{
			if (!string.IsNullOrEmpty(keyword))
				material.EnableKeyword(keyword);
		}
		return material;
	}

	protected static void SetAdditive(Material _Material)
	{
		_Material.SetInt(m_BlendSrcPropertyID, (int)BlendMode.SrcAlpha);
		_Material.SetInt(m_BlendDstPropertyID, (int)BlendMode.One);
	}

	protected static void SetBlend(Material _Material)
	{
		_Material.SetInt(m_BlendSrcPropertyID, (int)BlendMode.SrcAlpha);
		_Material.SetInt(m_BlendDstPropertyID, (int)BlendMode.OneMinusSrcAlpha);
	}
}