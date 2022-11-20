using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class AmbientSource : MonoBehaviour
{
	[Preserve]
	public class Pool : MonoMemoryPool<AmbientSource>
	{
		protected override void OnDespawned(AmbientSource _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Restore();
		}
	}

	public AudioClip Clip
	{
		get => m_AudioSource.clip;
		set => m_AudioSource.clip = value;
	}

	public float Volume { get; set; }

	[SerializeField] AudioSource m_AudioSource;
	[SerializeField] float       m_FadeInDuration;
	[SerializeField] float       m_FadeOutDuration;

	CancellationTokenSource m_TokenSource;

	public async Task PlayAsync(Action _Finished = null)
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await Task.WhenAll(
				m_AudioSource.SetVolumeAsync(Volume, m_FadeInDuration, token),
				m_AudioSource.PlayAsync(m_AudioSource.time, token)
			);
			
			token.ThrowIfCancellationRequested();
			
			_Finished?.Invoke();
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		finally
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}
	}

	public async Task PauseAsync()
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await m_AudioSource.SetVolumeAsync(0, m_FadeOutDuration, token);
			
			m_AudioSource.Pause();
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		finally
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}
	}

	void Restore()
	{
		m_AudioSource.clip = null;
	}
}
