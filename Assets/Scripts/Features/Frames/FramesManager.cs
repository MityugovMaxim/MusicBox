using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class FramesManager : IDataManager
{
	public FramesCollection Collection => m_FramesCollection;

	public ProfileFrames Profile => m_ProfileFrames;

	[Inject] FramesCollection m_FramesCollection;
	[Inject] ProfileFrames    m_ProfileFrames;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			Collection.Load,
			Profile.Load
		);
	}

	public string GetFrameID() => Profile.Value?.FrameID;

	public List<string> GetFrameIDs() => Profile.Value?.FrameIDs.OrderBy(Collection.GetOrder).ToList();

	public string GetImage(string _FrameID)
	{
		FrameSnapshot snapshot = Collection.GetSnapshot(_FrameID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public Task<bool> SelectAsync(string _FrameID)
	{
		string frameID = GetFrameID();
		
		if (frameID == _FrameID)
			return Task.FromResult(true);
		
		FrameSelectRequest request = new FrameSelectRequest(_FrameID);
		
		return request.SendAsync();
	}
}
