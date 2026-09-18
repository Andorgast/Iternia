using System.Collections.Generic;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

public class Action(string id, string name, Element attackType, int damage, int secondaryDamage, TileMask originSquare, TileMask targetSquare, bool mustTargetUnit)
{
    public string Id { get; init; } = id;
    public string Name { get; init; } = name;
    public Element AttackType { get; set; } = attackType;
    public int Damage { get; set; } = damage;
    public int SecondaryDamage { get; set; } = secondaryDamage;
    public TileMask OriginSquares { get; set; } = originSquare;
    public TileMask TargetSquares { get; set; } = targetSquare;
    public bool MustTargetUnit { get; set; } = mustTargetUnit;

    public virtual void ExecuteAction(string targetId, List<string> secondaryTargetId)
    {
        
    }
}