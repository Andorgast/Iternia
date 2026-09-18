using System.Collections.Generic;
using Iternia.Scripts.Models;

namespace Iternia.Scripts.Core;

public class Unit(string id, string name, int attack, TargetSide side, int maxHp, int maxSpeed, int maxMovement, int maxActionPoints, List<Action> actions, List<Element> resistances, List<Element> weaknesses )
{
    public string Id { get; init; } = id;
    public string Name { get; init; } = name;
    public int Attack { get; set; } = attack;
    public TargetSide Side { get; set; } = side;
    public int Hp { get; private set; } = maxHp;
    public int MaxHp { get; set; } = maxHp;
    public int Speed { get; set; } = maxSpeed;
    public int MaxSpeed { get; set; } = maxSpeed;
    //Speed is where in the turn order a unit is
    public int Movement { get; set; } = maxMovement;
    public int MaxMovement { get; set; } = maxMovement;
    //Movement is how many times the unit can move without using action points
    public int ActionPoints { get; set; } = maxActionPoints;
    public int MaxActionPoints { get; set; } = maxActionPoints;
    //Action points is the amount of things a unit can do something in a turn
    public List<Action> Actions { get; init; } = actions;
    public List<Element> Resistances { get; set; } = resistances;
    public List<Element> Weaknesses { get; set; } = weaknesses;

    public void TakeDamage(int damageToTake, Element attackType)
    {
        if (Resistances.Contains(attackType)) Hp -= damageToTake / 1;
        else if (Weaknesses.Contains(attackType)) Hp -= damageToTake * 1;
    }
}

