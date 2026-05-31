using DartsPractice.Core.Interfaces;
using DartsPractice.Core.Models;

namespace DartsPractice.Games.Standard;

public class ConfigurableRoundRobin : IDartsGame
{
    public string Name => "Round Robin Targets";
    public string Description => "Hit target on 1st dart to advance early. Miss all 3 to advance with a penalty.";
    public GameInputType InputType => GameInputType.HitCount;
    public bool IsGameOver { get; private set; }
    
    public DartTarget CurrentTarget => _targetIndex < _targets.Count ? _targets[_targetIndex] : new DartTarget { Value = 0 };
    public int DartsRemainingInTurn => 3 - _dartsThrownThisTurn;
    public List<TurnResult> TurnHistory { get; private set; } = new();

    public List<GamePreset> AvailablePresets => new()
    {
        new GamePreset { Id = "singles", Name = "Singles Circuit", Description = "Hit numbers 15 to 20 + Bullseye as singles." },
        new GamePreset { Id = "doubles", Name = "Double Out Practice", Description = "Hit doubles 15 to 20 + Double Bull." },
        new GamePreset { Id = "triples", Name = "Scoring Power (Triples)", Description = "Hit triples 15 to 20." }
    };

    private readonly List<DartTarget> _targets = new();
    private int _targetIndex;
    private int _dartsThrownThisTurn;
    private int _hitsThisTurn;
    private List<DartThrowInfo> _currentTurnDarts = new();
    private readonly Stack<RoundRobinSnapshot> _undoStack = new();

    private class RoundRobinSnapshot
    {
        public int TargetIndex { get; set; }
        public int DartsThrownThisTurn { get; set; }
        public int HitsThisTurn { get; set; }
        public bool IsGameOver { get; set; }
        public List<TurnResult> TurnHistoryCopy { get; set; } = new();
        public List<DartThrowInfo> CurrentTurnDartsCopy { get; set; } = new();
    }

    public void SetupGame(string presetId)
    {
        _targets.Clear();
        int[] coreNumbers = { 15, 16, 17, 18, 19, 20 };

        switch (presetId)
        {
            case "doubles":
                foreach (var n in coreNumbers) _targets.Add(new DartTarget { Value = n, Segment = SegmentType.Double });
                _targets.Add(new DartTarget { Value = 25, Segment = SegmentType.Double }); 
                break;
            case "triples":
                foreach (var n in coreNumbers) _targets.Add(new DartTarget { Value = n, Segment = SegmentType.Triple });
                break;
            case "singles":
            default:
                foreach (var n in coreNumbers) _targets.Add(new DartTarget { Value = n, Segment = SegmentType.Single });
                _targets.Add(new DartTarget { Value = 25, Segment = SegmentType.Single }); 
                break;
        }
    }

    public void StartGame()
    {
        _targetIndex = 0;
        _dartsThrownThisTurn = 0;
        _hitsThisTurn = 0;
        IsGameOver = false;
        TurnHistory.Clear();
        _currentTurnDarts.Clear();
        _undoStack.Clear();
        
        if (!_targets.Any()) SetupGame("singles"); // Safeguard if SetupGame wasn't called
    }

    public bool RecordDart(DartTarget hitTarget)
    {
        _undoStack.Push(new RoundRobinSnapshot
        {
            TargetIndex = _targetIndex,
            DartsThrownThisTurn = _dartsThrownThisTurn,
            HitsThisTurn = _hitsThisTurn,
            IsGameOver = IsGameOver,
            TurnHistoryCopy = new List<TurnResult>(TurnHistory),
            CurrentTurnDartsCopy = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _dartsThrownThisTurn++;
        int multiplier = hitTarget.Segment switch { SegmentType.Triple => 3, SegmentType.Double => 2, _ => 1 };

        _currentTurnDarts.Add(new DartThrowInfo
        {
            DartNumber = _dartsThrownThisTurn,
            Value = hitTarget.Value,
            Segment = hitTarget.Segment,
            DisplayName = hitTarget.DisplayName,
            Points = hitTarget.Value * multiplier
        });

        bool isHit = (hitTarget.Value == CurrentTarget.Value && hitTarget.Segment == CurrentTarget.Segment);

        if (isHit) _hitsThisTurn++;

        // RULE: Hit target on the 1st dart -> Advance immediately and end turn early
        if (isHit && _dartsThrownThisTurn == 1)
        {
            AdvanceTarget();
            return true; 
        }

        // RULE: Subsequent hits on darts 2 or 3 also advance the target
        if (isHit && _dartsThrownThisTurn > 1)
        {
            AdvanceTarget();
        }

        // Check if standard 3 darts are complete
        if (_dartsThrownThisTurn >= 3)
        {
            // RULE: If you missed all 3 darts, you still advance anyway (penalty)
            if (_hitsThisTurn == 0)
            {
                AdvanceTarget();
            }
            return true; 
        }

        return false; 
    }

    public void EndTurn()
    {
        TurnHistory.Add(new TurnResult
        {
            TurnNumber = TurnHistory.Count + 1,
            DisplayValue = $"Target {_targets[Math.Min(_targetIndex, _targets.Count - 1)].DisplayName}: {_hitsThisTurn} hits",
            RawValue = _hitsThisTurn,
            DartsThrown = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _currentTurnDarts.Clear();
        _dartsThrownThisTurn = 0;
        _hitsThisTurn = 0;
    }

    public bool UndoLastDart()
    {
        if (_undoStack.Count == 0) return false;

        var snapshot = _undoStack.Pop();
        _targetIndex = snapshot.TargetIndex;
        _dartsThrownThisTurn = snapshot.DartsThrownThisTurn;
        _hitsThisTurn = snapshot.HitsThisTurn;
        IsGameOver = snapshot.IsGameOver;
        TurnHistory = snapshot.TurnHistoryCopy;
        _currentTurnDarts = new List<DartThrowInfo>(snapshot.CurrentTurnDartsCopy);

        return true;
    }

    private void AdvanceTarget()
    {
        _targetIndex++;
        if (_targetIndex >= _targets.Count) IsGameOver = true;
    }

    public string GetCurrentStatus() => $"Progress: {_targetIndex} / {_targets.Count} Targets Complete";
}
