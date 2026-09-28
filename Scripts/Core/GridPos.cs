namespace Iternia.Scripts.Core;

public readonly record struct GridPos(int collum, int row)
{
    public static GridPos operator +(GridPos a, GridPos b) => new(a.collum + b.collum, a.row + b.row);
    public static GridPos operator -(GridPos a, GridPos b) => new(a.collum - b.collum, a.row - b.row);
}