using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class LevelWizard : ScriptableWizard
{
	[MenuItem("Assets/Create level...")]
	public static void Open()
	{
		DisplayWizard<LevelWizard>("Create level", "Create");
	}

	string m_Artist      = string.Empty;
	string m_Title       = string.Empty;
	string m_AudioPath   = string.Empty;
	string m_ArtworkPath = string.Empty;
	float  m_BPM         = 90;

	protected override bool DrawWizardGUI()
	{
		bool value = base.DrawWizardGUI();
		
		m_Artist = EditorGUILayout.TextField("Artist", m_Artist);
		m_Title  = EditorGUILayout.TextField("Title", m_Title);
		m_BPM    = EditorGUILayout.FloatField("BPM", m_BPM);
		
		EditorGUILayout.BeginHorizontal();
		m_AudioPath = EditorGUILayout.TextField("Audio", m_AudioPath);
		if (GUILayout.Button("...", GUILayout.Width(40)))
			m_AudioPath = GetAudioPath();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		m_ArtworkPath = EditorGUILayout.TextField("Artwork", m_ArtworkPath);
		if (GUILayout.Button("...", GUILayout.Width(40)))
			m_ArtworkPath = GetArtworkPath();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.HelpBox("Don't forget to register LevelInfo in level registry", MessageType.Info);
		
		return value;
	}

	static string GetAudioPath()
	{
		return EditorUtility.OpenFilePanel("Select audio", Application.dataPath, "wav,mp3,mp4,aac,ogg");
	}

	static string GetArtworkPath()
	{
		return EditorUtility.OpenFilePanel("Select artwork", Application.dataPath, "png,jpg");
	}

	void OnWizardCreate()
	{
		CreateFolders();
		
		ImportAudio();
		
		ImportPreview();
		
		ImportArtwork();
		
		CreateTracks();
		
		CreateBackground();
		
		CreateColorSchemes();
		
		CreateLevelInfo();
		
		CreateLevel();
		
		ResolveDependencies();
	}

	void CreateFolders()
	{
		if (string.IsNullOrEmpty(m_Artist) && string.IsNullOrEmpty(m_Title))
			return;
		
		List<string> directories = new List<string>();
		
		string id        = LevelInfoEditor.GetID(m_Artist, m_Title);
		string root      = $"Assets/Levels/{m_Artist} - {m_Title}";
		string resources = Path.Combine(root, "Resources");
		
		directories.Add(root);
		directories.Add(resources);
		directories.Add(Path.Combine(root, "Background"));
		directories.Add(Path.Combine(root, "Data"));
		directories.Add(Path.Combine(resources, id));
		directories.Add(Path.Combine(root, "ColorSchemes"));
		directories.Add(Path.Combine(root, "Tracks"));
		directories.Add(Path.Combine(root, "Sounds"));
		
		foreach (string directory in directories)
		{
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
		}
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void ImportAudio()
	{
		if (string.IsNullOrEmpty(m_AudioPath))
			return;
		
		string root      = $"Assets/Levels/{m_Artist} - {m_Title}";
		string sounds    = Path.Combine(root, "Sounds");
		string extension = Path.GetExtension(m_AudioPath);
		
		string path = Path.Combine(sounds, $"{m_Artist} - {m_Title}{extension}");
		
		File.Copy(m_AudioPath, path);
		
		AssetDatabase.ImportAsset(path);
		
		AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
		
		if (importer == null)
			return;
		
		AudioImporterSampleSettings settings = importer.defaultSampleSettings;
		settings.compressionFormat     = AudioCompressionFormat.Vorbis;
		settings.quality               = 0.5f;
		settings.sampleRateOverride    = 48000;
		settings.sampleRateSetting     = AudioSampleRateSetting.OverrideSampleRate;
		importer.defaultSampleSettings = settings;
		
		importer.SaveAndReimport();
	}

	void ImportPreview()
	{
		if (string.IsNullOrEmpty(m_AudioPath))
			return;
		
		string id        = LevelInfoEditor.GetID(m_Artist, m_Title);
		string root      = $"Assets/Levels/{m_Artist} - {m_Title}";
		string resources = Path.Combine(root, "Resources");
		string extension = Path.GetExtension(m_AudioPath);
		
		string path = Path.Combine(resources, id, $"preview_clip{extension}");
		
		File.Copy(m_AudioPath, path);
		
		AssetDatabase.ImportAsset(path);
		
		AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
		
		if (importer == null)
			return;
		
		AudioImporterSampleSettings settings = importer.defaultSampleSettings;
		settings.compressionFormat     = AudioCompressionFormat.Vorbis;
		settings.quality               = 0.5f;
		settings.sampleRateOverride    = 48000;
		settings.sampleRateSetting     = AudioSampleRateSetting.OverrideSampleRate;
		importer.defaultSampleSettings = settings;
		
		importer.SaveAndReimport();
	}

	void ImportArtwork()
	{
		if (string.IsNullOrEmpty(m_ArtworkPath))
			return;
		
		string id        = LevelInfoEditor.GetID(m_Artist, m_Title);
		string root      = $"Assets/Levels/{m_Artist} - {m_Title}";
		string resources = Path.Combine(root, "Resources");
		string extension = Path.GetExtension(m_ArtworkPath);
		
		string path = Path.Combine(resources, id, $"preview_thumbnail{extension}");
		
		File.Copy(m_ArtworkPath, path);
		
		AssetDatabase.ImportAsset(path);
		
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		
		if (importer == null)
			return;
		
		importer.textureType         = TextureImporterType.Sprite;
		importer.spritePixelsPerUnit = 1;
		importer.spriteImportMode    = SpriteImportMode.Single;
		importer.alphaIsTransparency = false;
		importer.alphaSource         = TextureImporterAlphaSource.None;
		importer.textureCompression  = TextureImporterCompression.Uncompressed;
		
		TextureImporterSettings settings = new TextureImporterSettings();
		importer.ReadTextureSettings(settings);
		settings.spriteMeshType                     = SpriteMeshType.FullRect;
		settings.spriteGenerateFallbackPhysicsShape = false;
		importer.SetTextureSettings(settings);
		
		importer.SaveAndReimport();
	}

	void CreateBackground()
	{
		const string materialPath = "Assets/Common/Materials/Spectrum-Boids.mat";
		
		Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
		
		CustomRenderTexture texture = new CustomRenderTexture(720, 1280);
		
		texture.graphicsFormat       = GraphicsFormat.R16G16B16A16_UNorm;
		texture.depth                = 0;
		texture.anisoLevel           = 0;
		texture.antiAliasing         = 2;
		texture.doubleBuffered       = true;
		texture.useDynamicScale      = false;
		texture.initializationMode   = CustomRenderTextureUpdateMode.OnLoad;
		texture.updateMode           = CustomRenderTextureUpdateMode.Realtime;
		texture.material             = material;
		texture.initializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
		texture.initializationColor  = new Color(0, 0, 0, 1);
		
		string root       = $"Assets/Levels/{m_Artist} - {m_Title}";
		string background = Path.Combine(root, "Background");
		string path       = Path.Combine(background, "background.asset");
		
		AssetDatabase.CreateAsset(texture, path);
		
		AssetDatabase.ImportAsset(path);
	}

	void CreateColorSchemes()
	{
		string root         = $"Assets/Levels/{m_Artist} - {m_Title}";
		string colorSchemes = Path.Combine(root, "ColorSchemes");
		string path         = Path.Combine(colorSchemes, "Default.asset");
		
		ColorSchemeAsset colorScheme = CreateInstance<ColorSchemeAsset>();
		
		colorScheme.ColorScheme = new ColorScheme(
			new Color(0f, 1f, 0.65f, 0f),
			new Color(0.45f, 0.7f, 1f, 0f),
			new Color(1, 1, 1, 0.75f),
			new Color(0f, 0.55f, 1f, 0f)
		);
		
		AssetDatabase.CreateAsset(colorScheme, path);
		
		AssetDatabase.ImportAsset(path);
	}

	void CreateTracks()
	{
		string root   = $"Assets/Levels/{m_Artist} - {m_Title}";
		string tracks = Path.Combine(root, "Tracks");
		
		string musicTrackPath  = Path.Combine(tracks, "Music Track.asset");
		string tapTrack1Path   = Path.Combine(tracks, "Tap Track 1.asset");
		string tapTrack2Path   = Path.Combine(tracks, "Tap Track 2.asset");
		string tapTrack3Path   = Path.Combine(tracks, "Tap Track 3.asset");
		string tapTrack4Path   = Path.Combine(tracks, "Tap Track 4.asset");
		string holdTrack1Path  = Path.Combine(tracks, "Hold Track 1.asset");
		string holdTrack2Path  = Path.Combine(tracks, "Hold Track 2.asset");
		string doubleTrackPath = Path.Combine(tracks, "Double Track.asset");
		string colorTrackPath  = Path.Combine(tracks, "Color Track.asset");
		
		MusicTrack  musicTrack  = CreateInstance<MusicTrack>();
		TapTrack    tapTrack1   = CreateInstance<TapTrack>();
		TapTrack    tapTrack2   = CreateInstance<TapTrack>();
		TapTrack    tapTrack3   = CreateInstance<TapTrack>();
		TapTrack    tapTrack4   = CreateInstance<TapTrack>();
		HoldTrack   holdTrack1  = CreateInstance<HoldTrack>();
		HoldTrack   holdTrack2  = CreateInstance<HoldTrack>();
		DoubleTrack doubleTrack = CreateInstance<DoubleTrack>();
		ColorTrack  colorTrack  = CreateInstance<ColorTrack>();
		
		using (SerializedObject musicTrackObject = new SerializedObject(musicTrack))
		{
			SerializedProperty audioSourceProperty = musicTrackObject.FindProperty("m_AudioSource");
			audioSourceProperty.stringValue = "music";
			musicTrackObject.ApplyModifiedProperties();
		}
		
		using (SerializedObject colorTrackObject = new SerializedObject(colorTrack))
		{
			SerializedProperty colorProcessorProperty = colorTrackObject.FindProperty("m_ColorProcessor");
			colorProcessorProperty.stringValue = "color";
			colorTrackObject.ApplyModifiedProperties();
		}
		
		void SetupTrack(Track _Track, string _TrackReference)
		{
			using (SerializedObject trackObject = new SerializedObject(_Track))
			{
				SerializedProperty trackProperty = trackObject.FindProperty("m_Track");
				trackProperty.stringValue = _TrackReference;
				trackObject.ApplyModifiedProperties();
			}
		}
		
		void FoldTrack(Track _Track)
		{
			using (SerializedObject trackObject = new SerializedObject(_Track))
			{
				SerializedProperty expandedProperty = trackObject.FindProperty("m_Expanded");
				expandedProperty.boolValue = false;
				trackObject.ApplyModifiedProperties();
			}
		}
		
		SetupTrack(tapTrack1, "game_canvas/tracks/tracks_area/tap_track_1");
		SetupTrack(tapTrack2, "game_canvas/tracks/tracks_area/tap_track_2");
		SetupTrack(tapTrack3, "game_canvas/tracks/tracks_area/tap_track_3");
		SetupTrack(tapTrack4, "game_canvas/tracks/tracks_area/tap_track_4");
		SetupTrack(holdTrack1, "game_canvas/tracks/tracks_area/hold_track_1");
		SetupTrack(holdTrack2, "game_canvas/tracks/tracks_area/hold_track_2");
		SetupTrack(doubleTrack, "game_canvas/tracks/tracks_area/double_track");
		
		FoldTrack(musicTrack);
		FoldTrack(tapTrack1);
		FoldTrack(tapTrack2);
		FoldTrack(tapTrack3);
		FoldTrack(tapTrack4);
		FoldTrack(holdTrack1);
		FoldTrack(holdTrack2);
		FoldTrack(doubleTrack);
		FoldTrack(colorTrack);
		
		tapTrack1.Mnemonic   = "1";
		tapTrack2.Mnemonic   = "2";
		tapTrack3.Mnemonic   = "3";
		tapTrack4.Mnemonic   = "4";
		holdTrack1.Mnemonic  = "q";
		holdTrack2.Mnemonic  = "w";
		doubleTrack.Mnemonic = "d";
		colorTrack.Mnemonic  = "-";
		
		AssetDatabase.CreateAsset(musicTrack, musicTrackPath);
		AssetDatabase.CreateAsset(tapTrack1, tapTrack1Path);
		AssetDatabase.CreateAsset(tapTrack2, tapTrack2Path);
		AssetDatabase.CreateAsset(tapTrack3, tapTrack3Path);
		AssetDatabase.CreateAsset(tapTrack4, tapTrack4Path);
		AssetDatabase.CreateAsset(holdTrack1, holdTrack1Path);
		AssetDatabase.CreateAsset(holdTrack2, holdTrack2Path);
		AssetDatabase.CreateAsset(doubleTrack, doubleTrackPath);
		AssetDatabase.CreateAsset(colorTrack, colorTrackPath);
		
		AssetDatabase.ImportAsset(musicTrackPath);
		AssetDatabase.ImportAsset(tapTrack1Path);
		AssetDatabase.ImportAsset(tapTrack2Path);
		AssetDatabase.ImportAsset(tapTrack3Path);
		AssetDatabase.ImportAsset(tapTrack4Path);
		AssetDatabase.ImportAsset(holdTrack1Path);
		AssetDatabase.ImportAsset(holdTrack2Path);
		AssetDatabase.ImportAsset(doubleTrackPath);
		AssetDatabase.ImportAsset(colorTrackPath);
	}

	void CreateLevelInfo()
	{
		string id   = LevelInfoEditor.GetID(m_Artist, m_Title);
		string root = $"Assets/Levels/{m_Artist} - {m_Title}";
		string path = Path.Combine(root, "Data", $"{m_Artist} - {m_Title}.asset");
		
		LevelInfo levelInfo = ScriptableObject.CreateInstance<LevelInfo>();
		
		using (SerializedObject levelInfoObject = new SerializedObject(levelInfo))
		{
			SerializedProperty artistProperty        = levelInfoObject.FindProperty("m_Artist");
			SerializedProperty titleProperty         = levelInfoObject.FindProperty("m_Title");
			SerializedProperty idProperty            = levelInfoObject.FindProperty("m_ID");
			SerializedProperty leaderboardIDProperty = levelInfoObject.FindProperty("m_LeaderboardID");
			SerializedProperty achievementIDProperty = levelInfoObject.FindProperty("m_AchievementID");
			SerializedProperty thumbnailProperty     = levelInfoObject.FindProperty("m_Thumbnail");
			SerializedProperty clipProperty          = levelInfoObject.FindProperty("m_Clip");
			
			artistProperty.stringValue        = m_Artist;
			titleProperty.stringValue         = m_Title;
			idProperty.stringValue            = id;
			leaderboardIDProperty.stringValue = LevelInfoEditor.GetLeaderboardID(m_Title);
			achievementIDProperty.stringValue = LevelInfoEditor.GetAchievementID(m_Title);
			thumbnailProperty.stringValue     = $"{id}/preview_thumbnail";
			clipProperty.stringValue          = $"{id}/preview_clip";
			
			levelInfoObject.ApplyModifiedProperties();
		}
		
		AssetDatabase.CreateAsset(levelInfo, path);
		
		AssetDatabase.ImportAsset(path);
	}

	void CreateLevel()
	{
		const string prefabPath = "Assets/Common/Prefabs/level.prefab";
		
		Level prefab = AssetDatabase.LoadAssetAtPath<Level>(prefabPath);
		
		string id        = LevelInfoEditor.GetID(m_Artist, m_Title);
		string root      = $"Assets/Levels/{m_Artist} - {m_Title}";
		string resources = Path.Combine(root, "Resources");
		string path      = Path.Combine(resources, id, "level.prefab");
		
		Level instance = PrefabUtility.InstantiatePrefab(prefab) as Level;
		
		if (instance == null)
			return;
		
		PrefabUtility.SaveAsPrefabAsset(instance.gameObject, path);
		DestroyImmediate(instance.gameObject);
		
		AssetDatabase.ImportAsset(path);
	}

	void ResolveDependencies()
	{
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		string id             = LevelInfoEditor.GetID(m_Artist, m_Title);
		string root           = $"Assets/Levels/{m_Artist} - {m_Title}";
		string resources      = Path.Combine(root, "Resources");
		string backgroundPath = Path.Combine(root, "Background", "background.asset");
		string levelPath      = Path.Combine(resources, id, "level.prefab");
		string tracksPath     = Path.Combine(root, "Tracks");
		string soundsPath     = Path.Combine(root, "Sounds");
		
		Level               level      = AssetDatabase.LoadAssetAtPath<Level>(levelPath);
		RawImage            fx         = level.transform.Find("game_canvas/fx").GetComponent<RawImage>();
		Sequencer           sequencer  = level.GetComponent<Sequencer>();
		CustomRenderTexture background = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(backgroundPath);
		
		background.Initialize();
		
		fx.texture = background;
		
		Track[] tracks = Directory.GetFiles(tracksPath, "*.asset", SearchOption.AllDirectories)
			.Select(AssetDatabase.LoadAssetAtPath<Track>)
			.ToArray();
		
		AudioClip[] sounds = Directory.GetFiles(soundsPath, "*.wav", SearchOption.AllDirectories)
			.Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
			.ToArray();
		
		sequencer.Length = sounds.Sum(_AudioClip => _AudioClip.length) + 2;
		
		using (SerializedObject sequencerObject = new SerializedObject(sequencer))
		{
			sequencerObject.UpdateIfRequiredOrScript();
			
			SerializedProperty bpmProperty    = sequencerObject.FindProperty("m_BPM");
			SerializedProperty tracksProperty = sequencerObject.FindProperty("m_Tracks");
			
			bpmProperty.floatValue = m_BPM;
			
			for (int i = 0; i < tracks.Length; i++)
			{
				tracksProperty.InsertArrayElementAtIndex(i);
				
				SerializedProperty trackProperty = tracksProperty.GetArrayElementAtIndex(i);
				
				trackProperty.objectReferenceValue = tracks[i];
			}
			
			sequencerObject.ApplyModifiedProperties();
		}
	}
}
