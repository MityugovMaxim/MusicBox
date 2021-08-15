using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[RequireComponent(typeof(CanvasGroup))]
public class UIProductPromo : UIEntity, IPointerClickHandler
{
	[SerializeField] UIProductPreviewLabel     m_Label;
	[SerializeField] UIProductPreviewPrice     m_Price;
	[SerializeField] UIProductPreviewThumbnail m_Thumbnail;

	PurchaseProcessor m_PurchaseProcessor;
	HapticProcessor   m_HapticProcessor;
	MenuProcessor     m_MenuProcessor;

	string      m_ProductID;
	CanvasGroup m_CanvasGroup;
	IEnumerator m_AlphaRoutine;

	[Inject]
	public void Construct(
		PurchaseProcessor _PurchaseProcessor,
		MenuProcessor     _MenuProcessor,
		HapticProcessor   _HapticProcessor
	)
	{
		m_PurchaseProcessor = _PurchaseProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_HapticProcessor   = _HapticProcessor;
		m_CanvasGroup       = GetComponent<CanvasGroup>();
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		if (string.IsNullOrEmpty(m_ProductID))
		{
			Hide();
			return;
		}
		
		void SetupInternal(bool _Success)
		{
			if (!_Success || m_PurchaseProcessor.IsProductPurchased(m_ProductID))
			{
				Hide();
				return;
			}
			
			m_Label.Setup(m_ProductID);
			m_Price.Setup(m_ProductID);
			m_Thumbnail.Setup(m_ProductID);
			
			Show();
		}
		
		m_PurchaseProcessor.OnInitialize += SetupInternal;
	}

	void Show()
	{
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		m_AlphaRoutine = AlphaRoutine(m_CanvasGroup, 1, 0.3f);
		
		m_CanvasGroup.interactable   = true;
		m_CanvasGroup.blocksRaycasts = true;
		
		StartCoroutine(m_AlphaRoutine);
	}

	void Hide()
	{
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		m_AlphaRoutine = AlphaRoutine(m_CanvasGroup, 0, 0.3f);
		
		m_CanvasGroup.interactable   = false;
		m_CanvasGroup.blocksRaycasts = false;
		
		StartCoroutine(m_AlphaRoutine);
	}

	static IEnumerator AlphaRoutine(CanvasGroup _CanvasGroup, float _Alpha, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactHeavy);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>(MenuType.ProductMenu);
		if (productMenu != null)
			productMenu.Setup(m_ProductID);
		m_MenuProcessor.Show(MenuType.ProductMenu);
	}
}