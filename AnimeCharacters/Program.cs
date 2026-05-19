using Blazored.LocalStorage;
using AnimeCharacters.Extensions;
using AnimeCharacters.Extensions.GenshinImpact;
using MatBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ReferenceApis;
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
            builder.Services.AddScoped<IGenshinImpactCharacterRepository, GenshinImpactCharacterRepository>();
            builder.Services.AddScoped<IVoiceActorCreditProvider, KitsuLibraryCreditProvider>();
            builder.Services.AddScoped<IVoiceActorCreditProvider, GenshinImpactCreditProvider>();
            builder.Services.AddScoped<IVoiceActorCreditService, VoiceActorCreditService>();
            builder.Services.AddScoped<IReferenceAnimeProvider, JikanReferenceAnimeProvider>();
            builder.Services.AddScoped<IReferenceAnimeProvider, AniListReferenceAnimeProvider>();
            builder.Services.AddScoped<IReferenceAnimeService, ReferenceAnimeService>();

            await builder.Build().RunAsync();
        }
    }
}
