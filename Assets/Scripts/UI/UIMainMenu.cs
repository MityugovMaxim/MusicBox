using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : UIMenu
{
	[SerializeField] UIPreview     m_Preview;
	[SerializeField] LevelInfo[]   m_LevelInfos;
	[SerializeField] RectTransform m_GridContent;
	[SerializeField] CanvasGroup   m_GridGroup;
	[SerializeField] CanvasGroup   m_ControlGroup;
	[SerializeField] ScrollRect    m_Scroll;
	[SerializeField] UIPauseMenu   m_PauseMenu;
	[SerializeField] UIGameMenu    m_GameMenu;

	Preview[] m_Previews;
	int       m_PreviewIndex = -1;

	IEnumerator m_GridRoutine;
	IEnumerator m_ControlRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		Show(true);
		
		LoadPreviews();
	}

	public void ShowPreview(int _PreviewIndex)
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Show preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		if (m_PreviewIndex == _PreviewIndex)
		{
			HidePreview();
			return;
		}
		
		Preview preview = m_Previews[_PreviewIndex];
		if (preview == null)
		{
			Debug.LogErrorFormat(gameObject, "[UIMainMenu] Show preview failed. Preview is null at '{0}' index.", _PreviewIndex);
			HidePreview();
			return;
		}
		
		m_PreviewIndex = _PreviewIndex;
		
		EnableControl();
		DisableGrid();
		
		m_Preview.Show(preview);
	}

	public void HidePreview()
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Hide preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		m_PreviewIndex = -1;
		
		DisableControl();
		EnableGrid();
		
		if (m_Preview != null)
			m_Preview.Hide();
	}

	public void NextPreview()
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Next preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		int previewIndex = MathUtility.Repeat(m_PreviewIndex + 1, m_Previews.Length);
		
		Recenter(previewIndex);
		
		ShowPreview(previewIndex);
	}

	public void PreviousPreview()
	{
		if (m_Preview == null)
		{
			Debug.LogError("[UIMainMenu] Next preview failed. Preview is not assigned.", gameObject);
			return;
		}
		
		int previewIndex = MathUtility.Repeat(m_PreviewIndex - 1, m_Previews.Length);
		
		Recenter(previewIndex);
		
		ShowPreview(previewIndex);
	}

	public void Play()
	{
		if (m_PreviewIndex < 0 || m_PreviewIndex >= m_LevelInfos.Length)
		{
			Debug.LogError("[UIMainMenu] Play failed. Preview index is out of range.", gameObject);
			return;
		}
		
		LevelInfo levelInfo = m_LevelInfos[m_PreviewIndex];
		
		if (levelInfo == null)
		{
			Debug.LogError("[UIMainMenu] Play failed. Level is not assigned.", gameObject);
			return;
		}
		
		levelInfo.InstantiateLevel(
			null,
			_Level =>
			{
				Hide(
					false,
					() =>
					{
						if (m_PauseMenu != null)
							m_PauseMenu.Initialize(_Level);
						
						if (m_GameMenu != null)
						{
							m_GameMenu.Initialize(_Level, levelInfo.Title, levelInfo.Artist);
							m_GameMenu.Show(true);
						}
						
						_Level.Initialize();
					},
					_Level.Play
				);
			}
		);
	}

	void LoadPreviews()
	{
		int count = m_LevelInfos.Length;
		
		m_Previews = new Preview[count];
		
		int index = 0;
		foreach (var levelInfo in m_LevelInfos)
		{
			RectTransform mount = CreateMount(m_GridContent);
			
			int indexClosure = index;
			
			levelInfo.InstantiatePreview(
				mount,
				_Preview =>
				{
					m_Previews[indexClosure] = _Preview;
					
					_Preview.OnClick += () => ShowPreview(indexClosure);
				}
			);
			
			index++;
		}
	}

	void Recenter(int _PreviewIndex)
	{
		Rect source = m_Previews[_PreviewIndex].GetWorldRect();
		Rect target = m_Scroll.content.GetWorldRect();
		
		float position = MathUtility.Remap01(source.yMin, target.yMin, target.yMax - source.height);
		
		m_Scroll.StopMovement();
		m_Scroll.verticalNormalizedPosition = position;
	}

	protected override void OnHideFinished()
	{
		DisableControl(true);
		EnableGrid(true);
		
		m_PreviewIndex = -1;
		
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