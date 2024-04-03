using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWheel
{
    private int x;
    private int y;
    private Grid<GridItemPosition> grid;
    private GridLogic gridLogic;

    GridItemPosition itemPosition0;
    GridItemPosition itemPosition1;
    GridItemPosition itemPosition2;
    GridItemPosition itemPosition3;
    GridItemPosition itemPosition4;
    GridItemPosition itemPosition5;
    GridItemPosition itemPosition6;
    GridItemPosition itemPosition7;



    public GridWheel(int x, int y, Grid<GridItemPosition> grid, GridLogic gridLogic)
    {
        this.x = x;
        this.y = y;
        this.grid = grid;
        this.gridLogic = gridLogic;
        SetGridItemPositionList(); 
    }

    private List<Vector2Int> GetWheelPositionList()
    {
        List<Vector2Int> possibleWheelGridPositionList = new List<Vector2Int>();

        possibleWheelGridPositionList.Add(new Vector2Int(x - 1, y + 1));
        possibleWheelGridPositionList.Add(new Vector2Int(x - 1, y));
        possibleWheelGridPositionList.Add(new Vector2Int(x - 1, y - 1));
        possibleWheelGridPositionList.Add(new Vector2Int(x, y - 1));
        possibleWheelGridPositionList.Add(new Vector2Int(x + 1, y - 1));
        possibleWheelGridPositionList.Add(new Vector2Int(x + 1, y));
        possibleWheelGridPositionList.Add(new Vector2Int(x + 1, y + 1));
        possibleWheelGridPositionList.Add(new Vector2Int(x, y + 1));

        return possibleWheelGridPositionList; 
    }

    private void SetGridItemPositionList()
    {
        List<Vector2Int> positionList = GetWheelPositionList();
        if (positionList.Count < 1)
        {
            Debug.Log("No Wheel Found");
            return;
        }

        itemPosition0 = grid.GetGridObject(positionList[0].x, positionList[0].y);
        itemPosition1 = grid.GetGridObject(positionList[1].x, positionList[1].y);
        itemPosition2 = grid.GetGridObject(positionList[2].x, positionList[2].y);
        itemPosition3 = grid.GetGridObject(positionList[3].x, positionList[3].y);
        itemPosition4 = grid.GetGridObject(positionList[4].x, positionList[4].y);
        itemPosition5 = grid.GetGridObject(positionList[5].x, positionList[5].y);
        itemPosition6 = grid.GetGridObject(positionList[6].x, positionList[6].y);
        itemPosition7 = grid.GetGridObject(positionList[7].x, positionList[7].y);
    }

    public void SpinWheel()
    {
        GridItem gridItem0 = null;
        GridItem gridItem1 = null;
        GridItem gridItem2 = null;
        GridItem gridItem3 = null;
        GridItem gridItem4 = null;
        GridItem gridItem5 = null;
        GridItem gridItem6 = null;
        GridItem gridItem7 = null;

        if (itemPosition0.HasGridItem())
        {
            gridItem0 = itemPosition0.GetGridItem(); 
        }

        if (itemPosition1.HasGridItem())
        {
            gridItem1 = itemPosition1.GetGridItem();
        }

        if (itemPosition2.HasGridItem())
        {
            gridItem2 = itemPosition2.GetGridItem();
        }

        if (itemPosition3.HasGridItem())
        {
            gridItem3 = itemPosition3.GetGridItem();
        }

        if (itemPosition4.HasGridItem())
        {
            gridItem4 = itemPosition4.GetGridItem();
        }

        if (itemPosition5.HasGridItem())
        {
            gridItem5 = itemPosition5.GetGridItem();
        }

        if (itemPosition6.HasGridItem())
        {
            gridItem6 = itemPosition6.GetGridItem();
        }

        if (itemPosition7.HasGridItem())
        {
            gridItem7 = itemPosition7.GetGridItem();
        }


        if (gridItem0 != null)
        {
            gridItem0.SetItemXY(itemPosition1.GetX(), itemPosition1.GetY()); 
            itemPosition1.SetGridItem(gridItem0); 
        }
        if (gridItem1 != null)
        {
            gridItem1.SetItemXY(itemPosition2.GetX(), itemPosition2.GetY());
            itemPosition2.SetGridItem(gridItem1);
        }
        if (gridItem2 != null)
        {
            gridItem2.SetItemXY(itemPosition3.GetX(), itemPosition3.GetY());
            itemPosition3.SetGridItem(gridItem2);
        }
        if (gridItem3 != null)
        {
            gridItem3.SetItemXY(itemPosition4.GetX(), itemPosition4.GetY());
            itemPosition4.SetGridItem(gridItem3);
        }
        if (gridItem4 != null)
        {
            gridItem4.SetItemXY(itemPosition5.GetX(), itemPosition5.GetY());
            itemPosition5.SetGridItem(gridItem4);
        }
        if (gridItem5 != null)
        {
            gridItem5.SetItemXY(itemPosition6.GetX(), itemPosition6.GetY());
            itemPosition6.SetGridItem(gridItem5);
        }
        if (gridItem6 != null)
        {
            gridItem6.SetItemXY(itemPosition7.GetX(), itemPosition7.GetY());
            itemPosition7.SetGridItem(gridItem6);
        }
        if (gridItem7 != null)
        {
            gridItem7.SetItemXY(itemPosition0.GetX(), itemPosition0.GetY());
            itemPosition0.SetGridItem(gridItem7);
        }
    }
}
