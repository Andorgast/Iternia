namespace Iternia.Scripts.Core;

public class TurnBudget
{
    public int MoveSteps { get; set; }
    public bool MainAction { get; set; }

    public void Reset()
    {
        MoveSteps = 1;
        MainAction = true;
    }

    public bool SpendMove()
    {
        if (MoveSteps > 0)
        {
            MoveSteps--;
            return true;
        }
        
        if (MainAction)
        {
            MainAction = false;
            return true;
        }

        return false;
    }
    
    public bool SpendMainAction()
    {
        if (MainAction)
        {
            MainAction = false;
            return true;
        }
        return false;
    }
}

