﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnoExtensions.Models;
using Uno.Extensions.Serialization;

namespace UnoExtensions.Services;

class CartService : ICartService
{
    private IJsonDataService<Product> _productDataService;
    public CartService(IJsonDataService<Product> products)
    {
        _productDataService = products;
        _productDataService.DataFile = ProductService.ProductDataFile;
    }

    public async ValueTask<Cart> Get(CancellationToken ct)
    {
        var entities = await _productDataService.GetEntities();
        var cart = new Cart(entities.Select(e => new CartItem(e, 1)).ToArray());
        return cart;
    }

}
