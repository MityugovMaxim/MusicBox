using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputReceiver : Graphic, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	public override Material material { get; set; }

	public override bool raycastTarget => true;

	float PixelSize { get; set; }

	[SerializeField] InputReader[] m_InputReaders;
	[SerializeField] Vector2       m_AreaSize;
	[SerializeField] Image         m_Background;
	[SerializeField] Image         m_Thumb;

	Vector2   m_Origin;
	Vector2   m_Position;
	InputType m_SwipeType;

	protected override void Awake()
	{
		base.Awake();
		
		CalcPixelSize();
	}

	void CalcPixelSize()
	{
		Rect rect = GetPixelAdjustedRect();
		
		PixelSize = Mathf.Min(rect.width / Screen.width, rect.height / Screen.height);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		CalcPixelSize();
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	public void OnPointerDown(PointerEventData _EventData)
	{
		StopAllCoroutines();
		
		Vector2 position = GetLocalPosition(_EventData.position, _EventData.pressEventCamera);
		
		m_Origin    = position;
		m_Position  = position;
		m_SwipeType = InputType.None;
		
		if (m_Background != null)
		{
			m_Background.rectTransform.localPosition = m_Origin;
			StartCoroutine(AlphaRoutine(m_Background, 0.5f, 0.2f));
		}
		
		if (m_Thumb != null)
		{
			m_Thumb.rectTransform.localPosition = m_Position;
			StartCoroutine(AlphaRoutine(m_Thumb, 1, 0.2f));
		}
		
		foreach (InputReader inputReader in m_InputReaders)
			inputReader.ProcessInput(InputType.TouchDown);
	}

	public void OnDrag(PointerEventData _EventData)
	{
		Vector2 position = GetLocalPosition(_EventData.position, _EventData.pressEventCamera);
		
		m_Position = position;
		
		Rect rect = new Rect(m_Origin - m_AreaSize * 0.5f, m_AreaSize);
		
		if (!rect.Contains(m_Position))
		{
			Vector2 delta = m_Position - m_Origin;
			
			m_Origin = m_Position;
			
			float dx = Mathf.Abs(delta.x);
			float dy = Mathf.Abs(delta.y);
			
			InputType swipeType;
			
			if (dx >= dy)
				swipeType = delta.x >= 0 ? InputType.SwipeRight : InputType.SwipeLeft;
			else
				swipeType = delta.y >= 0 ? InputType.SwipeUp : InputType.SwipeDown;
			
			if (m_SwipeType != swipeType)
			{
				m_SwipeType = swipeType;
				
				foreach (InputReader inputReader in m_InputReaders)
					inputReader.ProcessInput(swipeType);
			}
		}
		
		if (m_Background != null)
			m_Background.rectTransform.localPosition = m_Origin;
		
		if (m_Thumb != null)
			m_Thumb.rectTransform.localPosition = m_Position;
	}

	public void OnPointerUp(PointerEventData _EventData)
	{
		StopAllCoroutines();
		
		m_Position  = Vector2.zero;
		m_Origin    = Vector2.zero;
		m_SwipeType = InputType.None;
		
		if (m_Background != null)
			StartCoroutine(AlphaRoutine(m_Background, 0, 0.2f));
		
		if (m_Thumb != null)
			StartCoroutine(AlphaRoutine(m_Thumb, 0, 0.2f));
		
		foreach (InputReader inputReader in m_InputReaders)
			inputReader.ProcessInput(InputType.TouchUp);
	}

	static IEnumerator AlphaRoutine(Graphic _Graphic, float _Alpha, float _Duration)
	{
		Color source = _Graphic.color;
		Color target = new Color(source.r, source.g, source.b, _Alpha);
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			_Graphic.color = Color.Lerp(source, target, time / _Duration);
		}
		
		_Graphic.color = target;
	}

	Vector2 GetLocalPosition(Vector2 _Position, Camera _Camera)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rectTransform,
			_Position,
			_Camera,
			out Vector2 position
		);
		
		return position;
	}
}
