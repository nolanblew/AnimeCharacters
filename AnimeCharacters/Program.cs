using Blazored.LocalStorage;
using MatBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using AnimeCharacters.Shared;

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
            
            builder.Services.AddScoped<IUpdateAvailableDetector, UpdateAvailableDetector>();

            await builder.Build().RunAsync();
        }
    }
}
