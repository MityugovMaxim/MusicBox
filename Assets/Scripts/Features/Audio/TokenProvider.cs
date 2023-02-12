using System.Collections.Generic;
using System.Threading;
using AudioBox.Logging;

public static class TokenProvider
{
	const string TAG = nameof(TokenProvider);

	static readonly Dictionary<string, CancellationTokenSource> m_Tokens = new Dictionary<string, CancellationTokenSource>();

	public static CancellationToken CreateToken<T>(object _Context, T _TokenID) => CreateToken($"[{_Context.GetType().Name}] {_TokenID}");

	public static CancellationToken CreateToken(object _Context, string _TokenID) => CreateToken($"[{_Context.GetType().Name}] {_TokenID}");

	public static CancellationToken CreateToken(string _Context, string _TokenID) => CreateToken($"[{_Context}] {_TokenID}");

	public static CancellationToken CreateToken(string _TokenID)
	{
		if (string.IsNullOrEmpty(_TokenID))
		{
			Log.Error(TAG, "Create token failed. Token ID is null or empty.");
			return default;
		}
		
		CancellationTokenSource source = new CancellationTokenSource();
		
		m_Tokens[_TokenID] = source;
		
		return source.Token;
	}

	public static CancellationToken CreateToken(object _Context, string _TokenID, CancellationToken _Token) => CreateToken($"[{_Context.GetType().Name}] {_TokenID}", _Token);

	public static CancellationToken CreateToken(string _Context, string _TokenID, CancellationToken _Token) => CreateToken($"[{_Context}] {_TokenID}", _Token);

	public static CancellationToken CreateToken(string _TokenID, CancellationToken _Token)
	{
		if (string.IsNullOrEmpty(_TokenID))
		{
			Log.Error(TAG, "Create token failed. Token ID is null or empty.");
			return _Token;
		}
		
		CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(_Token);
		
		m_Tokens[_TokenID] = source;
		
		return source.Token;
	}

	public static void CancelToken<T>(object _Context, T _TokenID) => CancelToken($"[{_Context.GetType().Name}] {_TokenID}");

	public static void CancelToken(object _Context, string _TokenID) => CancelToken($"[{_Context.GetType().Name}] {_TokenID}");

	public static void CancelToken(string _Context, string _TokenID) => CancelToken($"[{_Context}] {_TokenID}");

	public static void CancelToken(string _TokenID)
	{
		if (string.IsNullOrEmpty(_TokenID))
		{
			Log.Error(TAG, "Cancel token failed. Token ID is null or empty.");
			return;
		}
		
		if (!m_Tokens.TryGetValue(_TokenID, out CancellationTokenSource source))
			return;
		
		m_Tokens.Remove(_TokenID);
		
		if (source == null)
		{
			Log.Error(TAG, "Cancel token failed. Source for Token ID '{0}' is null.", _TokenID);
			return;
		}
		
		source.Cancel();
	}

	public static void RemoveToken<T>(object _Context, T _TokenID) => RemoveToken($"[{_Context.GetType().Name}] {_TokenID}");

	public static void RemoveToken(object _Context, string _TokenID) => RemoveToken($"[{_Context.GetType().Name}] {_TokenID}");

	public static void RemoveToken(string _Context, string _TokenID) => RemoveToken($"[{_Context}] {_TokenID}");

	public static void RemoveToken(string _TokenID)
	{
		if (string.IsNullOrEmpty(_TokenID))
		{
			Log.Error(TAG, "Remove token failed. Token ID is null or empty.");
			return;
		}
		
		if (!m_Tokens.TryGetValue(_TokenID, out CancellationTokenSource source))
			return;
		
		m_Tokens.Remove(_TokenID);
		
		if (source == null)
		{
			Log.Error(TAG, "Remove token failed. Source for Token ID '{0}' is null.", _TokenID);
			return;
		}
		
		source.Dispose();
	}
}
