using DartsPractice.Core.Interfaces;

namespace DartsPractice.Services;

public class PlayerInstance { public string Name { get; set; } = string.Empty; public IDartsGame GameInstance { get; set; } = null!; }

public class MatchManager
{
    public List<PlayerInstance> Players { get; private set; } = new();
    public int CurrentPlayerIndex { get; private set; }
    public PlayerInstance CurrentPlayer => Players[CurrentPlayerIndex];
    public bool IsMatchOver => Players.Any(p => p.GameInstance.IsGameOver);

    public void InitializeMatch(List<string> profileNames, IDartsGame basePlugin, string presetId)
    {
        Players.Clear();
        CurrentPlayerIndex = 0;
        foreach (var name in profileNames)
        {
            var pInstance = (IDartsGame)Activator.CreateInstance(basePlugin.GetType())!;
            pInstance.SetupGame(presetId);
            pInstance.StartGame();
            Players.Add(new PlayerInstance { Name = name, GameInstance = pInstance });
        }
    }

    public void AdvanceTurn() => CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    public void RegressTurn() => CurrentPlayerIndex = (CurrentPlayerIndex - 1 + Players.Count) % Players.Count;
}