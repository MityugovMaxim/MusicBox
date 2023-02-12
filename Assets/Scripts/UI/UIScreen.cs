using UnityEngine;

[ExecuteAlways]
public class UIScreen : UIOrder
{
	[SerializeField] Camera[] m_Cameras;
	[SerializeField] float    m_Distance;
	[SerializeField] float    m_Width;
	[SerializeField] float    m_Height;

	protected override void Awake()
	{
		base.Awake();
		
		ProcessScreen();
		
		ProcessCamera();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessScreen();
		
		ProcessCamera();
	}
	#endif

	void ProcessCamera()
	{
		if (m_Cameras == null || m_Cameras.Length == 0)
			return;
		
		Rect rect = GetWorldRect();
		
		foreach (Camera screenCamera in m_Cameras)
		{
			if (m_Cameras == null)
				continue;
			
			Transform cameraTransform = screenCamera.transform;
			
			cameraTransform.localPosition = new Vector3(0, 0, -m_Distance);
			
			if (screenCamera.orthographic)
			{
				screenCamera.nearClipPlane    = 0;
				screenCamera.farClipPlane     = m_Distance;
				screenCamera.orthographicSize = rect.height * 0.5f;
			}
			else
			{
				Vector3 position = cameraTransform.position;
				
				Vector3 a = new Vector3(position.x, rect.yMin, 0) - position;
				Vector3 b = new Vector3(position.x, rect.yMax, 0) - position;
				
				float angle = Vector3.Angle(a, b);
				
				screenCamera.fieldOfView = angle;
			}
		}
	}

	void ProcessScreen()
	{
		RectTransform.pivot     = new Vector2(0.5f, 0.5f);
		RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		
		Rect view = new Rect(
			-m_Width * 0.5f,
			-m_Height * 0.5f,
			m_Width,
			m_Height
		);
		
		float width  = Screen.width;
		float height = Screen.height;
		float aspect = width / height;
		
		view = MathUtility.Fill(view, aspect);
		
		RectTransform.sizeDelta = view.size;
	}
}
