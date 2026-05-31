using DartsPractice.Core.Interfaces;
using System.Net.Http.Json;
using System.Runtime.Loader;

namespace DartsPractice.PluginsHanders;

public class PluginManifestEntry { public string AssemblyFile { get; set; } = string.Empty; public bool Enabled { get; set; } }
public class AppManifest { public List<PluginManifestEntry> PluginRegistry { get; set; } = new(); }

public class WasmPluginLoader
{
    private readonly HttpClient _http;
    public WasmPluginLoader(HttpClient http) => _http = http;

    public async Task<List<IDartsGame>> LoadPluginsAsync()
    {
        var games = new List<IDartsGame>();
        try
        {
            var manifest = await _http.GetFromJsonAsync<AppManifest>("app-manifest.json");
            if (manifest == null) return games;

            foreach (var entry in manifest.PluginRegistry.Where(e => e.Enabled))
            {
                var bytes = await _http.GetByteArrayAsync($"plugins/{entry.AssemblyFile}");
                var asm = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(bytes));
                foreach (var type in asm.GetTypes())
                {
                    if (typeof(IDartsGame).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        if (Activator.CreateInstance(type) is IDartsGame game) games.Add(game);
                    }
                }
            }
        }
        catch (Exception ex) { Console.WriteLine($"Plugin Load Failure: {ex.Message}"); }
        return games;
    }
}