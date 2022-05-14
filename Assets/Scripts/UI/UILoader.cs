using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class UILoader : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	[ContextMenu("Generate")]
	public void Generate()
	{
		Color   sourceColor = new Color(1, 1, 1);
		Color   targetColor = new Color(0.25f, 0.75f, 1f);
		Vector2 sourceSize  = new Vector2(20, 20);
		Vector2 targetSize  = new Vector2(20, 50);
		Image[] images      = GetComponentsInChildren<Image>();
		foreach (Image image in images)
		{
			float phase = Random.value;
			
			image.rectTransform.sizeDelta = Vector2.Lerp(sourceSize, targetSize, phase);
			
			image.color = Color.Lerp(sourceColor, targetColor, phase);
		}
	}
}