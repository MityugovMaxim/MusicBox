#if UNITY_EDITOR
using System;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class EditorAudioManager : AudioManager
{
	protected override void Load(Action _AudioSourceChanged)
	{
		Log.Info(this, "Load completed.");
	}

	protected override void Unload()
	{
		Log.Info(this, "Unload completed.");
	}

	public override void SetAudioActive(bool _Value)
	{
		if (_Value)
			Log.Info(this, "Audio enabled");
		else
			Log.Info(this, "Audio disabled");
	}

	public override string GetAudioOutputName()
	{
		return "Speaker";
	}

	public override AudioOutputType GetAudioOutputType()
	{
		return AudioOutputType.BuiltIn;
	}

	public override string GetAudioOutputID()
	{
		return "speaker_id";
	}
}
#endif