using System;
using UnityEngine;

[ExecuteAlways]
public abstract class UIRenderer : UIOrder
{
	static readonly int m_ColorPropertyID = Shader.PropertyToID("_Color");

	static Material m_DefaultMaterial;

	public Color Color
	{
		get => m_Color;
		set
		{
			if (m_Color == value)
				return;
			
			m_Color = value;
			
			InvalidateProperties();
		}
	}

	public float Alpha
	{
		get => m_Color.a;
		set
		{
			if (Mathf.Approximately(m_Color.a, value))
				return;
			
			m_Color.a = value;
			
			InvalidateProperties();
		}
	}

	static Material DefaultMaterial
	{
		get
		{
			if (m_DefaultMaterial == null)
				m_DefaultMaterial = new Material(Shader.Find("UI/Default"));
			return m_DefaultMaterial;
		}
	}

	[SerializeField] Color    m_Color = Color.white;
	[SerializeField] Material m_Material;

	Renderer              m_Renderer;
	MaterialPropertyBlock m_PropertyBlock;

	[NonSerialized] bool m_DirtyProperties = true;
	[NonSerialized] bool m_DirtyMaterial   = true;

	protected override void Awake()
	{
		base.Awake();
		
		m_Renderer = CreateRenderer();
		
		m_PropertyBlock = new MaterialPropertyBlock();
	}

	protected virtual void LateUpdate()
	{
		if (m_DirtyProperties)
		{
			m_DirtyProperties = false;
			
			GenerateProperty();
		}
		
		if (m_DirtyMaterial)
		{
			m_DirtyMaterial = false;
			
			UpdateMaterial();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		GenerateProperty();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		if (m_Renderer == null)
			m_Renderer = CreateRenderer();
		
		if (m_PropertyBlock == null)
			m_PropertyBlock = new MaterialPropertyBlock();
		
		InvalidateProperties();
		
		InvalidateMaterial();
	}
	#endif

	protected abstract Renderer CreateRenderer();

	protected override void OnEnable()
	{
		base.OnEnable();
		
		InvalidateProperties();
	}

	void UpdateMaterial()
	{
		m_Renderer.sharedMaterial = m_Material != null ? m_Material : DefaultMaterial;
	}

	void GenerateProperty()
	{
		m_Renderer.GetPropertyBlock(m_PropertyBlock);
		
		m_PropertyBlock.SetColor(m_ColorPropertyID, m_Color);
		
		FillProperty(m_PropertyBlock);
		
		m_Renderer.SetPropertyBlock(m_PropertyBlock);
	}

	protected void InvalidateProperties()
	{
		m_DirtyProperties = true;
	}

	protected void InvalidateMaterial()
	{
		m_DirtyMaterial = true;
	}

	protected virtual void FillProperty(MaterialPropertyBlock _PropertyBlock) { }
}