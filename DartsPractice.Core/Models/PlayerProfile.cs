using System.ComponentModel.DataAnnotations;

namespace DartsPractice.Core.Models;

public class PlayerProfile
{
    [Key]
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public int LifetimeMatchesPlayed { get; set; }
    public int LifetimeMatchesWon { get; set; }
    public int LifetimeTurnsThrown { get; set; }
}