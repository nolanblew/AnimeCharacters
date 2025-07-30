using Blazored.LocalStorage;
using MatBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AnimeCharacters.Data;
using AnimeCharacters.Data.Services;
using Kitsu;
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

            // Register Entity Framework with SQLite for WASM
            builder.Services.AddDbContextFactory<AnimeCharactersDbContext>(options =>
                options.UseSqlite("Data Source=animecharacters.sqlite3"));

            // Register database services
            builder.Services.AddScoped<SqliteDatabaseProvider>();
            builder.Services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
            
            // Register sync and cache services
            builder.Services.AddScoped<IPrioritySyncService, PrioritySyncService>();
            builder.Services.AddScoped<ISyncService, SyncService>();
            builder.Services.AddScoped<ICharacterCacheService, CharacterCacheService>();
            
            // Register Kitsu client
            builder.Services.AddScoped(_ => new KitsuClient());
            
            // Register the LocalStorage-based provider for migration purposes
            builder.Services.AddScoped<DatabaseProvider>();
            
            // Use a factory to decide which provider to use
            builder.Services.AddScoped<IDatabaseProvider>(serviceProvider =>
            {
                // For now, return the SQLite provider
                // In the future, this could be configurable or based on feature flags
                return serviceProvider.GetRequiredService<SqliteDatabaseProvider>();
            });

            builder.Services.AddSingleton<IPageStateManager, PageStateManager>();
            builder.Services.AddScoped(_ => new AniListClient.AniListClient());

            var app = builder.Build();

            // Initialize the database
            using (var scope = app.Services.CreateScope())
            {
                var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
                await dbInit.InitializeDatabaseAsync();
                await dbInit.MigrateFromLocalStorageAsync();
            }

            await app.RunAsync();
        }
    }
}
