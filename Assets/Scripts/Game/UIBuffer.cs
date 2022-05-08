using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIBuffer : MonoBehaviour
{
	[SerializeField] Camera   m_Camera;
	[SerializeField] RawImage m_Target;
	[SerializeField] Renderer m_Renderer;
	[SerializeField] Vector2  m_Size;

	bool m_Active;

	MaterialPropertyBlock m_PropertyBlock;

	RenderTexture       m_A;
	RenderTexture       m_B;
	static readonly int m_MainTexPropertyID = Shader.PropertyToID("_MainTex");

	protected void OnEnable()
	{
		CreateBuffer();
	}

	protected void OnDisable()
	{
		RemoveBuffer();
	}

	void CreateBuffer()
	{
		if (m_Active)
			return;
		
		#if UNITY_EDITOR
		Vector2 screen = Handles.GetMainGameViewSize();
		#else
		Vector2 screen = new Vector2(Screen.width, Screen.height);
		#endif
		
		float aspect = screen.x / screen.y;
		
		Vector2 size = MathUtility.Fill(m_Size, aspect);
		
		int width  = (int)size.x;
		int height = (int)size.y;
		
		if (width <= 0 || height <= 0)
			return;
		
		m_Active = true;
		
		m_Renderer.transform.localScale  = new Vector3(size.x, size.y, 1);
		
		m_PropertyBlock = new MaterialPropertyBlock();
		
		m_A = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
		m_B = new RenderTexture(width >> 1, height >> 1, 0, RenderTextureFormat.R8);
	}

	void RemoveBuffer()
	{
		if (!m_Active)
			return;
		
		m_Active = false;
		
		if (m_PropertyBlock != null)
		{
			m_Renderer.GetPropertyBlock(m_PropertyBlock);
			
			m_PropertyBlock.SetTexture(m_MainTexPropertyID, Texture2D.blackTexture);
			
			m_Renderer.SetPropertyBlock(m_PropertyBlock);
		}
		
		m_Camera.targetTexture = null;
		m_Target.texture       = null;
		m_PropertyBlock        = null;
		
		DestroyImmediate(m_A);
		DestroyImmediate(m_B);
		
		m_A = null;
		m_B = null;
	}

	void SetTexture(Texture _Texture)
	{
		m_Renderer.GetPropertyBlock(m_PropertyBlock);
		
		m_PropertyBlock.SetTexture(m_MainTexPropertyID, _Texture);
		
		m_Renderer.SetPropertyBlock(m_PropertyBlock);
	}

	void Update()
	{
		if (!m_Active)
			return;
		
		if (Time.frameCount % 2 == 0)
		{
			m_Camera.targetTexture = m_A;
			m_Target.texture       = m_B;
			
			SetTexture(m_B);
		}
		else
		{
			m_Camera.targetTexture = m_B;
			m_Target.texture       = m_A;
			
			SetTexture(m_A);
		}
	}
}