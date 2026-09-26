namespace Iternia.Scripts.Core;

public enum TargetSide
{
    Ally,
    Enemy,
    Both
}

public enum Element
{
    Fire,
    Water,
    Plant,
    Ice,
    Physical,
    Dark
}

public enum ActionType
{
    Attack,
    Buff,
    Debuff,
    TileEffect,
    AoiAttack,
    AoiTileEffect,
    BuffAndDebuff,
}

public enum Stat
{
    None,
    MaxHp,
    Attack,
    Speed,
    Aggro,
    MaxMovement,
    Movement,
    MaxActionPoints,
    ActionPoints
}

public enum TargetType
{
    Tile,
    Unit,
    EnemyAndAlly,
}

public enum AllyTargetRange
{
    Full,
    Self,
    NextToSelf,
    DefinedByTargetGrid,
    DoesntTargetAlly
}