using DartsPractice.Core.Interfaces;
using DartsPractice.Core.Models;

namespace DartsPractice.Games.Standard;

public class Practice501 : IDartsGame
{
    public string Name => "X01 Point Practice";
    public string Description => "Reduce your score to exactly 0. Standard bust rules apply.";
    public GameInputType InputType => GameInputType.Score; // Explicitly expects score values
    public bool IsGameOver { get; private set; }
    
    // For x01, we display the current score total as the main visual target indicator
    public DartTarget CurrentTarget => new DartTarget { Value = _score, Segment = SegmentType.Single };
    public int DartsRemainingInTurn => 3 - _dartsThrownThisTurn;
    public List<TurnResult> TurnHistory { get; private set; } = new();

    public List<GamePreset> AvailablePresets => new()
    {
        new GamePreset { Id = "501", Name = "501 Standard", Description = "Start with 501 points." },
        new GamePreset { Id = "301", Name = "301 Fast Track", Description = "Start with 301 points." },
        new GamePreset { Id = "701", Name = "701 Endurance", Description = "Start with 701 points." }
    };

private int _score;
    private int _startingScore = 501;
    private int _dartsThrownThisTurn;
    private int _pointsScoredThisTurn;
    private bool _isBustThisTurn;
    private List<DartThrowInfo> _currentTurnDarts = new();
    private readonly Stack<X01Snapshot> _undoStack = new();

    private class X01Snapshot
    {
        public int Score { get; set; }
        public int DartsThrownThisTurn { get; set; }
        public int PointsScoredThisTurn { get; set; }
        public bool IsBustThisTurn { get; set; }
        public bool IsGameOver { get; set; }
        public List<TurnResult> TurnHistoryCopy { get; set; } = new();
        public List<DartThrowInfo> CurrentTurnDartsCopy { get; set; } = new();
    }

    public void SetupGame(string presetId)
    {
        _startingScore = presetId switch
        {
            "301" => 301,
            "701" => 701,
            _ => 501
        };
    }

    public void StartGame()
    {
        _score = _startingScore;
        _dartsThrownThisTurn = 0;
        _pointsScoredThisTurn = 0;
        _isBustThisTurn = false;
        IsGameOver = false;
        TurnHistory.Clear();
        _currentTurnDarts.Clear();
        _undoStack.Clear();
    }

    public bool RecordDart(DartTarget hitTarget)
    {
        // 1. Capture exact historical snapshot prior to changing any state properties
        _undoStack.Push(new X01Snapshot
        {
            Score = _score,
            DartsThrownThisTurn = _dartsThrownThisTurn,
            PointsScoredThisTurn = _pointsScoredThisTurn,
            IsBustThisTurn = _isBustThisTurn,
            IsGameOver = IsGameOver,
            TurnHistoryCopy = new List<TurnResult>(TurnHistory),
            CurrentTurnDartsCopy = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _dartsThrownThisTurn++;

        // If the turn is already busted by dart 1 or 2, subsequent darts this turn are dead misses
        if (_isBustThisTurn)
        {
            return _dartsThrownThisTurn >= 3;
        }

        // 2. Calculate point multiplier based on the structural segment definition
        int multiplier = hitTarget.Segment switch
        {
            SegmentType.Triple => 3,
            SegmentType.Double => 2,
            _ => 1
        };
        
        int dartPoints = hitTarget.Value * multiplier;

        _currentTurnDarts.Add(new DartThrowInfo
        {
            DartNumber = _dartsThrownThisTurn,
            Value = hitTarget.Value,
            Segment = hitTarget.Segment,
            DisplayName = hitTarget.DisplayName,
            Points = dartPoints
        });

        // 3. Evaluate X01 rules & Bust parameters
        int remainingScore = _score - dartPoints;

        if (remainingScore == 0)
        {
            // Game Won!
            _score = 0;
            _pointsScoredThisTurn += dartPoints;
            IsGameOver = true;
            return true; // Force turn execution end early
        }
        else if (remainingScore < 0 || remainingScore == 1)
        {
            // BUST RULE: Going below 0 or landing exactly on 1 is a bust (cannot finish on 1)
            _isBustThisTurn = true;
            
            // Revert point updates accumulated *during this specific turn*
            _score += _pointsScoredThisTurn; 
            _pointsScoredThisTurn = 0;
        }
        else
        {
            // Valid scoring dart
            _score = remainingScore;
            _pointsScoredThisTurn += dartPoints;
        }

        return _dartsThrownThisTurn >= 3;
    }

    public void EndTurn()
    {
        string displayLog = _isBustThisTurn ? "BUST" : $"{_pointsScoredThisTurn} pts";

        TurnHistory.Add(new TurnResult
        {
            TurnNumber = TurnHistory.Count + 1,
            DisplayValue = displayLog,
            RawValue = _pointsScoredThisTurn,
            DartsThrown = new List<DartThrowInfo>(_currentTurnDarts)
        });

        _currentTurnDarts.Clear();
        _dartsThrownThisTurn = 0;
        _pointsScoredThisTurn = 0;
        _isBustThisTurn = false;
    }

    public bool UndoLastDart()
    {
        if (_undoStack.Count == 0) return false;

        var snapshot = _undoStack.Pop();
        
        _score = snapshot.Score;
        _dartsThrownThisTurn = snapshot.DartsThrownThisTurn;
        _pointsScoredThisTurn = snapshot.PointsScoredThisTurn;
        _isBustThisTurn = snapshot.IsBustThisTurn;
        IsGameOver = snapshot.IsGameOver;
        TurnHistory = snapshot.TurnHistoryCopy;
        _currentTurnDarts = new List<DartThrowInfo>(snapshot.CurrentTurnDartsCopy);

        return true;
    }

    public string GetCurrentStatus()
    {
        return IsGameOver ? "Finished! 🎉" : $"Remaining Score: {_score}";
    }
}
