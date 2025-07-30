using AnimeCharacters.Services;
using AnimeCharacters.Services.Providers;
using Blazored.LocalStorage;
using MatBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnimeCharacters
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddMatBlazor();
            builder.Services.AddEventAggregator();

            builder.Services.AddSingleton<IPageStateManager, PageStateManager>();

            builder.Services.AddScoped<IDatabaseProvider, DatabaseProvider>();
            builder.Services.AddScoped(_ => new AniListClient.AniListClient());

            // Register data provider service and providers
            builder.Services.AddSingleton<IDataProviderService, DataProviderService>();
            builder.Services.AddScoped<ILegacyDataService, LegacyDataService>();
            builder.Services.AddScoped<IProviderManagementService, ProviderManagementService>();
            
            // Register providers
            builder.Services.AddTransient<KitsuLibraryProvider>();
            builder.Services.AddTransient<AniListCharacterProvider>();

            // Configure providers
            var app = builder.Build();
            
            // Register providers with the service
            var providerService = app.Services.GetRequiredService<IDataProviderService>();
            providerService.RegisterProvider(app.Services.GetRequiredService<KitsuLibraryProvider>());
            providerService.RegisterProvider(app.Services.GetRequiredService<AniListCharacterProvider>());

            await app.RunAsync();
        }
    }
}
