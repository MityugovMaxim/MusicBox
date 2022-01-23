using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(CanvasGroup))]
public class UIProductPromo : UIGroup, IPointerClickHandler
{
	[SerializeField] UIProductLabel     m_Label;
	[SerializeField] UIProductPrice     m_Price;
	[SerializeField] UIProductThumbnail m_Thumbnail;

	ProfileProcessor m_ProfileProcessor;
	HapticProcessor  m_HapticProcessor;
	MenuProcessor    m_MenuProcessor;

	string m_ProductID;

	[Inject]
	public void Construct(
		ProfileProcessor _ProfileProcessor,
		MenuProcessor    _MenuProcessor,
		HapticProcessor  _HapticProcessor
	)
	{
		m_ProfileProcessor = _ProfileProcessor;
		m_MenuProcessor    = _MenuProcessor;
		m_HapticProcessor  = _HapticProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		if (string.IsNullOrEmpty(m_ProductID))
		{
			Hide();
			return;
		}
		
		if (m_ProfileProcessor.HasProduct(m_ProductID))
		{
			Hide();
			return;
		}
		
		try
		{
			m_Label.Setup(m_ProductID);
			m_Price.Setup(m_ProductID);
			m_Thumbnail.Setup(m_ProductID);
			Show();
		}
		catch
		{
			Hide();
		}
	}

	protected override IEnumerator ShowAnimationRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return new WaitForSeconds(2);
		
		LayoutElement layoutElement = GetComponent<LayoutElement>();
		
		if (layoutElement != null)
		{
			float       time     = 0;
			float       source   = layoutElement.preferredHeight;
			const float duration = 0.2f;
			const float target   = 150;
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				layoutElement.preferredHeight = Mathf.Lerp(source, target, time / duration);
			}
			layoutElement.preferredHeight = target;
		}
		
		yield return base.ShowAnimationRoutine(_CanvasGroup, _Duration);
	}

	protected override IEnumerator HideAnimationRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return base.HideAnimationRoutine(_CanvasGroup, _Duration);
		
		LayoutElement layoutElement = GetComponent<LayoutElement>();
		
		if (layoutElement != null)
		{
			float       time     = 0;
			float       source   = layoutElement.preferredHeight;
			const float duration = 0.2f;
			const float target   = 0;
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				layoutElement.preferredHeight = Mathf.Lerp(source, target, time / duration);
			}
			layoutElement.preferredHeight = target;
		}
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactHeavy);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(m_ProductID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Store, true);
	}
}