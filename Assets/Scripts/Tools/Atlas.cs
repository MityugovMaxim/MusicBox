using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Atlas
{
	class Anchor
	{
		public Texture2D Atlas { get; }
		public RectInt   Rect  { get; }

		public Anchor(Texture2D _Atlas, RectInt _Rect)
		{
			Atlas = _Atlas;
			Rect  = _Rect;
		}
	}

	class Entry
	{
		public Anchor Anchor     { get; }
		public Sprite Sprite     { get; }
		public int    References { get; set; }

		public Entry(Anchor _Anchor, Sprite _Sprite)
		{
			Anchor     = _Anchor;
			Sprite     = _Sprite;
			References = 0;
		}
	}

	public static Atlas Create(string _AtlasID, int _AtlasSize, int _Width, int _Height)
	{
		string atlasID = $"{_AtlasID}_{_AtlasSize}_[{_Width}x{_Height}]";
		
		if (m_Atlases.ContainsKey(atlasID) && m_Atlases[atlasID] != null)
			return m_Atlases[atlasID];
		
		m_Atlases[atlasID] = new Atlas(_AtlasSize, new Vector2Int(_Width, _Height));
		
		return m_Atlases[atlasID];
	}

	static readonly Dictionary<string, Atlas> m_Atlases = new Dictionary<string, Atlas>();

	readonly int        m_AtlasSize;
	readonly Vector2Int m_SpriteSize;

	readonly Dictionary<string, Entry> m_Entries = new Dictionary<string, Entry>();

	readonly Stack<Anchor> m_Anchors = new Stack<Anchor>();

	Atlas(int _AtlasSize, Vector2Int _SpriteSize)
	{
		m_AtlasSize  = _AtlasSize;
		m_SpriteSize = _SpriteSize;
		
		GenerateAnchors();
	}

	public bool TryAlloc(string _ID, out Sprite _Sprite)
	{
		_Sprite = Alloc(_ID);
		
		return _Sprite != null;
	}

	public Sprite Alloc(string _ID)
	{
		if (string.IsNullOrEmpty(_ID))
			return null;
		
		if (!m_Entries.TryGetValue(_ID, out Entry entry) || entry == null)
			return null;
		
		entry.References++;
		
		return entry.Sprite;
	}

	public void Release(string _ID)
	{
		if (!Contains(_ID))
			return;
		
		Entry entry = m_Entries[_ID];
		
		if (entry == null)
			m_Entries.Remove(_ID);
		else
			entry.References--;
	}

	public Sprite Bake(string _ID, Texture2D _Texture)
	{
		if (string.IsNullOrEmpty(_ID) || _Texture == null)
			return null;
		
		if (m_Entries.ContainsKey(_ID) && m_Entries[_ID] != null)
		{
			Entry entry = m_Entries[_ID];
			entry.References++;
			return entry.Sprite;
		}
		
		if (m_Anchors.Count == 0)
			GenerateAnchors();
		
		Anchor anchor = m_Anchors.Pop();
		
		_Texture = _Texture.SetSize(m_SpriteSize.x, m_SpriteSize.y);
		
		Graphics.CopyTexture(
			_Texture,
			0,
			0,
			0,
			0,
			Mathf.Min(_Texture.width, anchor.Rect.width),
			Mathf.Min(_Texture.height, anchor.Rect.height),
			anchor.Atlas,
			0,
			0,
			anchor.Rect.x,
			anchor.Rect.y
		);
		
		Rect uv = new Rect(
			anchor.Rect.x,
			anchor.Rect.y,
			Mathf.Min(_Texture.width, anchor.Rect.width),
			Mathf.Min(_Texture.height, anchor.Rect.height)
		);
		
		uv = new RectOffset(1, 1, 1, 1).Remove(uv);
		
		Sprite sprite = Sprite.Create(
			anchor.Atlas,
			uv,
			uv.center,
			1,
			0,
			SpriteMeshType.FullRect,
			Vector4.zero
		);
		
		sprite.name = _ID;
		
		m_Entries[sprite.name] = new Entry(anchor, sprite);
		m_Entries[sprite.name].References++;
		
		return sprite;
	}

	void GenerateAnchors()
	{
		if (TryFreeAnchors())
			return;
		
		Texture2D atlas = new Texture2D(m_AtlasSize, m_AtlasSize, TextureFormat.RGB24, false);
		
		int colCount = atlas.width / m_SpriteSize.x;
		int rowCount = atlas.height / m_SpriteSize.y;
		
		for (int x = 0; x < colCount; x++)
		for (int y = 0; y < rowCount; y++)
		{
			RectInt rect = new RectInt(
				m_SpriteSize.x * x,
				m_SpriteSize.y * y,
				m_SpriteSize.x,
				m_SpriteSize.y
			);
			
			Anchor anchor = new Anchor(atlas, rect);
			
			m_Anchors.Push(anchor);
		}
	}

	bool TryFreeAnchors()
	{
		if (m_Entries.Count == 0)
			return false;
		
		string[] ids = m_Entries.Keys.ToArray();
		
		foreach (string id in ids)
		{
			Entry entry = m_Entries[id];
			
			if (entry == null)
			{
				m_Entries.Remove(id);
				continue;
			}
			
			if (entry.References > 0)
				continue;
			
			m_Entries.Remove(id);
			m_Anchors.Push(entry.Anchor);
			
			return true;
		}
		
		return false;
	}

	bool Contains(string _ID) => m_Entries.ContainsKey(_ID);
}