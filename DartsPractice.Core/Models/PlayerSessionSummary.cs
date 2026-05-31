namespace DartsPractice.Core.Models;

public class PlayerSessionSummary
{
    public string PlayerName { get; set; } = string.Empty;
    public string FinalStatus { get; set; } = string.Empty; 
    public int TotalTurnsPlayed { get; set; }
}