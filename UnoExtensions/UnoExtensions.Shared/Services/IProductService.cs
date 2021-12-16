using System.Threading;
using System.Threading.Tasks;
using UnoExtensions.Models;

namespace UnoExtensions.Services;

public interface IProductService
{
	ValueTask<Product[]> GetProducts(string? term, CancellationToken ct);

	ValueTask<Review[]> GetReviews(int productId, CancellationToken ct);
}
