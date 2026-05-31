using DartsPractice;
using DartsPractice.PluginsHanders;
using DartsPractice.Services;
using IndexedDB.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IIndexedDbFactory, IndexedDbFactory>();

builder.Services.AddScoped<GameStateContainer>();
builder.Services.AddScoped<MatchManager>();
builder.Services.AddScoped<DataBackupService>();
builder.Services.AddTransient<WasmPluginLoader>();
await builder.Build().RunAsync();
