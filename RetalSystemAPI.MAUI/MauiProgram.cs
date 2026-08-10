using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using RetalSystemAPI.MAUI.Core.Auth;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.MAUI.ViewModels;
using RetalSystemAPI.MAUI.ViewModels.Auth;
using RetalSystemAPI.MAUI.ViewModels.Branches;
using RetalSystemAPI.MAUI.ViewModels.Catalog;
using RetalSystemAPI.MAUI.ViewModels.Tenants;
using RetalSystemAPI.MAUI.Views;
using RetalSystemAPI.MAUI.Views.Auth;
using RetalSystemAPI.MAUI.Views.Branches;
using RetalSystemAPI.MAUI.Views.Catalog;
using RetalSystemAPI.MAUI.Views.Tenants;

namespace RetalSystemAPI.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "Cairo");
                    fonts.AddFont("OpenSans-Semibold.ttf", "CairoBold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Auth State & HTTP Handlers
            builder.Services.AddSingleton<IAuthStateService, AuthStateService>();
            builder.Services.AddTransient<AuthHeaderHandler>();

            // Configure Base HttpClient
            var apiBaseUrl = "https://localhost:7226/"; // API Project Port
            builder.Services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });


            // API Services
            builder.Services.AddSingleton<IAuthApiService, AuthApiService>();
            builder.Services.AddSingleton<ITenantApiService, TenantApiService>();
            builder.Services.AddSingleton<IBranchApiService, BranchApiService>();
            builder.Services.AddSingleton<ICategoryApiService, CategoryApiService>();
            builder.Services.AddSingleton<IUnitApiService, UnitApiService>();
            builder.Services.AddSingleton<IProductApiService, ProductApiService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<TenantsViewModel>();
            builder.Services.AddTransient<BranchesViewModel>();
            builder.Services.AddTransient<CategoriesViewModel>();
            builder.Services.AddTransient<UnitsViewModel>();
            builder.Services.AddTransient<ProductsViewModel>();
            builder.Services.AddTransient<ProductDetailViewModel>();

            // Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<TenantsPage>();
            builder.Services.AddTransient<BranchesPage>();
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<UnitsPage>();
            builder.Services.AddTransient<ProductsPage>();
            builder.Services.AddTransient<ProductDetailPage>();

            return builder.Build();
        }
    }
}
