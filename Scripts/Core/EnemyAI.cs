using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Iternia.Manager;
using Iternia.Scripts.Models;

namespace Iternia.Scripts.Core;

public class EnemyAI
{
    private static readonly Random _rng = new();

    public static GridPos? PickMoveTarget(BattleState state, EnemyUnit enemy)
    {
        if (!MovementRules.TryFindUnitPosition(state, enemy.Id, out Formation formation, out GridPos currentPos))
            return null;

        var candidates = enemy.AllowedTiles
            .GetPositions()
            .Where(pos =>
            {
                if (pos == currentPos) return false;
                if (formation.GetUnitAt(pos) != null) return false;
                int rankDiff = Math.Abs(pos.row - currentPos.row);
                int laneDiff = Math.Abs(pos.collum - currentPos.collum);
                return (rankDiff == 1 && laneDiff == 0) || (rankDiff == 0 && laneDiff == 1);
            })
            .ToList();

        if (candidates.Count == 0)
            return null;

        return candidates[_rng.Next(candidates.Count)];
    }

    public (Models.Action Action, GridPos TargetPos, GridPos? SecondTargetPos)? PickAbility(BattleState state, EnemyUnit enemy)
    {
        if (!MovementRules.TryFindUnitPosition(state, enemy.Id, out Formation formation, out GridPos enemyPos))
            return null;

        var usable = new List<(Models.Action Action, GridPos TargetPos, GridPos? SecondTargetPos)>();

        foreach (var action in enemy.Actions)
        {
            if (!action.OriginSquares.HasPos(enemyPos))
                continue;

            Formation targetFormation = action.TargetSide != TargetSide.Ally
                ? state.PlayerFormation
                : state.EnemyFormation;

            var validTargets = action.TargetSquares
                .GetPositions()
                .Where(tile => targetFormation.GetUnitAt(tile) != null && state.AllUnits.Keys.Contains(targetFormation.GetUnitAt(tile)))
                .ToList();

            GD.Print($"  [AI] ability={action.Id} originOk=true validTargets={validTargets.Count}");

            if (validTargets.Count > 0)
            {
                GridPos chosenTarget = validTargets[_rng.Next(validTargets.Count)];
                usable.Add((action, chosenTarget, null));
            }
        }

        if (usable.Count == 0)
            return null;

        return usable[_rng.Next(usable.Count)];
    }
}

