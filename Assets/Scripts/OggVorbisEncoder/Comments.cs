using System.Collections.Generic;
using System.Text;

namespace OggVorbisEncoder
{
	public class Comments
	{
		public List<string> UserComments { get; } = new();

		public void AddTag(string _Tag, string _Contents)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(_Tag);
			stringBuilder.Append('=');
			stringBuilder.Append(_Contents);
			UserComments.Add(stringBuilder.ToString());
		}
	}
}