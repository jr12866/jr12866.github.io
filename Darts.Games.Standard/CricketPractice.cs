using DartsPractice.Core.Interfaces;
using DartsPractice.Core.Models;

namespace DartsPractice.Games.Standard;

public class CricketPractice : IDartsGame
{
    public string Name => "Cricket Target Practice";
    public string Description => "Close numbers 15 through 20 and the Bullseye by hitting each 3 times.";
    public GameInputType InputType => GameInputType.HitCount; 
    public bool IsGameOver { get; private set; }
    
    public DartTarget CurrentTarget => GetNextUnclosedTarget();
    public int DartsRemainingInTurn => 3 - _dartsThrownThisTurn;
    public List<TurnResult> TurnHistory { get; private set; } = new();

    public List<GamePreset> AvailablePresets => new()
    {
        new GamePreset { Id = "standard", Name = "Standard Sequence", Description = "Close 15 to 20 sequentially, then the Bullseye." },
        new GamePreset { Id = "free", Name = "Free Aim Mode", Description = "Aim at any open number. Target display shows the lowest open number." }
    };

    private readonly int[] _cricketNumbers = { 15, 16, 17, 18, 19, 20, 25 };
    private readonly Dictionary<int, int> _hitCounters = new(); 
    private string _activePresetId = "standard";
    private int _dartsThrownThisTurn;
    private int _marksThisTurn;
    private List<DartThrowInfo> _currentTurnDarts = new();
    private readonly Stack<CricketSnapshot> _undoStack = new();

    private class CricketSnapshot
    {
        public Dictionary<int, int> HitCountersCopy { get; set; } = new();
        public int DartsThrownThisTurn { get; set; }
        public int MarksThisTurn { get; set; }
        public bool IsGameOver { get; set; }
        public List<TurnResult> TurnHistoryCopy { get; set; } = new();
        public List<DartThrowInfo> CurrentTurnDartsCopy { get; set; } = new();
    }

    public void SetupGame(string presetId)
    {
        _activePresetId = string.IsNullOrWhiteSpace(presetId) ? "standard" : presetId;
    }

    public void StartGame()
    {
        IsGameOver = false;
        _dartsThrownThisTurn = 0;
        _marksThisTurn = 0;
        TurnHistory.Clear();
        _currentTurnDarts.Clear();
        _undoStack.Clear();

        foreach (var num in _cricketNumbers)
        {
            _hitCounters[num] = 0;
        }
    }

    public bool RecordDart(DartTarget hitTarget)
    {
        // 1. Capture snapshot BEFORE modifying any variables for perfect Undo support
        _undoStack.Push(new CricketSnapshot
        {
            HitCountersCopy = new Dictionary<int, int>(_hitCounters),
            DartsThrownThisTurn = _dartsThrownThisTurn,
            MarksThisTurn = _marksThisTurn,
            IsGameOver = IsGameOver,
            TurnHistoryCopy = new List<TurnResult>(TurnHistory),
            CurrentTurnDartsCopy = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _dartsThrownThisTurn++;
        int multiplier = hitTarget.Segment switch
        {
            SegmentType.Triple => 3,
            SegmentType.Double => 2,
            _ => 1
        };
        int dartMarks = hitTarget.Value == 25 && hitTarget.Segment == SegmentType.Triple ? 2 : multiplier;

        _currentTurnDarts.Add(new DartThrowInfo
        {
            DartNumber = _dartsThrownThisTurn,
            Value = hitTarget.Value,
            Segment = hitTarget.Segment,
            DisplayName = hitTarget.DisplayName,
            Points = dartMarks
        });
        if (_hitCounters.ContainsKey(hitTarget.Value))
        {
            int currentHits = _hitCounters[hitTarget.Value];

            if (currentHits < 3)
            {
                // Sequence Restriction Check
                if (_activePresetId == "standard" && hitTarget.Value != CurrentTarget.Value)
                {
                    return _dartsThrownThisTurn >= 3; 
                }

                // Translate segment type to marks
                int marksScored = hitTarget.Segment switch
                {
                    SegmentType.Triple => hitTarget.Value == 25 ? 2 : 3, // Bullseye can only be Double max
                    SegmentType.Double => 2,
                    _ => 1
                };

                int actualMarksAdded = Math.Min(marksScored, 3 - currentHits);
                _hitCounters[hitTarget.Value] += actualMarksAdded;
                _marksThisTurn += actualMarksAdded;

                // Win Condition Check
                if (_hitCounters.Values.All(hits => hits >= 3))
                {
                    IsGameOver = true;
                    return true; 
                }
            }
        }

        return _dartsThrownThisTurn >= 3;
    }

    public void EndTurn()
    {
        TurnHistory.Add(new TurnResult
        {
            TurnNumber = TurnHistory.Count + 1,
            DisplayValue = $"{_marksThisTurn} Marks",
            RawValue = _marksThisTurn,
            DartsThrown = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _currentTurnDarts.Clear();
        _dartsThrownThisTurn = 0;
        _marksThisTurn = 0;
    }

    public bool UndoLastDart()
    {
        if (_undoStack.Count == 0) return false;

        var snapshot = _undoStack.Pop();
        _dartsThrownThisTurn = snapshot.DartsThrownThisTurn;
        _marksThisTurn = snapshot.MarksThisTurn;
        IsGameOver = snapshot.IsGameOver;
        TurnHistory = snapshot.TurnHistoryCopy;
        _currentTurnDarts = new List<DartThrowInfo>(snapshot.CurrentTurnDartsCopy);

        _hitCounters.Clear();
        foreach (var kvp in snapshot.HitCountersCopy)
        {
            _hitCounters[kvp.Key] = kvp.Value;
        }

        return true;
    }

    public string GetCurrentStatus()
    {
        var statusStrings = _cricketNumbers.Select(num =>
        {
            string label = num == 25 ? "B" : num.ToString();
            string marks = _hitCounters[num] switch { 1 => "/", 2 => "X", 3 => "O", _ => "-" };
            return $"{label}:{marks}";
        });
        return string.Join(" | ", statusStrings);
    }

    private DartTarget GetNextUnclosedTarget()
    {
        foreach (var num in _cricketNumbers)
        {
            if (_hitCounters[num] < 3)
                return new DartTarget { Value = num, Segment = SegmentType.Single };
        }
        return new DartTarget { Value = 0, Segment = SegmentType.Single };
    }
}
