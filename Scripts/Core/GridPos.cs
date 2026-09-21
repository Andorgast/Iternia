namespace Iternia.Scripts.Core;

public readonly record struct GridPos(int row, int collum)
{
    public static GridPos operator +(GridPos a, GridPos b) => new(a.row + b.row, a.collum + b.collum);
    public static GridPos operator -(GridPos a, GridPos b) => new(a.row - b.row, a.collum - b.collum);
}