using System;
using System.Collections.Generic;

namespace Iternia.Scripts.Core;

public static class MovementRules
{
    public static bool CanMove(BattleState state, string unitId, GridPos targetPos)
    {
        if (state.ActiveUnitId != null && state.ActiveUnitId != unitId)
        {
            return false;
        }

        if (state.CurrentTurnBudget.MoveSteps <= 0 && state.CurrentTurnBudget.ActionPoints <= 0)
        {
            return false;
        }

        if (!TryFindUnitPosition(state, unitId, out Formation formation, out GridPos currentPos))
        {
            return false;
        }

        if (targetPos.row < 0 || targetPos.row > 2 || targetPos.collum < 0 || targetPos.collum > 2)
        {
            return false;
        }

        int rankDiff = Math.Abs(currentPos.row - targetPos.row);
        int laneDiff = Math.Abs(currentPos.collum - targetPos.collum);
        if ((rankDiff == 1 && laneDiff == 0) || (rankDiff == 0 && laneDiff == 1))
        {
            if (formation.GetUnitAt(targetPos) != null)
            {
                return false;
            }

            return true;
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

    public static List<GridPos> GetValidMoves(BattleState state, string unitId)
    {
        var validMoves = new List<GridPos>();
        for (int rank = 0; rank < 3; rank++)
        {
            for (int lane = 0; lane < 3; lane++)
            {
                var target = new GridPos(rank, lane);
                if (CanMove(state, unitId, target))
                {
                    validMoves.Add(target);
                }
            }
        }
        return validMoves;
    }
}

