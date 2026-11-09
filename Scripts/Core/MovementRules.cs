using System;
using System.Collections.Generic;
using Iternia.Scripts.Models;

namespace Iternia.Scripts.Core;

public static class MovementRules
{
    public static bool CanMove(BattleState state, Unit unit, GridPos targetPos)
    {
        if (
                TryFindUnitPosition(state, unit.Name, out Formation formation, out GridPos currentPos) &&
                (unit.Movement >= 0 || unit.ActionPoints >= 0) &&
                state.ActiveUnit?.Name == unit.Name
            )
        {
            int rankDiff = Math.Abs(currentPos.collum - targetPos.collum);
            int laneDiff = Math.Abs(currentPos.row - targetPos.row);
            if ((rankDiff == 1 && laneDiff == 0) || (rankDiff == 0 && laneDiff == 1))
            {
                if (formation.GetUnitAt(targetPos) != null)
                {
                    return false;
                }

                return true;
            }
        }
        return false;
    }

    public static bool TryFindUnitPosition(BattleState state, string unitId, out Formation outFormation, out GridPos outPos)
    {
        for (int lane = 0; lane < 3; lane++)
        {
            for (int rank = 0; rank < 3; rank++)
            {
                var pos = new GridPos(rank, lane);
                if (state.PlayerFormation.GetUnitAt(pos) == unitId)
                {
                    outFormation = state.PlayerFormation;
                    outPos = pos;
                    return true;
                }
                if (state.EnemyFormation.GetUnitAt(pos) == unitId)
                {
                    outFormation = state.EnemyFormation;
                    outPos = pos;
                    return true;
                }
            }
        }

        outFormation = null;
        outPos = default;
        return false;
    }

    public static List<GridPos> GetValidMoves(BattleState state, Unit unit)
    {
        var validMoves = new List<GridPos>();
        for (int rank = 0; rank < 3; rank++)
        {
            for (int lane = 0; lane < 3; lane++)
            {
                var target = new GridPos(rank, lane);
                if (CanMove(state, unit, target))
                {
                    validMoves.Add(target);
                }
            }
        }
        return validMoves;
    }
}

