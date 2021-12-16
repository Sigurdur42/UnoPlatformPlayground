using System.Threading;
using System.Threading.Tasks;
using UnoExtensions.Models;
using UnoExtensions.Services;
using Uno.Extensions.Reactive;

namespace UnoExtensions.ViewModels;

[ReactiveBindable(true)]
public partial class ProductDetailsViewModel
{
	private readonly IProductService _productService;
	private readonly Product _product;

	public ProductDetailsViewModel(
		IProductService productService,
		Product product)
	{
		_productService = productService;
		_product = product;
	}

	public IFeed<Product> Product => Feed.Async(Load);

	public IFeed<Review[]> Reviews => Product.SelectAsync(async (p, ct) => await _productService.GetReviews(p.ProductId, ct));

	private async ValueTask<Product> Load(CancellationToken ct)
		=> _product;
}
