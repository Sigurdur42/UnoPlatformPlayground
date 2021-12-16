using System;
using System.Collections.Generic;
using System.Text;
using UnoExtensions.Models;
using UnoExtensions.Services;
using Uno.Extensions.Reactive;

namespace UnoExtensions.ViewModels;

public partial record CartViewModel(ICartService CartService)
{
	public IFeed<Cart> Cart => Feed.Async(CartService.Get);
}
