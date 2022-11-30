using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class FramesManager : IDataManager
{
	public bool Activated { get; private set; }

	public FramesCollection Collection => m_FramesCollection;

	public ProfileFrames Profile => m_ProfileFrames;

	[Inject] FramesCollection m_FramesCollection;
	[Inject] ProfileFrames    m_ProfileFrames;

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			m_FramesCollection.Load(),
			m_ProfileFrames.Load()
		);
		
		Activated = true;
		
		return frame == Time.frameCount;
	}

	public List<string> GetFrameIDs()
	{
		return Profile.GetIDs().ToList();
	}

	public string GetImage(string _FrameID)
	{
		FrameSnapshot snapshot = Collection.GetSnapshot(_FrameID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public async Task<RequestState> Select(string _FrameID)
	{
		if (IsUnavailable(_FrameID))
			return RequestState.Fail;
		
		FrameSelectRequest request = new FrameSelectRequest(_FrameID);
		
		bool success = await request.SendAsync();
		
		return success ? RequestState.Success : RequestState.Fail;
	}

	bool IsAvailable(string _FrameID) => Profile.Contains(_FrameID);

	bool IsUnavailable(string _FrameID) => !IsAvailable(_FrameID);
}