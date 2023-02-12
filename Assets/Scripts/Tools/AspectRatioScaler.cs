using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[AddComponentMenu("Layout/Aspect Ratio Scaler")]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class AspectRatioScaler : UIEntity, ILayoutSelfController
{
	public enum AspectMode
	{
		None,
		Fit,
		Fill
	}

	[SerializeField] AspectMode    m_AspectMode;
	[SerializeField] float         m_Scale = 1;
	[SerializeField] bool          m_Limit;
	[NonSerialized]  RectTransform m_RectTransform;

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
	void Update()
	{
		if (Application.isPlaying || !IsInstanced)
			return;
		
		UpdateScale();
	}
	#endif

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
		
		RectTransform.localScale = new Vector3(scale, scale, 1);
	}
}
