using UnityEngine.Scripting;

[Preserve]
public class ProductsCollection : DataCollection<ProductSnapshot>
{
	protected override string Path => "products";
}
