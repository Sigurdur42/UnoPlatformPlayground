using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnoExtensions.Models;

namespace UnoExtensions.Services;

public interface ICartService
{
	ValueTask<Cart> Get(CancellationToken ct);
}
