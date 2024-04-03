using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFall : MonoBehaviour
{
    private Grid<GridItemPosition> grid;
    private GridLogic gridLogic; 
    private int x;
    private int y;
    private int gridWidth; 
    private int gridHeight; 

    public ItemFall(Grid<GridItemPosition> grid, int x, int y, int gridWidth, int gridHeight)
    {
        this.grid = grid; 
        this.x = x;
        this.y = y;
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight; 
    }


}
