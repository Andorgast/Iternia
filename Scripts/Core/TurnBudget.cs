using Iternia.Scripts.Models;

namespace Iternia.Scripts.Core;

public class TurnBudget
{
    public int MoveSteps { get; set; }
    public int ActionPoints { get; set; }

    public void Reset(Unit activeUnit)
    {
        MoveSteps = activeUnit.Movement;
        ActionPoints = activeUnit.ActionPoints;
    }

    public bool SpendMove()
    {
        if (MoveSteps > 0)
        {
            MoveSteps--;
            return true;
        }
        
        if (ActionPoints > 0)
        {
            ActionPoints--;
            return true;
        }

        return false;
    }
    
    public bool SpendMainAction()
    {
        if (ActionPoints > 0)
        {
            ActionPoints--;
            return true;
        }
        return false;
    }
}

