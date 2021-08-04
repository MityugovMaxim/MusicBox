using UnityEngine;

[CreateAssetMenu(fileName = "Product Registry", menuName = "Registry/Product Registry")]
public class ProductRegistry : Registry<ProductInfo>
{
	public override string Name => "Products";
}