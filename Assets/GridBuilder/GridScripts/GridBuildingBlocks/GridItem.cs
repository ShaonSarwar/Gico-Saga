using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;


/* All normal item boosterID will be 0 
     * Stripped Booster ID is 1 
     * Wrapped Booster ID is 2 
     * Power Booster ID is 3 
     * SO ON...... 
     */

public class GridItem 
{
    public enum BlockerType { None, levelOne, levelTwo} 
    public enum CellProtectionLayer {None, levelOne, levelTwo} 
    private Grid<GridItemPosition> grid;
    private int x;
    private int y;
    private ItemSO itemSO;
    private int boosterID = 0; 
    private bool isBooster = false;
    private TextMeshProUGUI remainingLifeText;
    private bool isRegister;
    private BlockerType blockerType;
    private bool isBlocker;
    private bool hasCell;
    private CellProtectionLayer cellProtectionLayer; 

    public GridItem( Grid<GridItemPosition> grid, int x, int y, ItemSO itemSO)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.itemSO = itemSO;
        isBlocker = false; 
    }

    public Vector3 GetWorldPosition()
    {
        return grid.GetWorldPosition(x, y); 
    }

    public ItemSO GetItem()
    {
        return itemSO; 
    }

    public void SetItemXY(int x, int y)
    {
        this.x = x;
        this.y = y; 
    }

    public void SetBoosterID(int boosterID)
    {
        this.boosterID = boosterID;    
    }

    public int GetBoosterID() { return boosterID; }

    public bool IsBooster() 
    {
        if (boosterID > 0)
        {
            isBooster = true;
        }
        else
        {
            isBooster = false; 
        }
        return isBooster; 
    }

    public int GetX() { return x; }

    public int GetY() { return y; }

    public bool IsInsulator() { return itemSO.isInsulator; }

    public void SetInsulatorLifeText(TextMeshProUGUI remainingLifeText)
    {
        this.remainingLifeText = remainingLifeText; 
    }

    public TextMeshProUGUI GetInsulatorLifeText() { return remainingLifeText; }

    public bool IsRegister() { return isRegister; }

    public void SetRegister(bool isRegister) { this.isRegister = isRegister; }

    public bool GetIsBlocker() { return isBlocker; }
    public BlockerType GetBlockerType() 
    {
        if (!GetIsBlocker()) return BlockerType.None;
        return blockerType; 
    }
    public void SetIsBlocker(bool isBlocker, BlockerType blockerType)
    {
        this.isBlocker = isBlocker;
        this.blockerType = blockerType; 
    }
    public bool GetHasCell() { return hasCell; }

    public CellProtectionLayer GetCellProtectionLayer() 
    {
        if (!GetHasCell()) return CellProtectionLayer.None; 
        return cellProtectionLayer; 
    }
    public void SetHasCell(bool hasCell, CellProtectionLayer cellProtectionLayer)
    {
        this.hasCell = hasCell;
        this.cellProtectionLayer = cellProtectionLayer; 
    }
}
