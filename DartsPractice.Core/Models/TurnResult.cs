namespace DartsPractice.Core.Models;

public class TurnResult
{
    public int TurnNumber { get; set; }
    public string DisplayValue { get; set; } = string.Empty;
    public int RawValue { get; set; }
    public List<DartThrowInfo> DartsThrown { get; set; } = new();
}

public class DartThrowInfo
{
    public int DartNumber { get; set; }
    public int Value { get; set; }
    public SegmentType Segment { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Points { get; set; }
}