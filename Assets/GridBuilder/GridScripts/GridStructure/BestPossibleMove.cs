using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BestPossibleMove 
{
    private GridLogic gridLogic; 

    public BestPossibleMove(GridLogic gridLogic)
    {
        this.gridLogic = gridLogic; 
    }

    public PossibleMove FindBestPossibleMove()
    {
        List<PossibleMove> allPossibleMove = gridLogic.GetAllPossibleMove(); 

        if (allPossibleMove.Count < 1) return null;

        PossibleMove bestPossibleMove = allPossibleMove[0];

        for (int i = 0; i < allPossibleMove.Count; i++)
        {
            PossibleMove testPossibleMove = allPossibleMove[i];
            GridItemPosition startGridPosition = gridLogic.GetGridObject(testPossibleMove.GetStartX(), testPossibleMove.GetStartY());
            GridItemPosition endGridPosition = gridLogic.GetGridObject(testPossibleMove.GetEndX(), testPossibleMove.GetEndY());

            if (startGridPosition.HasBooster() || endGridPosition.HasBooster())
            {
                bestPossibleMove = testPossibleMove;
                return bestPossibleMove;
            }
        }

        return bestPossibleMove;
    }
}
