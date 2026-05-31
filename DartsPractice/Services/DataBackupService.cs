using DartsPractice.Core.Models;
using DartsPractice.Data;
using IndexedDB.Blazor;
using System.Text.Json;

namespace DartsPractice.Services;

public class DataBackupService
{
    private IIndexedDbFactory _indexedDbFactory;
    public DataBackupService(IIndexedDbFactory dbFactory) =>_indexedDbFactory = dbFactory;

    public async Task<string> ExportDataAsync()
    {
        using var db = await _indexedDbFactory.Create<DartsDb>();

        var p = db.PlayerProfiles.ToList();
        var h = db.MatchRecords.ToList();
        return JsonSerializer.Serialize(new BackupModel {
            ExportDate = DateTime.Now.ToString("g"),
            Profiles = p?.ToList() ?? new(),
            History = h?.ToList() ?? new()
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<bool> ImportDataAsync(string json, bool overwrite)
    {
        try
        {
            using var db = await _indexedDbFactory.Create<DartsDb>();
            var data = JsonSerializer.Deserialize<BackupModel>(json);
            if (data == null) return false;

            if (overwrite)
            {
                foreach (var p in ((IEnumerable<PlayerProfile>)db.PlayerProfiles ?? Array.Empty<PlayerProfile>())) { db.PlayerProfiles!.Remove(p); await db.SaveChanges(); }
                foreach (var h in ((IEnumerable<MatchRecord>)db.MatchRecords ?? Array.Empty<MatchRecord>())){ db.MatchRecords!.Remove(h); await db.SaveChanges(); }
            }

            foreach (var prof in data.Profiles) { if (!overwrite) prof.Id = 0; db.PlayerProfiles!.Add(prof!); await db.SaveChanges(); }
            foreach (var rec in data.History) { if (!overwrite) rec.Id = 0; db.MatchRecords!.Add(rec); await db.SaveChanges(); }
            return true;
        }
        catch { return false; }
    }
}