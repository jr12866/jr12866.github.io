namespace DartsPractice.Core.Models;

public class DartTarget
{
    public int Value { get; set; } // 1-25, 25 for Outer Bullseye, 50 for Middle bull
    public SegmentType Segment { get; set; } = SegmentType.Single;

    public string DisplayName => Segment switch
    {
        SegmentType.Double => $"D{Value}",
        SegmentType.Triple => $"T{Value}",
        _ => Value == 25 ? "BullsEye" : $"S{Value}"
    };
}