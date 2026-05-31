using System.ComponentModel.DataAnnotations;

namespace DartsPractice.Core.Models;

public class MatchRecord
{
    [Key]
    public int Id { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public DateTime DatePlayed { get; set; } = DateTime.Now;
    public string WinnerName { get; set; } = string.Empty;
    public string PlayerStatsJson { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string DetailedTurnsJson { get; set; } = string.Empty;
    public bool QuitEarly { get; set; } = false;
}