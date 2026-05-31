using DartsPractice.Core.Interfaces;

namespace DartsPractice.Services;

public class GameStateContainer
{
    public IDartsGame? SelectedGame { get; private set; }
    public string SelectedPresetId { get; set; } = string.Empty;
    public event Action? OnStateChange;

    public void SetGame(IDartsGame game)
    {
        SelectedGame = game;
        SelectedPresetId = string.Empty;
        OnStateChange?.Invoke();
    }
    public void ClearGame()
    {
        SelectedGame = null;
        SelectedPresetId = string.Empty;
        OnStateChange?.Invoke();
    }
}