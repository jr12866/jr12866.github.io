namespace DartsPractice.Core.Models;

public class BackupModel
{
    public string ExportDate { get; set; } = string.Empty;
    public List<PlayerProfile> Profiles { get; set; } = new();
    public List<MatchRecord> History { get; set; } = new();
}