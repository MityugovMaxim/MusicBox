using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class UISafeArea : UIBehaviour
{
	RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	[SerializeField] bool    m_Left   = true;
	[SerializeField] bool    m_Right  = true;
	[SerializeField] bool    m_Top    = true;
	[SerializeField] bool    m_Bottom = true;
	[SerializeField] Vector4 m_Padding;

	RectTransform m_RectTransform;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Resize();
	}

	protected override void OnCanvasGroupChanged()
	{
		base.OnCanvasGroupChanged();
		
		Resize();
	}

	protected override void OnCanvasHierarchyChanged()
	{
		base.OnCanvasHierarchyChanged();
		
		Resize();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		
		Resize();
	}

	#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
			return;
		
		Resize();
	}
	#endif

	void Resize()
	{
		RectTransform.anchoredPosition = Vector2.zero;
		RectTransform.sizeDelta        = Vector2.zero;
		RectTransform.offsetMin        = Vector2.zero;
		RectTransform.offsetMax        = Vector2.zero;
		
		if (!enabled)
			return;
		
		Rect safeArea = Screen.safeArea;
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			RectTransform,
			safeArea.min,
			null,
			out Vector2 min
		);
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			RectTransform,
			safeArea.max,
			null,
			out Vector2 max
		);
		
		Rect source = RectTransform.rect;
		Rect target = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		
		Vector4 padding = new Vector4(
			m_Left ? Mathf.Max(0, target.xMin - source.xMin) : 0,
			m_Right ? Mathf.Max(0, source.xMax - target.xMax) : 0,
			m_Top ? Mathf.Max(0, source.yMax - target.yMax) : 0,
			m_Bottom ? Mathf.Max(0, target.yMin - source.yMin) : 0
		);
		
		padding += m_Padding;
		
		RectTransform.offsetMin = new Vector2(padding.x, padding.w);
		RectTransform.offsetMax = new Vector2(-padding.y, -padding.z);
	}
}
