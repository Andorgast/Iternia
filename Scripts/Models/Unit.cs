using System.Collections.Generic;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

public class Unit
{
    public virtual string Id { get; init; }
    public virtual string Name { get; init; }
    public virtual TargetSide Side { get; set; }
    public virtual int DefaultAttack { get; set; } = 1;
    public virtual int Attack { get; set; }
    public virtual int Hp { get; set; }
    public virtual int MaxHp { get; set; }
    public virtual int Speed { get; set; }
    public virtual int DefaultSpeed { get; set; }
    //Speed is where in the turn order a unit is
    public virtual int Movement { get; set; }
    public virtual int DefaultMovement { get; set; }
    //Movement is how many times the unit can move without using action points
    public virtual int ActionPoints { get; set; }
    public virtual int DefaultActionPoints { get; set; }
    //Action points is the amount of things a unit can do something in a turn
    public virtual float Aggro { get; set; }
    public virtual float DefaultAggro { get; set; }
    public virtual List<Action> Actions { get; init; }
    public virtual List<Element> Resistances { get; set; }
    public virtual List<Element> Weaknesses { get; set; }
    public int TakeDamage(int damageToTake, Element attackType)
    {
        if (Resistances.Contains(attackType)) Hp -= damageToTake / 1;
        else if (Weaknesses.Contains(attackType)) Hp -= damageToTake * 1;
        return Hp;
    }
}

