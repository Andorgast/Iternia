using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

public class Action(int id, string name, TileMask originSquare, TileMask targetSquare, bool mustTargetUnit)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
    public TileMask OriginSquares { get; set; } = originSquare;
    public TileMask TargetSquares { get; set; } = targetSquare;
    public bool MustTargetUnit { get; set; } = mustTargetUnit;

    public bool ExecuteAction(TileMask target)
    {
        //TODO MAKE THE SHIT FUNCTION AND DO THE STUFF
        return true;
    }
}