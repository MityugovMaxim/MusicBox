using System.Collections.Generic;
using UnityEngine;

public class UINormal : UIRenderer
{
	static readonly int m_NormalTexPropertyID  = Shader.PropertyToID("_NormalTex");
	static readonly int m_RefractionPropertyID = Shader.PropertyToID("_Refraction");
	static readonly int m_StrengthPropertyID   = Shader.PropertyToID("_Strength");
	static readonly int m_SpeedXPropertyID     = Shader.PropertyToID("_SpeedX");
	static readonly int m_SpeedYPropertyID     = Shader.PropertyToID("_SpeedY");
	static readonly int m_ScalePropertyID      = Shader.PropertyToID("_Scale");

	[SerializeField] Sprite  m_Normal;
	[SerializeField] Vector2 m_Speed      = Vector2.one;
	[SerializeField] float   m_Refraction = 1;
	[SerializeField] float   m_Strength   = 1;
	[SerializeField] float   m_Scale      = 1;

	readonly List<Vector2> m_UV = new List<Vector2>();

	protected override void FillMesh(Mesh _Mesh)
	{
		FillUV(m_Normal, m_UV);
		
		_Mesh.SetUVs(1, m_UV);
	}

	protected override void FillProperty(MaterialPropertyBlock _PropertyBlock)
	{
		Texture2D texture = m_Normal != null && m_Normal.texture != null
			? m_Normal.texture
			: Texture2D.whiteTexture;
		
		_PropertyBlock.SetTexture(m_NormalTexPropertyID, texture);
		_PropertyBlock.SetFloat(m_RefractionPropertyID, m_Refraction);
		_PropertyBlock.SetFloat(m_StrengthPropertyID, m_Strength);
		_PropertyBlock.SetFloat(m_SpeedXPropertyID, m_Speed.x);
		_PropertyBlock.SetFloat(m_SpeedYPropertyID, m_Speed.y);
		_PropertyBlock.SetFloat(m_ScalePropertyID, m_Scale);
	}
}