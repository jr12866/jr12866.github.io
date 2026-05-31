using DartsPractice.Core.Models;
using IndexedDB.Blazor;
using Microsoft.JSInterop;

namespace DartsPractice.Data;

public class DartsDb : IndexedDb
{
    public DartsDb(IJSRuntime jSRuntime, string name, int version): base(jSRuntime, name, version) { }

    public IndexedSet<PlayerProfile>? PlayerProfiles { get; set; }
    public IndexedSet<MatchRecord>? MatchRecords { get; set; }
}
