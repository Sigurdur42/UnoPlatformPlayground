﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnoExtensions.Models;
using UnoExtensions.Services;
using UnoExtensions.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.Extensions.Configuration;
using Uno.Extensions.Hosting;
using Uno.Extensions.Logging;
using Uno.Extensions.Navigation;
using Uno.Extensions.Navigation.UI;
using Uno.Extensions.Navigation.Regions;
using Uno.Extensions.Navigation.Toolkit;
using Uno.Extensions.Serialization;
using Uno.Foundation;
using UnoExtensions.Views;
using Uno.Extensions.Logging.Serilog;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
using Window = Microsoft.UI.Xaml.Window;
using CoreApplication = Windows.ApplicationModel.Core.CoreApplication;

namespace UnoExtensions
{
	public sealed partial class App : Application
	{
		private Window _window;
		public Window Window => _window;

		private IHost Host { get; }

		public App()
		{
			Host = UnoHost
					.CreateDefaultBuilder(true)
#if DEBUG
					// Switch to Development environment when running in DEBUG
					.UseEnvironment(Environments.Development)
#endif


			// Add platform specific log providers
#if !HAS_UNO || __WASM__
			.UseLogging()
#else
			.UseLogging(b => b.AddSimpleConsole(options =>
			{
				options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
			}))
#endif

					// Configure log levels for different categories of logging
					.ConfigureLogging(logBuilder =>
					{
						logBuilder
								.SetMinimumLevel(LogLevel.Information)
								.XamlLogLevel(LogLevel.Information)
								.XamlLayoutLogLevel(LogLevel.Information);
					})



					// Load configuration information from appsettings.json
					.UseAppSettings()

					// Load AppInfo section
					.UseConfiguration<AppInfo>()

					// Register entities for saving settings
					.UseSettings<Credentials>()


					// Register Json serializers (ISerializer and IStreamSerializer)
					.UseSerialization()

					// Register services for the application
					.ConfigureServices(services =>
					{
						services

							.AddSingleton<IProductService, ProductService>()
							.AddSingleton<ICartService, CartService>()
							.AddSingleton<IDealService, DealService>()
							.AddSingleton<IProfileService, ProfileService>();
					})


					// Enable navigation, including registering views and viewmodels
					.UseNavigation(RegisterRoutes)

					// Add navigation support for toolkit controls such as TabBar and NavigationView
					.UseToolkitNavigation()


					.Build(enableUnoLogging: true);

			this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
			this.Suspending += OnSuspending;
#endif
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected async override void OnLaunched(LaunchActivatedEventArgs args)
		{
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

#if NET5_0 && WINDOWS
            _window = new Window();
            _window.Activate();
#else
			_window = Window.Current;
#endif

			var notif = Host.Services.GetService<IRouteNotifier>();
			notif.RouteChanged += RouteUpdated;


			_window.Content = new ShellView().WithNavigation(Host.Services);
			_window.Activate();

			await Task.Run(async () =>
			{
				await Host.StartAsync();
			});

		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			// TODO: Save application state and stop any background activity
			deferral.Complete();
		}

		private static void RegisterRoutes(IRouteBuilder builder)
		{
			builder
				.Register(RouteMap.For(nameof(ShellView)))
				.Register(ViewMap.For(nameof(ShellView)).Show<ShellView>().With<ShellViewModel>())

				.Register(ViewMap.For("Login").Show<LoginPage>().With<LoginViewModel.BindableLoginViewModel>())

				.Register(RouteMap.For("Home")
					.Process((region, nav) => nav.Route.Next().IsEmpty() ?
												nav with { Route = nav.Route.Append(Route.NestedRoute("Products")) } :
												nav))
				.Register(ViewMap.For("Home").Show<HomePage>().With<HomeViewModel>())

				.Register(RouteMap.For("Products")
					.Process((region, nav) => nav.Route.Next().IsEmpty() ?
												nav with { Route = nav.Route.AppendPage<ProductsPage>() } : nav with
												{
													Route = nav.Route.ContainsView<ProductsPage>() ?
																	nav.Route :
																	nav.Route.InsertPage<ProductsPage>()
												}))
				.Register(ViewMap.For("Products").Show<FrameView>())

				.Register(ViewMap.For(nameof(ProductsPage)).Show<ProductsPage>().With<ProductsViewModel.BindableProductsViewModel>())

				.Register(RouteMap.For("Deals")
					.Process((region, nav) => nav.Route.Next().IsEmpty() ?
												nav with { Route = nav.Route.AppendPage<DealsPage>() } : nav with
												{
													Route = nav.Route.ContainsView<DealsPage>() ?
																	nav.Route :
																	nav.Route.InsertPage<DealsPage>()
												}))
				.Register(ViewMap.For("Deals").Show<FrameView>())

				.Register(ViewMap.For("Deals").Show<DealsPage>().With<DealsViewModel>())

				.Register(RouteMap<Product>.For("ProductDetails")
					.Process((region, nav) => (App.Current as App).Window.Content.ActualSize.X > 800 ?
												nav with { Route = nav.Route with { Scheme = "./", Base = "Details", Path = nameof(ProductDetailsPage) } } :
												nav with { Route = nav.Route with { Base = nameof(ProductDetailsPage) } }))

				.Register(RouteMap<Product>.For(nameof(ProductDetailsPage))
					.ConvertDataToQuery(product => new Dictionary<string, string> { { nameof(Product.ProductId), product.ProductId + "" } }))

				.Register(ViewMap.For(nameof(ProductDetailsPage)).Show<ProductDetailsPage>().With<ProductDetailsViewModel.BindableProductDetailsViewModel>())

				.Register(RouteMap.For(nameof(CartFlyout))
					.Process((region, nav) => nav.Route.Next().IsEmpty() ?
													nav with { Route = nav.Route.AppendNested<CartPage>() } :
													nav))
				.Register(ViewMap.For(nameof(CartFlyout)).Show<CartFlyout>())

				.Register(RouteMap<Filters>.For(nameof(Filters)))
				.Register(ViewMap.For("Filter").Show<FilterFlyout>().With<FiltersViewModel.BindableFiltersViewModel>())

				.Register(ViewMap.For("Profile").Show<ProfilePage>().With<ProfileViewModel>())

				.Register(ViewMap.For(nameof(CartPage)).Show<CartPage>().With<CartViewModel>());
		}

		public async void RouteUpdated(object sender, EventArgs e)
		{
			try
			{
				var reg = Host.Services.GetService<IRegion>();
				var rootRegion = reg.Root();
				var route = rootRegion.GetRoute();


#if !__WASM__ && !WINUI
				CoreApplication.MainView?.DispatcherQueue.TryEnqueue(() =>
				{
					var appTitle = ApplicationView.GetForCurrentView();
					appTitle.Title = "UnoExtensions: " + (route + "").Replace("+", "/");
				});
#endif


#if __WASM__
				// Note: This is a hack to avoid error being thrown when loading products async
				await Task.Delay(1000).ConfigureAwait(false);
				CoreApplication.MainView?.DispatcherQueue.TryEnqueue(() =>
				{
					var href = WebAssemblyRuntime.InvokeJS("window.location.href");
					var url = new UriBuilder(href);
					url.Query = route.Query();
					url.Path = route.FullPath()?.Replace("+", "/");
					var webUri = url.Uri.OriginalString;
					var js = $"window.history.pushState(\"{webUri}\",\"\", \"{webUri}\");";
					Console.WriteLine($"JS:{js}");
					var result = WebAssemblyRuntime.InvokeJS(js);
				});
#endif
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}
	}
}
