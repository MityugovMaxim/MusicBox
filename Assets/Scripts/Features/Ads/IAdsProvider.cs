using System.Threading.Tasks;

public interface IAdsProvider
{
	string     ID { get; }
	Task<bool> Initialize(string   _InterstitialID, string _RewardedID);
	Task<bool> Interstitial(string _Place);
	Task<bool> Rewarded(string     _Place);
}