using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageSize : MonoBehaviour
{
	[SerializeField] float m_Base;
	[SerializeField] float m_Size;
	[SerializeField] Image m_Image;

	void OnEnable()
	{
		ProcessSize();
	}

	void OnDidApplyAnimationProperties()
	{
		ProcessSize();
	}

	#if UNITY_EDITOR
	void OnValidate()
	{
		ProcessSize();
	}
	#endif

	void ProcessSize()
	{
		m_Image.pixelsPerUnitMultiplier = m_Base / m_Size;
		m_Image.SetVerticesDirty();
	}
}