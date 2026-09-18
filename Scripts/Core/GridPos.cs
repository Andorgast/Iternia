namespace Iternia.Scripts.Core;

public readonly record struct GridPos(int Rank, int Lane)
{
    public static GridPos operator +(GridPos a, GridPos b) => new(a.Rank + b.Rank, a.Lane + b.Lane);
    public static GridPos operator -(GridPos a, GridPos b) => new(a.Rank - b.Rank, a.Lane - b.Lane);
}