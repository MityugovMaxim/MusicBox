using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;
using Object = UnityEngine.Object;

public class AtlasPacker : EditorWindow
{
	#region nested types

	public enum SortingType
	{
		AreaAsc,
		AreaDesc,
		WidthAsc,
		WidthDesc,
		HeightAsc,
		HeightDesc,
	}

	public enum NoiseType
	{
		None,
		Low,
		Medium,
		High,
	}

	#endregion

	#region factory methods

	[MenuItem("Tools/Atlas packer...")]
	public static void Open()
	{
		AtlasPacker window = GetWindow<AtlasPacker>(true, "Atlas packer", true);
		window.minSize = new Vector2(500, 500);
		window.Show();
	}

	#endregion

	#region attributes

	static readonly int m_NoiseTexPropertyID   = Shader.PropertyToID("_NoiseTex");
	static readonly int m_NoiseScalePropertyID = Shader.PropertyToID("_NoiseScale");
	static readonly int m_NoisePropertyID      = Shader.PropertyToID("_Noise");

	[SerializeField] List<Sprite> m_Sprites     = new List<Sprite>();
	[SerializeField] List<Rect>   m_Rects       = new List<Rect>();
	[SerializeField] NoiseType    m_NoiseType   = NoiseType.None;
	[SerializeField] bool         m_POT         = true;
	[SerializeField] int          m_Padding     = 1;
	[SerializeField] float        m_Scale       = 1;
	[SerializeField] Vector2      m_Position    = Vector2.zero;

	RenderTexture m_Atlas;

	#endregion

	#region engine methods

	void Awake()
	{
		m_Scale    = 1;
		m_Position = Vector2.zero;
	}

	void OnSelectionChange()
	{
		Repaint();
	}

	void OnGUI()
	{
		if (m_Atlas != null)
		{
			float atlasAspect = (float)m_Atlas.width / m_Atlas.height;
			Rect  atlasRect   = MathUtility.Fit(new Rect(0, 0, position.width, position.height), atlasAspect);
			
			SetupMatrix();
			
			DrawAtlas(atlasRect);
			DrawBounds(atlasRect);
			
			ResetMatrix();
			
			float toolsWidth  = Mathf.Clamp(position.width * 0.4f, 0, 270);
			float toolsHeight = 88;
			
			Rect toolsRect = new Rect(
				position.width - toolsWidth - 20,
				position.height - toolsHeight - 20,
				toolsWidth,
				toolsHeight
			);
			
			DrawTools(toolsRect);
			
			SetupMatrix();
			
			ZoomInput(atlasRect);
			MoveInput(atlasRect);
			SelectInput(atlasRect);
			SelectAllInput();
			DeleteInput();
			
			ResetMatrix();
		}
		else
		{
			Rect placeholderRect = new Rect(0, 0, position.width, position.height);
			
			DrawPlaceholder(placeholderRect);
		}
		
		DragDropInput();
	}

	#endregion

	#region draw methods

	void DrawPlaceholder(Rect _Rect)
	{
		Rect rect = new RectOffset(30, 30, 30, 30).Remove(_Rect);
		
		Handles.DrawDottedLine(
			new Vector2(rect.xMin, rect.yMin), 
			new Vector2(rect.xMax, rect.yMin),
			5
		);
		
		Handles.DrawDottedLine(
			new Vector2(rect.xMin, rect.yMax), 
			new Vector2(rect.xMax, rect.yMax),
			5
		);
		
		Handles.DrawDottedLine(
			new Vector2(rect.xMin, rect.yMin), 
			new Vector2(rect.xMin, rect.yMax),
			5
		);
		
		Handles.DrawDottedLine(
			new Vector2(rect.xMax, rect.yMin), 
			new Vector2(rect.xMax, rect.yMax),
			5
		);
		
		EditorGUI.DropShadowLabel(
			new RectOffset(10, 10, 10, 10).Remove(rect),
			"Drag & Drop textures to create atlas"
		);
	}

	void DrawAtlas(Rect _Rect)
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		EditorGUI.DrawTextureTransparent(_Rect, m_Atlas);
		
		ResetMatrix();
		EditorGUI.DropShadowLabel(
			new Rect(_Rect.x, _Rect.yMax - 35, _Rect.width, 30),
			$"{m_Atlas.width}x{m_Atlas.height}"
		);
		SetupMatrix();
	}

	void DrawBounds(Rect _Rect)
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				foreach (Rect rect in m_Rects)
				{
					Handles.DrawSolidRectangleWithOutline(
						GetLocalRect(_Rect, rect),
						Color.clear,
						Color.white
					);
				}
				
				for (int i = 0; i < m_Sprites.Count; i++)
				{
					Sprite  sprite = m_Sprites[i];
					Rect    rect   = m_Rects[i];
					Vector2 pivot  = sprite.pivot;
					
					Rect border = new Rect(
						rect.x + sprite.border.x,
						rect.y + sprite.border.y,
						rect.width - sprite.border.x - sprite.border.z,
						rect.height - sprite.border.y - sprite.border.w
					);
					
					pivot  = GetLocalPosition(_Rect, rect.position + pivot);
					rect   = GetLocalRect(_Rect, rect);
					border = GetLocalRect(_Rect, border);
					
					if (border.xMin > rect.xMin)
						Handles.DrawDottedLine(new Vector2(border.xMin, rect.yMin), new Vector2(border.xMin, rect.yMax), 5);
					
					if (border.xMax < rect.xMax)
						Handles.DrawDottedLine(new Vector2(border.xMax, rect.yMin), new Vector2(border.xMax, rect.yMax), 5);
					
					if (border.yMin > rect.yMin)
						Handles.DrawDottedLine(new Vector2(rect.xMin, border.yMin), new Vector2(rect.xMax, border.yMin), 5);
					
					if (border.yMax < rect.yMax)
						Handles.DrawDottedLine(new Vector2(rect.xMin, border.yMax), new Vector2(rect.xMax, border.yMax), 5);
					
					Handles.color = new Color(1, 1, 1, 0.5f);
					Handles.DrawSolidDisc(pivot, Vector3.forward, 5 / m_Scale);
					Handles.color = new Color(0, 0.65f, 1);
					Handles.DrawWireDisc(pivot, Vector3.forward, 5 / m_Scale);
					Handles.color = Color.white;
				}
				
				break;
			}
		}
	}

	void DrawTools(Rect _Rect)
	{
		EditorGUI.DrawRect(_Rect, new Color(0.12f, 0.12f, 0.12f, 0.75f));
		
		GUILayout.BeginArea(new RectOffset(8, 8, 4, 4).Remove(_Rect));
		
		bool pot = EditorGUILayout.Toggle("Power of Two", m_POT);
		
		if (m_POT != pot)
		{
			m_POT = pot;
			Pack();
		}
		
		NoiseType noiseType = (NoiseType)EditorGUILayout.EnumPopup("Noise", m_NoiseType);
		if (m_NoiseType != noiseType)
		{
			m_NoiseType = noiseType;
			Pack();
		}
		
		int padding = EditorGUILayout.DelayedIntField("Padding", m_Padding);
		
		if (m_Padding != padding)
		{
			m_Padding = padding;
			Pack();
		}
		
		if (GUILayout.Button("Export..."))
			Export();
		
		GUILayout.EndArea();
		
		AudioCurveRendering.DrawCurveFrame(_Rect);
	}

	#endregion

	#region input methods

	void ZoomInput(Rect _Rect)
	{
		switch (Event.current.type)
		{
			case EventType.ScrollWheel:
			{
				if (!_Rect.Contains(Event.current.mousePosition) || Event.current.modifiers != EventModifiers.Command)
					break;
				
				Vector2 delta = Event.current.delta;
				
				Vector2 sourceSize = _Rect.size * m_Scale;
				
				m_Scale = Mathf.Clamp(m_Scale - delta.y * 0.025f, 1, 20);
				
				Vector2 targetSize = _Rect.size * m_Scale;
				
				Vector2 pivot = new Vector2(
					Event.current.mousePosition.x / _Rect.width,
					Event.current.mousePosition.y / _Rect.height
				);
				
				m_Position -= Vector2.Scale(targetSize - sourceSize, pivot);
				
				ClampPosition(_Rect);
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	static int MoveControlID => GUIUtility.GetControlID("atlas_packer_move".GetHashCode(), FocusType.Passive);

	void MoveInput(Rect _Rect)
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				if (GUIUtility.hotControl != MoveControlID)
					break;
				
				EditorGUIUtility.AddCursorRect(
					new Rect(0, 0, position.width, position.height),
					MouseCursor.Pan
				);
				
				break;
			}
			
			case EventType.ScrollWheel:
			{
				m_Position -= Event.current.delta * m_Scale;
				
				ClampPosition(_Rect);
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (Event.current.button == 2 || Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt)
				{
					GUIUtility.hotControl = MoveControlID;
					
					Event.current.Use();
					
					Repaint();
				}
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl != MoveControlID)
					break;
				
				m_Position += Event.current.delta * m_Scale;
				
				ClampPosition(_Rect);
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl != MoveControlID)
					break;
				
				GUIUtility.hotControl = 0;
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void SelectAllInput()
	{
		switch (Event.current.type)
		{
			case EventType.ValidateCommand:
			{
				if (Event.current.commandName != "SelectAll")
					break;
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName != "SelectAll")
					break;
				
				Selection.objects = m_Sprites.OfType<Object>().ToArray();
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void SelectInput(Rect _Rect)
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				for (int i = 0; i < m_Sprites.Count; i++)
				{
					if (!Selection.Contains(m_Sprites[i]))
						continue;
					
					Handles.DrawSolidRectangleWithOutline(
						GetLocalRect(_Rect, m_Rects[i]),
						Color.clear,
						new Color(0.25f, 0.65f, 0.85f)
					);
				}
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (!_Rect.Contains(Event.current.mousePosition))
					break;
				
				GUI.FocusControl(null);
				
				List<Sprite> sprites = Selection.GetFiltered<Sprite>(SelectionMode.Editable).ToList();
				
				Sprite sprite = null;
				
				for (int i = 0; i < m_Rects.Count; i++)
				{
					Rect rect = GetLocalRect(_Rect, m_Rects[i]);
					
					if (!rect.Contains(Event.current.mousePosition))
						continue;
					
					sprite = m_Sprites[i];
					
					break;
				}
				
				if (sprite != null)
				{
					if (Event.current.modifiers == EventModifiers.Command)
					{
						if (sprites.Contains(sprite))
							sprites.Remove(sprite);
						else
							sprites.Add(sprite);
					}
					else
					{
						sprites.Clear();
						sprites.Add(sprite);
					}
				}
				
				Selection.objects = sprites.Distinct().OfType<Object>().ToArray();
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void DeleteInput()
	{
		switch (Event.current.type)
		{
			case EventType.ValidateCommand:
			{
				if (Event.current.commandName != "Delete")
					break;
				
				foreach (Sprite sprite in m_Sprites)
				{
					if (!Selection.Contains(sprite))
						continue;
					
					Event.current.Use();
					
					break;
				}
				
				break;
			}
			
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName != "Delete")
					break;
				
				for (int i = m_Sprites.Count - 1; i >= 0; i--)
				{
					if (Selection.Contains(m_Sprites[i]))
						m_Sprites.RemoveAt(i);
				}
				
				Event.current.Use();
				
				Pack();
				
				Repaint();
				
				break;
			}
		}
	}

	void DragDropInput()
	{
		Rect rect = new Rect(0, 0, position.width, position.height);
		
		switch (Event.current.type)
		{
			case EventType.DragUpdated:
			{
				if (!rect.Contains(Event.current.mousePosition))
					break;
				
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.DragPerform:
			{
				if (!rect.Contains(Event.current.mousePosition))
					break;
				
				Texture2D[] textures = DragAndDrop.objectReferences.OfType<Texture2D>().ToArray();
				
				Sprite[] sprites = DragAndDrop.objectReferences.OfType<Sprite>().ToArray();
				
				DragAndDrop.AcceptDrag();
				
				foreach (Texture2D texture in textures)
					AddTexture(texture);
				
				foreach (Sprite sprite in sprites)
					AddSprite(sprite);
				
				Pack();
				
				Repaint();
				
				Event.current.Use();
				
				break;
			}
		}
	}

	#endregion

	#region service methods

	void SetupMatrix()
	{
		GUI.matrix = Matrix4x4.TRS(
			m_Position,
			Quaternion.identity,
			Vector3.one * m_Scale
		);
	}

	void ResetMatrix()
	{
		GUI.matrix = Matrix4x4.identity;
	}

	void AddTexture(Texture2D _Texture)
	{
		if (_Texture == null || !AssetDatabase.Contains(_Texture))
			return;
		
		string path = AssetDatabase.GetAssetPath(_Texture);
		
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		
		if (importer != null && importer.textureType == TextureImporterType.Sprite)
		{
			if (importer.spriteImportMode == SpriteImportMode.Multiple)
				AddMultipleTexture(_Texture, importer.spritesheet);
			else
				AddSingleTexture(_Texture, importer.spritePivot, importer.spriteBorder);
		}
		else
		{
			AddDefaultTexture(_Texture);
		}
	}

	void AddSprite(Sprite _Sprite)
	{
		if (_Sprite == null || _Sprite.texture == null)
			return;
		
		Texture2D texture = new Texture2D(
			(int)_Sprite.rect.width,
			(int)_Sprite.rect.height,
			TextureFormat.ARGB32,
			false
		);
		
		texture.name = _Sprite.name;
		
		CopyTexture(
			_Sprite.texture,
			texture,
			new RectInt(
				(int)_Sprite.rect.x,
				(int)_Sprite.rect.y,
				(int)_Sprite.rect.width,
				(int)_Sprite.rect.height
			),
			new Vector2Int(0, 0)
		);
		
		Sprite sprite = Sprite.Create(
			texture,
			new Rect(Vector2.zero, _Sprite.rect.size),
			new Vector2(
				_Sprite.pivot.x / _Sprite.rect.width,
				_Sprite.pivot.y / _Sprite.rect.height
			),
			1,
			0,
			SpriteMeshType.FullRect,
			_Sprite.border
		);
		
		sprite.name = texture.name;
		
		// Replace sprite
		for (int i = 0; i < m_Sprites.Count; i++)
		{
			if (m_Sprites[i] == null || m_Sprites[i].name != sprite.name)
				continue;
			
			m_Sprites[i] = sprite;
			
			return;
		}
		
		// Add sprite
		m_Sprites.Add(sprite);
	}

	void AddMultipleTexture(Texture2D _Texture, SpriteMetaData[] _SpriteSheet)
	{
		if (_Texture.format != TextureFormat.ARGB32)
			_Texture = ReformatTexture(_Texture);
		
		Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		
		foreach (SpriteMetaData spriteData in _SpriteSheet)
		{
			Rect rect = spriteData.rect;
			
			Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
			
			texture.name = spriteData.name;
			
			CopyTexture(
				_Texture,
				texture,
				new RectInt((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height),
				new Vector2Int(0, 0)
			);
			
			SpriteAlignment alignment = (SpriteAlignment)spriteData.alignment;
			
			Sprite sprite = Sprite.Create(
				texture,
				new Rect(0, 0, (int)rect.width, (int)rect.height),
				GetPivot(alignment, spriteData.pivot),
				1,
				0,
				SpriteMeshType.FullRect,
				spriteData.border,
				false
			);
			
			sprite.name = spriteData.name;
			
			sprites[sprite.name] = sprite;
		}
		
		// Replace textures
		for (int i = 0; i < m_Sprites.Count; i++)
		{
			Sprite sprite = m_Sprites[i];
			if (sprite != null && sprites.ContainsKey(sprite.name))
			{
				m_Sprites[i] = sprites[sprite.name];
				sprites.Remove(sprite.name);
			}
		}
		
		// Add textures
		m_Sprites.AddRange(sprites.Values);
	}

	void AddSingleTexture(Texture2D _Texture, Vector2 _Pivot, Vector4 _Border)
	{
		if (_Texture.format != TextureFormat.ARGB32)
			_Texture = ReformatTexture(_Texture);
		
		Texture2D texture = new Texture2D(_Texture.width, _Texture.height, TextureFormat.ARGB32, false);
		
		texture.name = _Texture.name;
		
		CopyTexture(
			_Texture,
			texture,
			new RectInt(0, 0, _Texture.width, _Texture.height),
			new Vector2Int(0, 0)
		);
		
		Sprite sprite = Sprite.Create(
			texture,
			new Rect(0, 0, _Texture.width, _Texture.height),
			_Pivot,
			1,
			0,
			SpriteMeshType.FullRect,
			_Border,
			false
		);
		
		sprite.name = texture.name;
		
		// Replace texture
		for (int i = 0; i < m_Sprites.Count; i++)
		{
			if (m_Sprites[i] == null || m_Sprites[i].name != sprite.name)
				continue;
			
			m_Sprites[i] = sprite;
			
			return;
		}
		
		// Add texture
		m_Sprites.Add(sprite);
	}

	void AddDefaultTexture(Texture2D _Texture)
	{
		if (_Texture.format != TextureFormat.ARGB32)
			_Texture = ReformatTexture(_Texture);
		
		Texture2D texture = new Texture2D(_Texture.width, _Texture.height, TextureFormat.ARGB32, false);
		
		texture.name = _Texture.name;
		
		CopyTexture(
			_Texture,
			texture,
			new RectInt(0, 0, texture.width, texture.height),
			new Vector2Int(0, 0)
		);
		
		Sprite sprite = Sprite.Create(
			texture,
			new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.5f, 0.5f),
			1,
			0,
			SpriteMeshType.FullRect,
			Vector4.zero,
			false
		);
		
		sprite.name = texture.name;
		
		// Replace texture
		for (int i = 0; i < m_Sprites.Count; i++)
		{
			if (m_Sprites[i] == null || m_Sprites[i].name != sprite.name)
				continue;
			
			m_Sprites[i] = sprite;
			
			return;
		}
		
		// Add texture
		m_Sprites.Add(sprite);
	}

	Rect GetLocalRect(Rect _Container, Rect _Rect)
	{
		Vector2 scale = new Vector2(
			_Container.width / m_Atlas.width,
			_Container.height / m_Atlas.height
		);
		
		return new Rect(
			_Container.x + _Rect.x * scale.x,
			_Container.yMax - (_Rect.y + _Rect.height) * scale.y,
			_Rect.width * scale.x,
			_Rect.height * scale.y
		);
	}

	Vector2 GetLocalPosition(Rect _Container, Vector2 _Position)
	{
		Vector2 scale = new Vector2(
			_Container.width / m_Atlas.width,
			_Container.height / m_Atlas.height
		);
		
		return new Vector2(
			_Container.position.x + _Position.x * scale.x,
			_Container.yMax - _Position.y * scale.y
		);
	}

	void Pack()
	{
		SortingType minSortingType = SortingType.AreaAsc;
		int         minArea        = int.MaxValue;
		foreach (SortingType sortingType in Enum.GetValues(typeof(SortingType)))
		{
			Pack(sortingType);
			
			if (m_Atlas == null)
				break;
			
			int area = m_Atlas.width * m_Atlas.height;
			
			if (minArea >= area)
			{
				minArea        = area;
				minSortingType = sortingType;
			}
		}
		
		Pack(minSortingType);
	}

	void Pack(SortingType _SortingType)
	{
		if (m_Sprites == null || m_Sprites.Count == 0)
		{
			m_Atlas = null;
			return;
		}
		
		m_Sprites.Sort(
			(_A, _B) =>
			{
				float aValue = 0;
				float bValue = 0;
				
				switch (_SortingType)
				{
					case SortingType.AreaAsc:
						aValue = _A.rect.width * _A.rect.height;
						bValue = _B.rect.width * _B.rect.height;
						break;
					case SortingType.AreaDesc:
						aValue = _B.rect.width * _B.rect.height;
						bValue = _A.rect.width * _A.rect.height;
						break;
					case SortingType.WidthAsc:
						aValue = _A.rect.width;
						bValue = _B.rect.width;
						break;
					case SortingType.WidthDesc:
						aValue = _B.rect.width;
						bValue = _A.rect.width;
						break;
					case SortingType.HeightAsc:
						aValue = _A.rect.height;
						bValue = _B.rect.height;
						break;
					case SortingType.HeightDesc:
						aValue = _B.rect.height;
						bValue = _A.rect.height;
						break;
				}
				
				int value = aValue.CompareTo(bValue);
				
				return value != 0 ? value : string.CompareOrdinal(_A.name, _B.name);
			}
		);
		
		const int step = 1;
		
		float maxWidth  = m_Sprites.Max(_Sprite => _Sprite.rect.width);
		float maxHeight = m_Sprites.Max(_Sprite => _Sprite.rect.height);
		float minSize   = Mathf.Max(maxWidth, maxHeight);
		
		int size = Mathf.CeilToInt(Mathf.Sqrt(minSize * minSize));
		
		Vector2[] sizes = m_Sprites.Select(_Sprite => _Sprite.rect.size).ToArray();
		
		while (true)
		{
			Texture2D.GenerateAtlas(sizes, m_Padding, size, m_Rects);
			
			bool success = m_Rects.Any(_Rect => _Rect.width > float.Epsilon && _Rect.height > float.Epsilon);
			
			if (success)
				break;
			
			size += step;
		}
		
		while (size > minSize && Compress(size))
			size--;
		
		int width;
		int height;
		
		if (m_POT)
		{
			size   = !Mathf.IsPowerOfTwo(size) ? Mathf.NextPowerOfTwo(size) : size;
			width  = size;
			height = size;
		}
		else
		{
			width  = Mathf.CeilToInt(m_Rects.Max(_Rect => _Rect.xMax));
			height = Mathf.CeilToInt(m_Rects.Max(_Rect => _Rect.yMax));
		}
		
		if (m_Atlas != null)
			m_Atlas.Release();
		
		m_Atlas = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		
		for (int i = 0; i < m_Rects.Count; i++)
			Pack(m_Sprites[i].texture, m_Rects[i]);
		
		Noise();
	}

	void Pack(Texture _Texture, Rect _Rect)
	{
		CopyTexture(
			_Texture,
			m_Atlas,
			new RectInt(0, 0, (int)_Rect.width, (int)_Rect.height),
			new Vector2Int((int)_Rect.x, (int)_Rect.y)
		);
	}

	bool Compress(int _Size)
	{
		List<Sprite> sprites = new List<Sprite>(m_Sprites);
		List<Rect>   rects   = new List<Rect>();
		
		for (int i = 0; i < sprites.Count; i++)
		for (int j = 0; j < sprites.Count; j++)
		{
			Sprite spriteBuffer = sprites[i];
			sprites[i] = sprites[j];
			sprites[j] = spriteBuffer;
			
			Texture2D.GenerateAtlas(
				sprites.Select(_Sprite => _Sprite.rect.size).ToArray(),
				m_Padding,
				_Size - 1,
				rects
			);
			
			bool success = rects.Any(_Rect => _Rect.width > float.Epsilon && _Rect.height > float.Epsilon);
			
			if (!success)
				continue;
			
			m_Sprites.Clear();
			m_Sprites.AddRange(sprites);
			
			m_Rects.Clear();
			m_Rects.AddRange(rects);
			
			return true;
		}
		return false;
	}

	void Noise()
	{
		if (m_NoiseType == NoiseType.None)
			return;
		
		float value;
		switch (m_NoiseType)
		{
			case NoiseType.Low:
				value = 0.1f;
				break;
			case NoiseType.Medium:
				value = 0.2f;
				break;
			case NoiseType.High:
				value = 0.3f;
				break;
			default:
				value = 0.1f;
				break;
		}
		
		Texture  noise    = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Slots/Editor/Data/noise.png");
		Shader   shader   = Shader.Find("Slots/Noise");
		Material material = new Material(shader);
		
		float scale = Mathf.Max(
			(float)m_Atlas.width / noise.width,
			(float)m_Atlas.height / noise.height
		);
		
		material.SetTexture(m_NoiseTexPropertyID, noise);
		material.SetFloat(m_NoiseScalePropertyID, scale);
		material.SetFloat(m_NoisePropertyID, value);
		
		Graphics.Blit(m_Atlas, m_Atlas, material);
	}

	void Export()
	{
		Pack();
		
		if (m_Atlas == null || m_Atlas.width == 0 || m_Atlas.height == 0)
			return;
		
		string path = EditorUtility.SaveFilePanelInProject(
			"Export atlas",
			"atlas",
			"png",
			"Select folder to save atlas"
		);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		Texture2D atlas = ReformatTexture(m_Atlas);
		
		byte[] data = atlas.EncodeToPNG();
		
		File.WriteAllBytes(path, data);
		
		AssetDatabase.ImportAsset(path);
		
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		
		if (importer != null)
		{
			importer.textureType         = TextureImporterType.Sprite;
			importer.spriteImportMode    = SpriteImportMode.Multiple;
			importer.npotScale           = TextureImporterNPOTScale.None;
			importer.textureCompression  = TextureImporterCompression.Uncompressed;
			importer.mipmapEnabled       = false;
			importer.alphaIsTransparency = true;
			importer.spritePixelsPerUnit = 1;
			
			TextureImporterSettings settings = new TextureImporterSettings();
			importer.ReadTextureSettings(settings);
			settings.spriteMeshType = SpriteMeshType.FullRect;
			settings.readable       = false;
			settings.wrapMode       = TextureWrapMode.Clamp;
			importer.SetTextureSettings(settings);
			
			Vector2[] pivots =
			{
				new Vector2(0.5f, 0.5f), // Center
				new Vector2(0, 1),       // TopLeft
				new Vector2(0.5f, 1),    // TopCenter
				new Vector2(1, 1),       // TopRight
				new Vector2(0, 0.5f),    // LeftCenter
				new Vector2(1, 0.5f),    // RightCenter
				new Vector2(0, 0),       // BottomLeft
				new Vector2(0.5f, 0),    // BottomCenter
				new Vector2(1, 0),       // BottomRight
			};
			
			List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
			for (int i = 0; i < m_Rects.Count; i++)
			{
				Sprite sprite = m_Sprites[i];
				
				Vector2 pivot = new Vector2(
					sprite.pivot.x / sprite.rect.width,
					sprite.pivot.y / sprite.rect.height
				);
				
				int pivotIndex = Array.IndexOf(pivots, pivot);
				
				SpriteMetaData spriteData = new SpriteMetaData();
				
				if (pivotIndex >= 0)
				{
					spriteData.alignment = pivotIndex;
				}
				else
				{
					spriteData.alignment = (int)SpriteAlignment.Custom;
					spriteData.pivot     = pivot;
				}
				
				spriteData.rect      = m_Rects[i];
				spriteData.name      = sprite.name;
				spriteData.border    = sprite.border;
				spritesheet.Add(spriteData);
			}
			importer.spritesheet = spritesheet.ToArray();
			importer.SaveAndReimport();
		}
		
		AssetDatabase.SaveAssets();
	}

	void ClampPosition(Rect _Rect)
	{
		SetupMatrix();
		
		_Rect = new RectOffset(100, 100, 100, 100).Add(_Rect);
		
		Vector2 min = _Rect.min;
		Vector2 max = _Rect.max;
		
		min = GUI.matrix.MultiplyPoint(min);
		max = GUI.matrix.MultiplyPoint(max);
		
		Rect rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		
		float horizontal = Mathf.Max(0, rect.xMin - _Rect.xMin) - Mathf.Max(0, _Rect.xMax - rect.xMax);
		float vertical   = Mathf.Max(0, rect.yMin - _Rect.yMin) - Mathf.Max(0, _Rect.yMax - rect.yMax);
		
		m_Position.x -= horizontal;
		m_Position.y -= vertical;
		
		SetupMatrix();
	}

	static Vector2 GetPivot(SpriteAlignment _Alignment, Vector2 _Custom)
	{
		switch (_Alignment)
		{
			case SpriteAlignment.Center:
				return new Vector2(0.5f, 0.5f);
			case SpriteAlignment.TopLeft:
				return new Vector2(0, 1);
			case SpriteAlignment.TopCenter:
				return new Vector2(0.5f, 1);
			case SpriteAlignment.TopRight:
				return new Vector2(1, 1);
			case SpriteAlignment.LeftCenter:
				return new Vector2(0, 0.5f);
			case SpriteAlignment.RightCenter:
				return new Vector2(1, 0.5f);
			case SpriteAlignment.BottomLeft:
				return new Vector2(0, 0);
			case SpriteAlignment.BottomCenter:
				return new Vector2(0.5f, 0);
			case SpriteAlignment.BottomRight:
				return new Vector2(1, 0);
			default:
				return _Custom;
		}
	}

	static Texture2D ReformatTexture(Texture _Texture)
	{
		RenderTexture buffer = RenderTexture.GetTemporary(_Texture.width, _Texture.height, 0, RenderTextureFormat.ARGB32);
		Graphics.Blit(_Texture, buffer);
		Texture2D texture = CreateTexture(buffer);
		RenderTexture.ReleaseTemporary(buffer);
		texture.name = _Texture.name;
		return texture;
	}

	static Texture2D CreateTexture(RenderTexture _Texture)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _Texture;
		Texture2D texture = new Texture2D(_Texture.width, _Texture.height, TextureFormat.ARGB32, false);
		texture.ReadPixels(new Rect(0, 0, _Texture.width, _Texture.height), 0, 0);
		RenderTexture.active = active;
		texture.Apply(false, false);
		return texture;
	}

	static void CopyTexture(Texture _Source, Texture _Target, RectInt _Rect, Vector2Int _Position)
	{
		Graphics.CopyTexture(
			_Source,
			0,
			0,
			_Rect.x,
			_Rect.y,
			_Rect.width,
			_Rect.height,
			_Target,
			0,
			0,
			_Position.x,
			_Position.y
		);
	}

	#endregion
}
