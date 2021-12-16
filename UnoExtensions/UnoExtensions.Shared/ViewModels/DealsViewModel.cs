using System;
using UnoExtensions.Models;
using UnoExtensions.Services;
using Uno.Extensions.Reactive;

namespace UnoExtensions.ViewModels;

public record DealsViewModel(IDealService DealService)
{
	public IFeed<Product[]> Items => Feed.Async(DealService.GetDeals);
}
