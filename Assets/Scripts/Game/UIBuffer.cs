using UnityEngine;

public class UIBuffer : UIOrder
{
	[SerializeField] Camera    m_ACamera;
	[SerializeField] Camera    m_BCamera;
	[SerializeField] UITexture m_AGraphic;
	[SerializeField] UITexture m_BGraphic;
	[SerializeField] UITexture m_Target;
	[SerializeField] Vector2   m_Size;

	bool m_Active;
	bool m_Switch;

	RenderTexture m_A;
	RenderTexture m_B;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		CreateBuffer();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		RemoveBuffer();
	}

	void CreateBuffer()
	{
		if (m_Active)
			return;
		
		#if UNITY_EDITOR
		Vector2 screen = UnityEditor.Handles.GetMainGameViewSize();
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
		
		m_A = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
		m_B = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
		
		m_ACamera.enabled = false;
		m_BCamera.enabled = false;
		
		m_ACamera.orthographicSize = size.y / 2;
		m_BCamera.orthographicSize = size.y / 2;
		
		m_ACamera.targetTexture = m_A;
		m_BCamera.targetTexture = m_B;
		
		m_AGraphic.Texture = m_B;
		m_BGraphic.Texture = m_A;
	}

	void RemoveBuffer()
	{
		if (!m_Active)
			return;
		
		m_Active = false;
		
		m_ACamera.enabled = false;
		m_BCamera.enabled = false;
		
		m_ACamera.targetTexture = null;
		m_BCamera.targetTexture = null;
		
		m_AGraphic.Texture = null;
		m_BGraphic.Texture = null;
		
		m_Target.Texture = null;
		
		DestroyImmediate(m_A);
		DestroyImmediate(m_B);
		
		m_A = null;
		m_B = null;
	}

	void Update()
	{
		if (!m_Active)
			return;
		
		m_Switch = !m_Switch;
		
		if (m_Switch)
		{
			m_ACamera.enabled = true;
			m_BCamera.enabled = false;
			
			m_Target.Texture = m_B;
		}
		else
		{
			m_BCamera.enabled = true;
			m_ACamera.enabled = false;
			
			m_Target.Texture = m_A;
		}
	}
}