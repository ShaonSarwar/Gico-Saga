using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq; 

public class InsulatorLogic : MonoBehaviour
{
    public event EventHandler OnReplaceItemWithRegister; 

    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private GridLogicVisual gridLogicVisual;

    private Grid<GridItemPosition> grid;
    private LevelSO levelSO;
    private int gridWidth;
    private int gridHeight;
    private List<GridItemPosition> insulatorSourceList; 
    private int rightRegisterAmount;
    private int leftRegisterAmount;
    private List<MatchLink> allLinkedGridItemPositionList; 

    private void Awake()
    {
        gridLogic.OnLevelSet += GridLogic_OnLevelSet;
        gridLogic.OnMoveUsed += GridLogic_OnMoveUsed;
        gridLogicVisual.OnBeforeRegisterSpawned += GridLogic_OnBeforeRegisterSpawned;
    }

    private void GridLogic_OnBeforeRegisterSpawned(object sender, EventArgs e)
    {
        allLinkedGridItemPositionList = new List<MatchLink>();
        allLinkedGridItemPositionList = gridLogic.GetAllMatchLinks(); 
    }

    private void GridLogic_OnLevelSet(object sender, GridLogic.OnLevelSetEventArgs e)
    {
        levelSO = e.levelSO;
        SetLevel(e.grid);
    }

    public void BuildRegister()
    {
        FindInsulatorSource();
        if (insulatorSourceList.Count < 1) return;
        foreach (GridItemPosition gridItemPosition in insulatorSourceList)
        {
            if (gridItemPosition.HasGridItem())
            {
                int horizontalX = gridItemPosition.GetX();
                int verticalY = gridItemPosition.GetY();

                // Make Register Right side first 
                int rightHorizontalX = horizontalX + 1;
                // Make Register Left side 
                int leftHorizontalX = horizontalX - 1;

                int randNum = UnityEngine.Random.Range(0, 2); 

                if (randNum == 0)
                {
                    if (IsLeftSideReadyForBeingRegister(leftHorizontalX, verticalY) != null)
                    {
                        MakeRegister(IsLeftSideReadyForBeingRegister(leftHorizontalX, verticalY));
                    }
                    else
                    {
                        if (IsRightSideReadyForBeingRegister(rightHorizontalX, verticalY) != null)
                        {
                            MakeRegister(IsRightSideReadyForBeingRegister(rightHorizontalX, verticalY));
                        }
                    }
                }
                else if(randNum == 1)
                {
                    if (IsRightSideReadyForBeingRegister(rightHorizontalX, verticalY) != null)
                    {
                        MakeRegister(IsRightSideReadyForBeingRegister(rightHorizontalX, verticalY));
                    }
                    else
                    {
                        if (IsLeftSideReadyForBeingRegister(leftHorizontalX, verticalY) != null)
                        {
                            MakeRegister(IsLeftSideReadyForBeingRegister(leftHorizontalX, verticalY));
                        }
                    }
                }
            }

        }
    }

    private void GridLogic_OnMoveUsed(object sender, EventArgs e)
    {
       
      
    }

    private void SetLevel(Grid<GridItemPosition> grid)
    {
        this.grid = grid; 
        gridWidth = levelSO.width;
        gridHeight = levelSO.height;
        //FindInsulatorSource();
    }

    public void FindInsulatorSource()
    {
        insulatorSourceList = new List<GridItemPosition>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridLogic.IsValidPosition(x, y) && gridLogic.IsGridItemPositionInsulator(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (gridItemPosition.HasGridItem())
                    {
                        insulatorSourceList.Add(gridItemPosition); 
                    }
                }
            }
        }
    }

    //public void CreateRegister(GridItemPosition insulatorSourceGrid, bool isRightSide)
    //{
    //    if (insulatorSourceGrid.HasGridItem())
    //    {
    //        int horizontalX = insulatorSourceGrid.GetX(); 
    //        int verticalY = insulatorSourceGrid.GetY();

    //        // Make Register Right side first 
    //        int rightHorizontalX = horizontalX + 1;
    //        // Make Register Left side 
    //        int leftHorizontalX = horizontalX - 1;

    //        if (isRightSide)
    //        {
    //            if (IsRightSideReadyForBeingRegister(rightHorizontalX, verticalY))
    //            {
    //                rightRegisterAmount++;
    //            }
    //        }
    //        else
    //        {
    //            if (IsLeftSideReadyForBeingRegister(leftHorizontalX, verticalY))
    //            {
    //                leftRegisterAmount++;
    //            }
    //        }
    //    }
    //}

    public GridItemPosition IsRightSideReadyForBeingRegister(int x, int y)
    {
        for (int i = x; i < gridWidth; i++)
        {
            if (gridLogic.IsValidPosition(i, y) && !gridLogic.IsGridItemPositionInsulator(i, y))
            {
                GridItemPosition gridItemPosition = grid.GetGridObject(i, y);
                if (gridItemPosition.IsRegister()) continue;
                if (AnyMatchAroundNewRegister(gridItemPosition)) return null; 
                return gridItemPosition;
            }         
        }
        return null; 
    }

    public void MakeRegister(GridItemPosition gridItemPosition)
    {
        OnReplaceItemWithRegister?.Invoke(gridItemPosition, EventArgs.Empty);
    }

    public GridItemPosition IsLeftSideReadyForBeingRegister(int x, int y)
    {
        for (int i = x; i >= 0; i--)
        {
            if (gridLogic.IsValidPosition(i, y) && !gridLogic.IsGridItemPositionInsulator(i, y))
            {
                GridItemPosition gridItemPosition = grid.GetGridObject(i, y);
                if (gridItemPosition.IsRegister()) continue;
                if (AnyMatchAroundNewRegister(gridItemPosition)) return null; 
                return gridItemPosition;
            }
        }
        return null; 
    }

    public bool AnyMatchAroundNewRegister(GridItemPosition gridItemPosition)
    {
        List<Vector2Int> allSurroundingPositionList = RegisterSurroundingArea(gridItemPosition);

        if (allLinkedGridItemPositionList.Count < 1) return false;
        if (allSurroundingPositionList.Count < 1) return false; 

        foreach (Vector2Int surroundingPosition in allSurroundingPositionList)
        {
            if (gridLogic.IsValidPosition(surroundingPosition.x, surroundingPosition.y))
            {
                GridItemPosition registerSurroundingPosition = grid.GetGridObject(surroundingPosition.x, surroundingPosition.y);
                foreach (MatchLink matchLink in allLinkedGridItemPositionList)
                {
                    if (matchLink.GetLinkedGridItemPositionList().Any(gridPosition => (gridPosition.GetX() == registerSurroundingPosition.GetX() && gridPosition.GetY() == registerSurroundingPosition.GetY())))
                    {
                        return true; 
                    }
                }
            }
        }

        return false; 
    }

    public List<Vector2Int> RegisterSurroundingArea(GridItemPosition gridItemPosition)
    {
        List<Vector2Int> sorroundingPositionList = new List<Vector2Int>();
        int registerSpawnPositionX = gridItemPosition.GetX();
        int registerSpawnPositionY = gridItemPosition.GetY();

        sorroundingPositionList.Add(new Vector2Int(registerSpawnPositionX - 1, registerSpawnPositionY));
        sorroundingPositionList.Add(new Vector2Int(registerSpawnPositionX, registerSpawnPositionY));
        sorroundingPositionList.Add(new Vector2Int(registerSpawnPositionX + 1, registerSpawnPositionY));
        return sorroundingPositionList; 
    }
}
