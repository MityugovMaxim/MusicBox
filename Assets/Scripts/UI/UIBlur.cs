﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIBlur : MaskableGraphic
{
	static RenderTexture m_BlurTexture;
	static Material      m_BlurMaterial;

	public override Texture mainTexture
	{
		get
		{
			if (m_BlurTexture == null)
			{
				m_BlurTexture            = new RenderTexture(Screen.width >> 3, Screen.height >> 3, 0);
				m_BlurTexture.filterMode = FilterMode.Bilinear;
				m_BlurTexture.wrapMode   = TextureWrapMode.Clamp;
			}
			return m_BlurTexture;
		}
	}

	static Material BlurMaterial
	{
		get
		{
			if (m_BlurMaterial == null)
				m_BlurMaterial = new Material(Shader.Find("UI/Blur"));
			return m_BlurMaterial;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		
		if (m_BlurTexture == null)
		{
			m_BlurTexture            = new RenderTexture(Screen.width >> 3, Screen.height >> 3, 0);
			m_BlurTexture.filterMode = FilterMode.Bilinear;
			m_BlurTexture.wrapMode   = TextureWrapMode.Clamp;
		}
	}

	public async void Blur(Action _Finished = null)
	{
		await BlurAsync();
		
		_Finished?.Invoke();
	}

	public async Task BlurAsync()
	{
		const string blurTag = "Blur";
		
		await UnityTask.Instruction(new WaitForEndOfFrame());
		
		foreach (Camera blurCamera in Camera.allCameras)
		{
			if (!blurCamera.CompareTag(blurTag))
				continue;
			
			RenderTexture target = blurCamera.targetTexture;
			
			blurCamera.targetTexture = m_BlurTexture;
			
			blurCamera.Render();
			
			blurCamera.targetTexture = target;
		}
		
		RenderTexture buffer = RenderTexture.GetTemporary(m_BlurTexture.width, m_BlurTexture.height);
		
		for (int i = 0; i < 3; i++)
		{
			Graphics.Blit(m_BlurTexture, buffer, BlurMaterial);
			Graphics.Blit(buffer, m_BlurTexture, BlurMaterial);
		}
		
		RenderTexture.ReleaseTemporary(buffer);
	}
}
