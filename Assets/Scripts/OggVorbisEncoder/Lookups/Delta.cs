namespace OggVorbisEncoder.Lookups
{
	public struct Delta
	{
		public readonly float Min;
		public readonly float Max;

		public Delta(
			float _Min,
			float _Max
		)
		{
			Min = _Min;
			Max = _Max;
		}
	}
}