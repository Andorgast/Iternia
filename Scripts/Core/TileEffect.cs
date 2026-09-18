namespace Iternia.Scripts.Core;

public record TileEffect(TargetSide Side, GridPos Pos, TileEffectType EffectType, int DurationInTurns);