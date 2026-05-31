using DartsPractice.Core.Models;

namespace DartsPractice.Core.Interfaces;

public interface IDartsGame
{
    string Name { get; }
    string Description { get; }
    bool IsGameOver { get; }
    GameInputType InputType { get; }
    List<GamePreset> AvailablePresets { get; }
    DartTarget CurrentTarget { get; } 
    int DartsRemainingInTurn { get; }    
    List<TurnResult> TurnHistory { get; }

    void SetupGame(string presetId);
    void StartGame();
    bool RecordDart(DartTarget hitTarget); 
    void EndTurn();
    bool UndoLastDart(); 
    string GetCurrentStatus();
}