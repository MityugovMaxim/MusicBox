using UnityEngine;

[ExecuteInEditMode]
public class UIParticleFX : UIEntity
{
	public enum ScaleMode
	{
		None,
		Stretch,
		Fill,
		Fit,
	}

	[SerializeField] ParticleSystem[] m_ParticleSystems;
	[SerializeField] ScaleMode        m_ScaleMode;
	[SerializeField] Vector2          m_Size;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessScale();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		ProcessScale();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		
		ProcessScale();
	}

	#if UNITY_EDITOR
	protected override void Reset()
	{
		base.Reset();
		
		m_ParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
		
		foreach (ParticleSystem particleSystem in m_ParticleSystems)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			
			main.scalingMode = ParticleSystemScalingMode.Local;
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying)
			return;
		
		ProcessScale();
	}
	#endif

	void ProcessScale()
	{
		Rect rect = GetLocalRect();
		
		Vector3 origin = RectTransform.lossyScale;
		
		origin.z = 1;
		
		Vector3 scale = Vector3.Scale(GetScale(rect.size), origin);
		
		foreach (ParticleSystem particleSystem in m_ParticleSystems)
			particleSystem.transform.localScale = scale;
	}

	Vector3 GetScale(Vector2 _Size)
	{
		switch (m_ScaleMode)
		{
			case ScaleMode.Stretch:
				return new Vector3(
					Mathf.Abs(_Size.x / m_Size.x),
					Mathf.Abs(_Size.y / m_Size.y),
					1
				);
			
			case ScaleMode.Fill:
				float fill = Mathf.Max(
					Mathf.Abs(_Size.x / m_Size.x),
					Mathf.Abs(_Size.y / m_Size.y)
				);
				return new Vector3(fill, fill, 1);
			
			case ScaleMode.Fit:
				float fit = Mathf.Min(
					Mathf.Abs(_Size.x / m_Size.x),
					Mathf.Abs(_Size.y / m_Size.y)
				);
				return new Vector3(fit, fit, 1);
			
			default:
				return Vector3.one;
		}
	}
}