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

public enum Stat
{
    None,
    MaxHp,
    Hp,
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

public enum TileColorReason
{
    MovePossible,
    OriginForAction,
    TargetForAction,
    Hover,
    Reset
}

public enum TileEffectType
{
    TempStatChange,
    PermStatChange,
    TileRemove
}