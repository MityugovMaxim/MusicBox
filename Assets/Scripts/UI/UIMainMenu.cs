using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIMainMenu : UIMenu
{
	[Inject] Thumbnail.Factory m_ThumbnailFactory;
	[Inject] LevelProvider     m_LevelProvider;
	[Inject] UIPauseMenu       m_PauseMenu;
	[Inject] UIGameMenu        m_GameMenu;

	[SerializeField] UIPreview     m_Preview;
	[SerializeField] LevelInfo[]   m_LevelInfos;
	[SerializeField] RectTransform m_GridContent;
	[SerializeField] CanvasGroup   m_GridGroup;
	[SerializeField] CanvasGroup   m_ControlGroup;
	[SerializeField] ScrollRect    m_Scroll;

	Thumbnail[] m_Thumbnails;
	int         m_ThumbnailIndex = -1;

	IEnumerator m_GridRoutine;
	IEnumerator m_ControlRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		Show(true);
		
		LoadPreviews();
	}

	public void ShowPreview(int _ThumbnailIndex, bool _Instant = false)
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Show preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		if (m_ThumbnailIndex == _ThumbnailIndex)
		{
			HidePreview();
			return;
		}
		
		Thumbnail thumbnail = m_Thumbnails[_ThumbnailIndex];
		if (thumbnail == null)
		{
			Debug.LogErrorFormat(gameObject, "[UIMainMenu] Show preview failed. Thumbnail is null at '{0}' index.", _ThumbnailIndex);
			HidePreview();
			return;
		}
		
		m_ThumbnailIndex = _ThumbnailIndex;
		
		EnableControl(_Instant);
		DisableGrid(_Instant);
		
		m_Preview.Show(thumbnail, _Instant);
	}

	public void HidePreview(bool _Instant = false)
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Hide preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		m_ThumbnailIndex = -1;
		
		DisableControl(_Instant);
		EnableGrid(_Instant);
		
		m_Preview.Hide(_Instant);
	}

	public void NextPreview(bool _Instant = false)
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Next preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		int previewIndex = MathUtility.Repeat(m_ThumbnailIndex + 1, m_Thumbnails.Length);
		
		Recenter(previewIndex);
		
		ShowPreview(previewIndex, _Instant);
	}

	public void PreviousPreview(bool _Instant = false)
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Next preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		int previewIndex = MathUtility.Repeat(m_ThumbnailIndex - 1, m_Thumbnails.Length);
		
		Recenter(previewIndex);
		
		ShowPreview(previewIndex, _Instant);
	}

	public void Play()
	{
		if (m_ThumbnailIndex < 0 || m_ThumbnailIndex >= m_LevelInfos.Length)
		{
			Debug.LogError("[UIMainMenu] Play failed. Thumbnail index is out of range.", gameObject);
			return;
		}
		
		LevelInfo levelInfo = m_LevelInfos[m_ThumbnailIndex];
		
		if (levelInfo == null)
		{
			Debug.LogError("[UIMainMenu] Play failed. Level is not assigned.", gameObject);
			return;
		}
		
		m_LevelProvider.Create(levelInfo);
		
		if (m_GameMenu != null)
			m_GameMenu.Show(true);
		
		Hide(false, null, m_LevelProvider.Play);
	}

	void LoadPreviews()
	{
		int count = m_LevelInfos.Length;
		
		m_Thumbnails = new Thumbnail[count];
		
		int index = 0;
		foreach (var levelInfo in m_LevelInfos)
		{
			RectTransform mount = CreateMount(m_GridContent);
			
			Thumbnail thumbnail = m_ThumbnailFactory.Create($"{levelInfo.ID}/thumbnail", mount);
			
			var indexClosure = index;
			
			thumbnail.OnClick += () => ShowPreview(indexClosure);
			
			m_Thumbnails[index++] = thumbnail;
		}
	}

	void Recenter(int _PreviewIndex)
	{
		Rect source = m_Thumbnails[_PreviewIndex].GetWorldRect();
		Rect target = m_Scroll.content.GetWorldRect();
		
		float position = MathUtility.Remap01(source.yMin, target.yMin, target.yMax - source.height);
		
		m_Scroll.StopMovement();
		m_Scroll.verticalNormalizedPosition = position;
	}

	protected override void OnHideFinished()
	{
		DisableControl(true);
		EnableGrid(true);
		
		m_ThumbnailIndex = -1;
		
		if (m_Preview != null)
			m_Preview.Hide(true);
	}

	protected override void OnShowFinished()
	{
		if (m_PauseMenu != null)
			m_PauseMenu.Hide(true);
		
		if (m_GameMenu != null)
			m_GameMenu.Hide(true);
	}

	static RectTransform CreateMount(Transform _Parent)
	{
		GameObject mountObject = new GameObject("mount", typeof(RectTransform));
		
		RectTransform mount = mountObject.GetComponent<RectTransform>();
		
		mount.SetParent(_Parent, false);
		
		return mount;
	}

	void EnableGrid(bool _Instant = false)
	{
		if (m_GridRoutine != null)
			StopCoroutine(m_GridRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_GridGroup.alpha          = 1;
			m_GridGroup.blocksRaycasts = true;
		}
		else
		{
			m_GridRoutine = EnableGroupRoutine(m_GridGroup, 0.2f);
			
			StartCoroutine(m_GridRoutine);
		}
	}

	void DisableGrid(bool _Instant = false)
	{
		if (m_GridRoutine != null)
			StopCoroutine(m_GridRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_GridGroup.alpha          = 0;
			m_GridGroup.blocksRaycasts = false;
		}
		else
		{
			m_GridRoutine = DisableGroupRoutine(m_GridGroup, 0.2f);
			
			StartCoroutine(m_GridRoutine);
		}
	}

	void EnableControl(bool _Instant = false)
	{
		if (m_ControlRoutine != null)
			StopCoroutine(m_ControlRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_ControlGroup.alpha          = 1;
			m_ControlGroup.blocksRaycasts = true;
		}
		else
		{
			m_ControlRoutine = EnableGroupRoutine(m_ControlGroup, 0.2f);
			
			StartCoroutine(m_ControlRoutine);
		}
	}

	void DisableControl(bool _Instant = false)
	{
		if (m_ControlRoutine != null)
			StopCoroutine(m_ControlRoutine);
		
		if (_Instant)
		{
			m_ControlGroup.alpha          = 0;
			m_ControlGroup.blocksRaycasts = false;
		}
		else
		{
			m_ControlRoutine = DisableGroupRoutine(m_ControlGroup, 0.2f);
			
			StartCoroutine(m_ControlRoutine);
		}
	}

	static IEnumerator EnableGroupRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = 1;
		
		if (!Mathf.Approximately(source, target) && _Duration > float.Epsilon)
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
		
		_CanvasGroup.blocksRaycasts = true;
	}

	static IEnumerator DisableGroupRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = 0;
		
		_CanvasGroup.blocksRaycasts = false;
		
		if (!Mathf.Approximately(source, target) && _Duration > float.Epsilon)
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
}