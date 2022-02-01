using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[AddComponentMenu("Layout/Aspect Ratio Scaler")]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class AspectRatioScaler : UIBehaviour, ILayoutSelfController
{
	public enum AspectMode
	{
		None,
		Fit,
		Fill
	}

	RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	[SerializeField] AspectMode    m_AspectMode;
	[SerializeField] float         m_Scale = 1;
	[SerializeField] bool          m_Limit;
	[NonSerialized]  RectTransform m_RectTransform;

	bool m_DelayedUpdateScale;

	protected AspectRatioScaler() { }

	public virtual void SetLayoutHorizontal() { }

	public virtual void SetLayoutVertical() { }

	protected override void OnEnable()
	{
		base.OnEnable();
		
		UpdateScale();
	}

	protected override void OnDisable()
	{
		LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
		
		base.OnDisable();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		m_DelayedUpdateScale = true;
	}
	#endif

	void Update()
	{
		if (m_DelayedUpdateScale)
		{
			m_DelayedUpdateScale = false;
			
			UpdateScale();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		UpdateScale();
	}

	void UpdateScale()
	{
		if (!IsActive())
			return;
		
		RectTransform parent     = RectTransform.parent as RectTransform;
		Vector2       sourceSize = RectTransform.rect.size;
		Vector2       targetSize = (parent != null ? parent.rect.size : sourceSize) * m_Scale;
		
		float scale;
		
		switch (m_AspectMode)
		{
			case AspectMode.Fit:
				scale = Mathf.Min(
					targetSize.x / sourceSize.x,
					targetSize.y / sourceSize.y
				);
				if (m_Limit)
					scale = Mathf.Min(scale, 1);
				break;
			
			case AspectMode.Fill:
				scale = Mathf.Max(
					targetSize.x / sourceSize.x,
					targetSize.y / sourceSize.y
				);
				if (m_Limit)
					scale = Mathf.Min(scale, 1);
				break;
			default:
				scale = 1;
				break;
		}
		
		RectTransform.localScale = new Vector3(scale, scale, scale);
	}
}