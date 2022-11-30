using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

[ExecuteAlways]
public class UIObject : UIEntity
{
	[SerializeField] int      m_Width  = 64;
	[SerializeField] int      m_Height = 64;
	[SerializeField] Camera   m_Camera;
	[SerializeField] RawImage m_Image;

	[NonSerialized] RenderTexture m_RenderTexture;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		CreateTexture();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		RemoveTexture();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		CreateTexture();
	}
	#endif

	void CreateTexture()
	{
		RemoveTexture();
		
		m_RenderTexture = RenderTexture.GetTemporary(m_Width, m_Height, 8, GraphicsFormat.R16G16B16A16_UNorm);
		
		m_Image.texture = m_RenderTexture;
		
		m_Camera.targetTexture = m_RenderTexture;
	}

	void RemoveTexture()
	{
		if (m_RenderTexture == null)
			return;
		
		RenderTexture.ReleaseTemporary(m_RenderTexture);
		
		m_Image.texture = null;
		
		m_Camera.targetTexture = null;
		
		m_RenderTexture = null;
	}
}
