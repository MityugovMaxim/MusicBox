using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class ProductsCollection : DataCollection<ProductSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "products";

	protected override Task OnLoad()
	{
		Log.Info(this, "Products loaded.");
		
		return base.OnLoad();
	}
}
