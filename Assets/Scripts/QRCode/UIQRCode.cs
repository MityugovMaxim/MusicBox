using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
public partial class UIQRCode
{
	bool m_ContentState;
	bool m_AnchorBLState;
	bool m_AnchorTLState;
	bool m_AnchorTRState;
	bool m_DirtyPosition;

	void LateUpdate()
	{
		bool dirty = false;
		
		if (m_Content != null && m_Content.gameObject.activeSelf ^ m_ContentState)
		{
			m_ContentState = m_Content.gameObject.activeSelf;
			dirty          = true;
		}
		if (m_AnchorBL != null && m_AnchorBL.gameObject.activeSelf ^ m_AnchorBLState)
		{
			m_AnchorBLState = m_AnchorBL.gameObject.activeSelf;
			dirty           = true;
		}
		if (m_AnchorTL != null && m_AnchorTL.gameObject.activeSelf ^ m_AnchorTLState)
		{
			m_AnchorTLState = m_AnchorTL.gameObject.activeSelf;
			dirty           = true;
		}
		if (m_AnchorTR != null && m_AnchorTR.gameObject.activeSelf ^ m_AnchorTRState)
		{
			m_AnchorTRState = m_AnchorTR.gameObject.activeSelf;
			dirty           = true;
		}
		
		if (dirty)
			Generate();
		
		if (m_DirtyPosition)
		{
			m_DirtyPosition = false;
			
			RepositionAnchors();
			
			RepositionContent();
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		
		GenerateMatrix();
		
		SetVerticesDirty();
		
		m_DirtyPosition = true;
	}
}
#endif

[ExecuteInEditMode]
public partial class UIQRCode : MaskableGraphic
{
	public enum Quality
	{
		Low    = 0,
		Medium = 1,
		High   = 2,
		Ultra  = 3
	}

	public string Message
	{
		get => m_Message;
		set
		{
			if (m_Message == value)
				return;
			
			m_Message = value;
			
			GenerateMatrix();
			
			SetVerticesDirty();
			
			RepositionAnchors();
			
			RepositionContent();
		}
	}

	public override Texture mainTexture => m_Body != null ? m_Body.texture : base.mainTexture;

	[SerializeField, HideInInspector] string m_Message;

	[SerializeField] Sprite     m_Body;
	[SerializeField] Sprite     m_Anchor;
	[SerializeField] Quality    m_Quality;
	[SerializeField] TextAnchor m_Alignment = TextAnchor.MiddleCenter;

	[Header("Anchors")]
	[SerializeField] Graphic m_AnchorBL;
	[SerializeField] Graphic m_AnchorTL;
	[SerializeField] Graphic m_AnchorTR;

	[Header("Content")]
	[SerializeField] RectTransform m_Content;
	[SerializeField] int           m_ContentSize = 11;
	
	[Header("Options")]
	[SerializeField] bool m_RotateAnchors;
	[SerializeField] bool m_RemovePointsBehindContent;

	readonly List<BitArray> m_Matrix   = new List<BitArray>();
	readonly List<UIVertex> m_Vertices = new List<UIVertex>();
	readonly List<int>      m_Indices  = new List<int>();

	public void Generate()
	{
		GenerateMatrix();
		
		RepositionAnchors();
		
		RepositionContent();
		
		SetAllDirty();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		RepositionAnchors();
		
		RepositionContent();
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		if (m_Matrix == null || m_Matrix.Count == 0)
			GenerateMatrix();
		
		m_Vertices.Clear();
		m_Indices.Clear();
		
		_VertexHelper.Clear();
		
		if (m_Matrix == null || m_Matrix.Count == 0)
			return;
		
		const float aspect = 1;
		
		Vector2 pivot = m_Alignment.GetPivot();
		
		Rect rect = MathUtility.Fit(rectTransform.rect, aspect, pivot);
		
		float step = Mathf.Min(
			rect.width / m_Matrix.Count,
			rect.height / m_Matrix.Count
		);
		
		Color32 color = this.color;
		
		Vector4 uv = m_Body != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(m_Body) : new Vector4(0, 0, 1, 1);
		
		void AddVertex(float _X, float _Y, float _U, float _V)
		{
			m_Vertices.Add(new UIVertex() { position = new Vector3(_X, _Y), uv0 = new Vector2(_U, _V), color = color });
		}
		
		void AddTriangle(int _A, int _B, int _C)
		{
			m_Indices.Add(_A);
			m_Indices.Add(_B);
			m_Indices.Add(_C);
		}
		
		void AddQuad(Rect _Rect, Vector4 _UV)
		{
			int index = m_Vertices.Count;
			
			AddVertex(_Rect.xMin, _Rect.yMax, _UV.x, _UV.w);
			AddVertex(_Rect.xMax, _Rect.yMax, _UV.z, _UV.w);
			AddVertex(_Rect.xMin, _Rect.yMin, _UV.x, _UV.y);
			AddVertex(_Rect.xMax, _Rect.yMin, _UV.z, _UV.y);
			
			AddTriangle(index + 0, index + 1, index + 2);
			AddTriangle(index + 2, index + 1, index + 3);
		}
		
		void AddPoint(int _X, int _Y)
		{
			AddQuad(new Rect(rect.x + _X * step, rect.y + _Y * step, step, step), uv);
		}
		
		for (int y = 0; y < m_Matrix.Count; y++)
		for (int x = 0; x < m_Matrix[y].Count; x++)
		{
			if (m_Matrix[y][x] == false)
				continue;
			
			Vector2Int point = new Vector2Int(x, y);
			
			if (FilterAnchors(point) || FilterContent(point))
				continue;
			
			AddPoint(x, y);
		}
		
		_VertexHelper.AddUIVertexStream(m_Vertices, m_Indices);
	}

	void GenerateMatrix()
	{
		m_Matrix.Clear();
		
		if (string.IsNullOrEmpty(Message))
			return;
		
		QRCodeGenerator.ErrorCorrection quality;
		switch (m_Quality)
		{
			case Quality.Low:
				quality = QRCodeGenerator.ErrorCorrection.L;
				break;
			case Quality.Medium:
				quality = QRCodeGenerator.ErrorCorrection.M;
				break;
			case Quality.High:
				quality = QRCodeGenerator.ErrorCorrection.Q;
				break;
			case Quality.Ultra:
				quality = QRCodeGenerator.ErrorCorrection.H;
				break;
			default:
				quality = QRCodeGenerator.ErrorCorrection.M;
				break;
		}
		
		List<BitArray> matrix = QRCodeGenerator.Generate(
			Message,
			m_Content != null
				? QRCodeGenerator.ErrorCorrection.H
				: quality
		);
		
		for (int i = matrix.Count - 1; i >= 0; i--)
			m_Matrix.Add(matrix[i]);
	}

	bool FilterAnchors(Vector2Int _Point)
	{
		int size = m_Matrix.Count;
		
		RectInt anchorBL = new RectInt(0, 0, 7, 7);
		if (m_AnchorBL != null && anchorBL.Contains(_Point))
			return true;
		
		RectInt anchorTL = new RectInt(0, size - 7, 7, 7);
		if (m_AnchorTL != null && anchorTL.Contains(_Point))
			return true;
		
		RectInt anchorTR = new RectInt(size - 7, size - 7, 7, 7);
		if (m_AnchorTR != null && anchorTR.Contains(_Point))
			return true;
		
		return false;
	}

	bool FilterContent(Vector2Int _Point)
	{
		if (!m_RemovePointsBehindContent || m_Content == null)
			return false;
		
		int maxContentSize = (int)(m_Matrix.Count * 0.3f);
		
		int contentSize = Mathf.Min(m_ContentSize, maxContentSize) / 2 * 2 + 1;
		
		RectInt contentRect = new RectInt(
			(m_Matrix.Count - contentSize) / 2,
			(m_Matrix.Count - contentSize) / 2,
			contentSize,
			contentSize
		);
		
		return contentRect.Contains(_Point);
	}

	void RepositionAnchors()
	{
		if (m_Matrix == null || m_Matrix.Count == 0)
			return;
		
		const float aspect = 1;
		
		Vector2 pivot = m_Alignment.GetPivot();
		
		Rect source = rectTransform.rect;
		Rect target = MathUtility.Fit(source, aspect, pivot);
		
		float step = Mathf.Min(
			target.width / m_Matrix.Count,
			target.height / m_Matrix.Count
		);
		
		Vector2 size = new Vector2(step, step);
		
		Vector2 anchorBL = new Vector2(0, 0);
		Vector2 anchorTL = new Vector2(0, 1);
		Vector2 anchorTR = new Vector2(1, 1);
		
		Vector2 positionBL = new Vector2(target.xMin - source.xMin, target.yMin - source.yMin);
		Vector2 positionTL = new Vector2(target.xMin - source.xMin, target.yMax - source.yMax);
		Vector2 positionTR = new Vector2(target.xMax - source.xMax, target.yMax - source.yMax);
		
		void SetAnchor(Graphic _Target, float _Rotation, Vector2 _Pivot, Vector2 _Anchor, Vector2 _Position)
		{
			if (_Target == null)
				return;
			
			_Target.color = color;
			
			if (_Target is Image image)
				image.sprite = m_Anchor;
			
			RectTransform transform = _Target.rectTransform;
			
			transform.sizeDelta        = size * 7;
			transform.pivot            = _Pivot;
			transform.anchorMin        = _Anchor;
			transform.anchorMax        = _Anchor;
			transform.localEulerAngles = new Vector3(0, 0, _Rotation);
			transform.anchoredPosition = _Position;
		}
		
		if (m_RotateAnchors)
		{
			SetAnchor(m_AnchorBL, 0, Vector2.zero, anchorBL, positionBL);
			SetAnchor(m_AnchorTL, 270, Vector2.zero, anchorTL, positionTL);
			SetAnchor(m_AnchorTR, 180, Vector2.zero, anchorTR, positionTR);
		}
		else
		{
			SetAnchor(m_AnchorBL, 0, new Vector2(0, 0), anchorBL, positionBL);
			SetAnchor(m_AnchorTL, 0, new Vector2(0, 1), anchorTL, positionTL);
			SetAnchor(m_AnchorTR, 0, new Vector2(1, 1), anchorTR, positionTR);
		}
	}

	void RepositionContent()
	{
		if (m_Matrix == null || m_Matrix.Count == 0)
			return;
		
		if (m_Content == null)
			return;
		
		const float aspect = 1;
		
		Vector2 pivot = m_Alignment.GetPivot();
		
		Rect source = rectTransform.rect;
		Rect target = MathUtility.Fit(source, aspect, pivot);
		
		float step = Mathf.Min(
			target.width / m_Matrix.Count,
			target.height / m_Matrix.Count
		);
		
		Vector2 size = new Vector2(step, step);
		
		Vector2 anchor = new Vector2(0.5f, 0.5f);
		
		Vector2 position = target.center - source.center;
		
		int maxContentSize = (int)(m_Matrix.Count * 0.3f);
		
		int contentSize = Mathf.Min(m_ContentSize, maxContentSize) / 2 * 2 + 1;
		
		if (contentSize <= 0 || Mathf.Approximately(step, 0) || float.IsNaN(step) || float.IsInfinity(step))
			return;
		
		m_Content.sizeDelta        = size * contentSize;
		m_Content.pivot            = anchor;
		m_Content.anchorMin        = anchor;
		m_Content.anchorMax        = anchor;
		m_Content.anchoredPosition = position;
	}
}