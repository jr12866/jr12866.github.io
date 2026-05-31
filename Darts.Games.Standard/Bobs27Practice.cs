using DartsPractice.Core.Interfaces;
using DartsPractice.Core.Models;

namespace DartsPractice.Games.Standard;

public class Bobs27Practice : IDartsGame
{
    public string Name => "Bob's 27 Double Practice";
    public string Description => "Start with 27 pts. Hit D1-D20 then D-Bull. Miss all 3 darts and lose the double value. Drop to 0 and lose.";
    public GameInputType InputType => GameInputType.HitCount;
    public bool IsGameOver { get; private set; }

    public DartTarget CurrentTarget => _targetIndex < _targets.Length
        ? new DartTarget { Value = _targets[_targetIndex], Segment = SegmentType.Double }
        : new DartTarget { Value = 0, Segment = SegmentType.Single };

    public int DartsRemainingInTurn => 3 - _dartsThrownThisTurn;
    public List<TurnResult> TurnHistory { get; private set; } = new();

    // Bob's 27 is a fixed-track layout game, so presets are not strictly required
    public List<GamePreset> AvailablePresets => new()
    {
        new GamePreset { Id = "standard", Name = "Standard Track", Description = "Progress sequentially from D1 to D20, ending on Double Bull." }
    };

    private readonly int[] _targets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 25 };
    private int _targetIndex;
    private int _score;
    private int _dartsThrownThisTurn;
    private int _hitsThisTurn;
    private List<DartThrowInfo> _currentTurnDarts = new();
    private readonly Stack<BobsSnapshot> _undoStack = new();

    private class BobsSnapshot
    {
        public int TargetIndex { get; set; }
        public int Score { get; set; }
        public int DartsThrownThisTurn { get; set; }
        public int HitsThisTurn { get; set; }
        public bool IsGameOver { get; set; }
        public List<TurnResult> TurnHistoryCopy { get; set; } = new();
        public List<DartThrowInfo> CurrentTurnDartsCopy { get; set; } = new();
    }

    public void SetupGame(string presetId) { }

    public void StartGame()
    {
        _score = 27; // Core starting point rule
        _targetIndex = 0;
        _dartsThrownThisTurn = 0;
        _hitsThisTurn = 0;
        IsGameOver = false;
        TurnHistory.Clear();
        _currentTurnDarts.Clear();
        _undoStack.Clear();
    }

    public bool RecordDart(DartTarget hitTarget)
    {
        // 1. Capture snapshot before changing state properties for perfect Undo support
        _undoStack.Push(new BobsSnapshot
        {
            TargetIndex = _targetIndex,
            Score = _score,
            DartsThrownThisTurn = _dartsThrownThisTurn,
            HitsThisTurn = _hitsThisTurn,
            IsGameOver = IsGameOver,
            TurnHistoryCopy = new List<TurnResult>(TurnHistory),
            CurrentTurnDartsCopy = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _dartsThrownThisTurn++;
        int multiplier = hitTarget.Segment == SegmentType.Double ? 2 : 1;

        _currentTurnDarts.Add(new DartThrowInfo
        {
            DartNumber = _dartsThrownThisTurn,
            Value = hitTarget.Value,
            Segment = hitTarget.Segment,
            DisplayName = hitTarget.DisplayName,
            Points = hitTarget.Value * multiplier
        });

        bool isTargetDoubleHit = (hitTarget.Value == CurrentTarget.Value && hitTarget.Segment == SegmentType.Double);

        if (isTargetDoubleHit)
        {
            _hitsThisTurn++;
            // Each hit adds the flat mathematical value of the double (Value * 2)
            _score += (CurrentTarget.Value * 2);
        }

        // Standard turn boundaries require all 3 darts to be thrown before advancing or penalizing
        return _dartsThrownThisTurn >= 3;
    }

    public void EndTurn()
    {
        int currentDoubleValue = CurrentTarget.Value * 2;

        // RULE: If they missed all 3 darts this turn, subtract the value of the double once
        if (_hitsThisTurn == 0)
        {
            _score -= currentDoubleValue;
        }

        // Log turn stats
        string logDisplay = _hitsThisTurn > 0 ? $"+{_hitsThisTurn * currentDoubleValue} pts" : $"-{currentDoubleValue} pts";
        TurnHistory.Add(new TurnResult
        {
            TurnNumber = TurnHistory.Count + 1,
            DisplayValue = $"D{CurrentTarget.Value}: {logDisplay}",
            RawValue = _score,
            DartsThrown = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _currentTurnDarts.Clear();
        // Reset turn parameters
        if (_score <= 0)
        {
            // Player loses immediately if score drops to zero or goes negative
            IsGameOver = true;
        }
        else
        {
            _targetIndex++;
            if (_targetIndex >= _targets.Length)
            {
                // Player survived the entire board track and completed the game successfully
                IsGameOver = true;
            }
        }

        // Reset turn parameters
        _dartsThrownThisTurn = 0;
        _hitsThisTurn = 0;
    }

    public bool UndoLastDart()
    {
        if (_undoStack.Count == 0) return false;

        var snapshot = _undoStack.Pop();
        _targetIndex = snapshot.TargetIndex;
        _score = snapshot.Score;
        _dartsThrownThisTurn = snapshot.DartsThrownThisTurn;
        _hitsThisTurn = snapshot.HitsThisTurn;
        IsGameOver = snapshot.IsGameOver;
        TurnHistory = snapshot.TurnHistoryCopy;
        _currentTurnDarts = new List<DartThrowInfo>(snapshot.CurrentTurnDartsCopy);

        return true;
    }

    public string GetCurrentStatus()
    {
        if (IsGameOver)
        {
            return _score <= 0 ? "Defeat (Score hit 0) 💀" : $"Victory! Final Score: {_score} 🎉";
        }
        return $"Score: {_score} pts";
    }
}
