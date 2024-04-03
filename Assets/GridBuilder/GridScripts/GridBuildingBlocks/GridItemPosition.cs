using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class GridItemPosition 
{
    public event EventHandler OnGlassDestroyed; 
    private GridItem gridItem; 
    private Grid<GridItemPosition> grid; 
    private int x;
    private int y;
    private bool enabled;
    private bool hasGlass;
    private bool hasWheel;

    public GridItemPosition(Grid<GridItemPosition> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetGridItem(GridItem gridItem)
    {
        this.gridItem = gridItem;
        grid.TriggerGridObjectChanged(x, y); 
    }

    public Vector3 GetWorldPosition() { return grid.GetWorldPosition(x, y); }

    public GridItem GetGridItem()
    {
        return gridItem; 
    }

    public void SetEnabled(bool isEnabled)
    {
        enabled = isEnabled; 
    }

    public bool GetEnabled() { return enabled; }

    public int GetX() { return x; }
    public int GetY() { return y; }

    public bool IsGridPositionInsulator()
    {
        return gridItem.GetItem().isInsulator; 
    }

    public bool IsEmpty() { return gridItem == null; }

    public bool HasGridItem() { return gridItem != null; }

    public void ClearGridItem() { gridItem = null; }

    public bool HasGlass() { return hasGlass; }

    public void SetHasGlass(bool hasGlass) 
    {
        this.hasGlass = hasGlass; 
    }

    public bool HasBooster()
    {
        return gridItem.IsBooster(); 
    }

    public void DestroyGlass()
    {
        SetHasGlass(false);
        OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public bool IsRegister() 
    {
        if (gridItem == null) return false; 
        return gridItem.IsRegister(); 
    }

    public bool IsPowerBooster()
    {
        if (gridItem.GetBoosterID() == 3)
        {
            return true; 
        }
        return false; 
    }

    public bool GetHasWheel() { return hasWheel; }
    public void SetWheel(bool hasWheel)
    {
        this.hasWheel = hasWheel; 
    }

    public bool GetHasBlocker() { return gridItem.GetIsBlocker(); }
    public bool HasCell() { return gridItem.GetHasCell(); }
}
