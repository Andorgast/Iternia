using Godot;
using Godot.Collections;
using Iternia.Scripts.Core;

namespace Iternia.Scripts.Models;

[GlobalClass]
public partial class Unit : Resource
{
    [Export] public string Id { get; protected set; }
    [Export] public string Name { get; protected set; }
    [Export] public TargetSide Side { get; set; }
    [Export] public Array<Action> Actions { get; protected set; }
    [Export] public Array<Element> Resistances { get; set; }
    [Export] public Array<Element> Weaknesses { get; set; }
    //The values below shouldn't be used for anything, these are here for easy editing in the editor
    [Export] protected int InitialAttack;
    [Export] protected int InitialHp;
    [Export] protected int InitialMovement;
    [Export] protected int InitialActionPoints;
    [Export] protected int InitialSpeed;
    [Export] protected float InitialAggro;
    
    //The actual attributes that the game should use
    public int Attack;
    public int DefaultAttack;
    public int Hp;
    public int MaxHp;
    public int Speed;
    public int DefaultSpeed;
    public int MaxMovement;
    public int Movement;
    public int DefaultMovement;
    public int MaxActionPoints;
    public int ActionPoints;
    public int DefaultActionPoints;
    public float Aggro;
    public float DefaultAggro;
    public int TakeDamage(int damageToTake, Element attackType)
    {
        if (Resistances.Contains(attackType)) Hp -= damageToTake / 1;
        else if (Weaknesses.Contains(attackType)) Hp -= damageToTake * 1;
        return Hp;
    }
    public Unit()
    {
        Attack = InitialAttack;
        DefaultAttack = InitialAttack;
        Hp = InitialHp;
        MaxHp = InitialHp;
        Speed = InitialSpeed;
        DefaultSpeed = InitialSpeed;
        MaxMovement = InitialMovement;
        Movement = InitialMovement; 
        DefaultMovement = InitialMovement;
        MaxActionPoints = InitialActionPoints;
        ActionPoints = InitialActionPoints;
        DefaultActionPoints = InitialActionPoints;
        Aggro = InitialAggro;
        DefaultAggro = InitialAggro;
    }
}

