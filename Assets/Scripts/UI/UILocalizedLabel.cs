using System;
using System.Linq;
using AudioBox.Logging;
using TMPro;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(TMP_Text))]
public class UILocalizedLabel : UIEntity
{
	[Flags]
	public enum Options
	{
		None    = 0,
		Format  = 1 << 0,
		Postfix = 1 << 1,
		Prefix  = 1 << 2,
		Trim    = 1 << 3,
		Upper   = 1 << 4,
		Lower   = 1 << 5,
	}

	public string Key
	{
		get => m_Key;
		set
		{
			if (m_Key == value)
				return;
			
			m_Key = value;
			
			ProcessText();
		}
	}

	[SerializeField, HideInInspector] string   m_Key;
	[SerializeField, HideInInspector] Options  m_Options;
	[SerializeField, HideInInspector] string[] m_Data;
	[SerializeField, HideInInspector] string   m_Prefix;
	[SerializeField, HideInInspector] string   m_Postfix;

	TMP_Text m_Label;

	[Inject] SignalBus             m_SignalBus;
	[Inject] LocalizationProcessor m_LocalizationProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Label = GetComponent<TMP_Text>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessText();
		
		if (m_SignalBus != null)
			m_SignalBus.Subscribe<LanguageSelectSignal>(ProcessText);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		if (m_SignalBus != null)
			m_SignalBus.Unsubscribe<LanguageSelectSignal>(ProcessText);
	}

	void ProcessText()
	{
		if (string.IsNullOrEmpty(Key))
		{
			m_Label.text = string.Empty;
			return;
		}
		
		string text = m_LocalizationProcessor.Get(Key);
		
		text = Format(text);
		text = Prefix(text);
		text = Postfix(text);
		text = Trim(text);
		text = Upper(text);
		text = Lower(text);
		
		m_Label.text = text; 
	}

	string Format(string _Text)
	{
		if (!CheckOptions(Options.Format) || m_Data == null)
			return _Text;
		
		try
		{
			switch (m_Data.Length)
			{
				case 0:
					return _Text;
				case 1:
					return string.Format(_Text, m_Data[0]);
				case 2:
					return string.Format(_Text, m_Data[0], m_Data[1]);
				case 3:
					return string.Format(_Text, m_Data[0], m_Data[1], m_Data[2]);
				default:
					return string.Format(_Text, m_Data.OfType<object>().ToArray());
			}
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		return _Text;
	}

	string Upper(string _Text)
	{
		return CheckOptions(Options.Upper) ? _Text.ToUpperInvariant() : _Text;
	}

	string Lower(string _Text)
	{
		return CheckOptions(Options.Lower) ? _Text.ToLowerInvariant() : _Text;
	}

	string Prefix(string _Text)
	{
		return CheckOptions(Options.Prefix) && !string.IsNullOrEmpty(m_Prefix)
			? m_Prefix + _Text
			: _Text;
	}

	string Postfix(string _Text)
	{
		return CheckOptions(Options.Postfix) && !string.IsNullOrEmpty(m_Postfix)
			? _Text + m_Postfix
			: _Text;
	}

	string Trim(string _Text)
	{
		return CheckOptions(Options.Trim) ? _Text.Trim() : _Text;
	}

	bool CheckOptions(Options _Options)
	{
		return CheckOptions(m_Options, _Options);
	}

	public static bool CheckOptions(Options _Options, Options _Check)
	{
		return (_Options & _Check) == _Check;
	}
}
