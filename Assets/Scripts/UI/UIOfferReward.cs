public abstract class UIOfferReward : UIOfferEntity
{
	public enum RewardType
	{
		Coins   = 0,
		Song    = 1,
		Chest   = 2,
		Voucher = 3,
	}

	protected abstract RewardType Type { get; }

	protected override void Subscribe()
	{
		OffersManager.Collection.Subscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		OffersManager.Collection.Unsubscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void ProcessData()
	{
		switch (Type)
		{
			case RewardType.Coins:
				ProcessCoins(OffersManager.GetCoins(OfferID));
				break;
			case RewardType.Song:
				ProcessSong(OffersManager.GetSongID(OfferID));
				break;
			case RewardType.Chest:
				ProcessChest(OffersManager.GetChestID(OfferID));
				break;
			case RewardType.Voucher:
				ProcessVoucher(OffersManager.GetVoucherID(OfferID));
				break;
		}
	}

	protected void SetActive(bool _Value) => gameObject.SetActive(_Value);

	protected virtual void ProcessCoins(long _Coins) { }

	protected virtual void ProcessSong(string _SongID) { }

	protected virtual void ProcessChest(string _ChestID) { }

	protected virtual void ProcessVoucher(string _VoucherID) { }
}
