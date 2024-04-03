using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MatchLink 
{
    public enum LinkedPositionLayer { None, Normal, Stripped, Wrapped, Power}

    private int x;
    private int y;

    private Grid<GridItemPosition> grid;
    private GridLogic gridLogic;
    private List<GridItemPosition> horizontalLink;
    private List<GridItemPosition> verticalLink;
    private LinkedPositionLayer linkLayer; 

    public MatchLink(int x, int y, Grid<GridItemPosition> grid, GridLogic gridLogic)
    {
        this.x = x;
        this.y = y;
        this.grid = grid;
        this.gridLogic = gridLogic;
        horizontalLink = GetHorizontalLink();
        verticalLink = GetVerticalLink();
        linkLayer = LinkedPositionLayer.None;
        UpdateLinkedPositionLayer(); 
    }

    public bool HasAnyLink()
    {
        UpdateLinkedPositionLayer(); 
        if (linkLayer == LinkedPositionLayer.None)
        {
            return false; 
        }
        return true; 
    }
    public List<GridItemPosition> GetLinkedGridItemPositionList()
    {
        //UpdateLinkedPositionLayer(); 
        switch (linkLayer)
        {
            case LinkedPositionLayer.Power:
                return GetPowerLink(); 
            case LinkedPositionLayer.Wrapped:
                return GetWrappedLink(); 
            case LinkedPositionLayer.Stripped:
                return GetStrippedLink(); 
            case LinkedPositionLayer.Normal:
                return GetNormalLink(); 
            case LinkedPositionLayer.None:
                return null; 
        }
        return null; 
    }
    public LinkedPositionLayer GetLinkedPositionLayer() { return linkLayer; }

    private void UpdateLinkedPositionLayer()
    {
        HasPowerLink();
        HasDoubleLink();
        HasStrippedLink();
        HasNormalLink(); 
    }

    public List<GridItemPosition> GetPowerLink()
    {
        if (!HasPowerLink()) return null;
        if (horizontalLink != null && horizontalLink.Count >= 5)
        {
            return horizontalLink;
        }

        return verticalLink; 
    }

    private bool HasPowerLink()
    {
        if ((horizontalLink != null && horizontalLink.Count >= 5) || (verticalLink != null && verticalLink.Count >= 5))
        {
            linkLayer = LinkedPositionLayer.Power; 
            return true; 
        }
        return false; 
    }

    public List<GridItemPosition> GetWrappedLink()
    {
        if (!HasDoubleLink()) return null;
        List<GridItemPosition> wrappedLink = new List<GridItemPosition>();
        foreach (GridItemPosition item in horizontalLink)
        {
            wrappedLink.Add(item); 
        }

        foreach (GridItemPosition item in verticalLink)
        {
            if (!wrappedLink.Contains(item))
            {
                wrappedLink.Add(item); 
            }
        }
        return wrappedLink; 
    }

    public GridItemPosition GetWrappedSpawnGridPosition()
    {
        if (!HasDoubleLink()) return null;

        foreach (GridItemPosition horizontalItem in horizontalLink)
        {
            foreach (GridItemPosition verticalItem in verticalLink)
            {
                if (horizontalItem == verticalItem)
                {
                    return horizontalItem; 
                }
            }
        }
        return null; 
    }

    private bool HasDoubleLink()
    {
        if (!HasPowerLink() && horizontalLink != null && verticalLink != null)
        {
            linkLayer = LinkedPositionLayer.Wrapped; 
            return true; 
        }
        return false; 
    }

    public List<GridItemPosition> GetStrippedLink()
    {
        if (!HasStrippedLink()) return null;
        if (horizontalLink != null)
        {
            return horizontalLink; 
        }
        return verticalLink; 
    }

    private bool HasStrippedLink()
    {
        if (HasDoubleLink() || HasPowerLink()) return false;
        if ((horizontalLink != null && horizontalLink.Count == 4) || (verticalLink != null && verticalLink.Count == 4)) 
        {
            linkLayer = LinkedPositionLayer.Stripped; 
            return true;
        } 
        return false; 
    }

    public List<GridItemPosition> GetNormalLink()
    {
        if (!HasNormalLink()) return null; 
        if(horizontalLink != null)
        {
            return horizontalLink; 
        }
        return verticalLink; 
    }

    private bool HasNormalLink()
    {
        if (HasPowerLink() || HasDoubleLink() || HasStrippedLink()) return false;
        if (horizontalLink != null || verticalLink != null)
        {
            linkLayer = LinkedPositionLayer.Normal; 
            return true;
        } 
        return false; 
    }

    private List<GridItemPosition> GetHorizontalLink()
    {
        // Cheeklist 
        if (!gridLogic.IsValidPosition(x, y)) return null;

        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (!gridLogic.MatchLinkCheeklist(gridItemPosition)) return null;

        int rightLinkAmount = 0;
        for (int i = 1; i < grid.GetWidth(); i++)
        {
            if (gridLogic.IsValidPosition(x + i, y))
            {
                GridItemPosition nextGridItemPosition = grid.GetGridObject(x + i, y);
                if (gridLogic.MatchLinkCheeklist(nextGridItemPosition))
                {
                    if (nextGridItemPosition.GetGridItem().GetItem().itemID == gridItemPosition.GetGridItem().GetItem().itemID)
                    {
                        rightLinkAmount++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break; 
                }            
            }
            else
            {
                break;
            }
        }

        int leftLinkAmount = 0;
        for (int i = 1; i < grid.GetWidth(); i++)
        {
            if (gridLogic.IsValidPosition(x - i, y))
            {
                GridItemPosition nextGridItemPosition = grid.GetGridObject(x - i, y);
                if (gridLogic.MatchLinkCheeklist(nextGridItemPosition))
                {
                    if (nextGridItemPosition.GetGridItem().GetItem().itemID == gridItemPosition.GetGridItem().GetItem().itemID)
                    {
                        leftLinkAmount++; 
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break; 
                }
            }
            else
            {
                break; 
            }
        }

        int horizontalLinkAmount = 1 + rightLinkAmount + leftLinkAmount;
        if (horizontalLinkAmount >= 3)
        {
            List<GridItemPosition> horizontalLinkedGridItemPositionList = new List<GridItemPosition>();
            int leftMostX = x - leftLinkAmount;
            for (int i = 0; i < horizontalLinkAmount; i++)
            {
                GridItemPosition gridPosition = grid.GetGridObject(leftMostX + i, y);
                horizontalLinkedGridItemPositionList.Add(gridPosition);
            }
            return horizontalLinkedGridItemPositionList;
        }

        return null; 
    }

    private List<GridItemPosition> GetVerticalLink()
    {
        // Cheeklist 
        if (!gridLogic.IsValidPosition(x, y)) return null;

        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (!gridLogic.MatchLinkCheeklist(gridItemPosition)) return null;

        int downLinkAmount = 0;
        for (int i = 1; i < grid.GetHeight(); i++)
        {
            if (gridLogic.IsValidPosition(x, y - i))
            {
                GridItemPosition nextGridItemPosition = grid.GetGridObject(x, y - i); 
                if (gridLogic.MatchLinkCheeklist(nextGridItemPosition))
                {
                    if (nextGridItemPosition.GetGridItem().GetItem().itemID == gridItemPosition.GetGridItem().GetItem().itemID)
                    {
                        downLinkAmount++;
                    }
                    else
                    {
                        break; 
                    }
                }
                else
                {
                    break; 
                }
            }
            else
            {
                break; 
            }
        }

        int upLinkAmount = 0;
        for (int i = 1; i < grid.GetHeight(); i++)
        {
            if (gridLogic.IsValidPosition(x, y + i))
            {
                GridItemPosition nextGridItemPosition = grid.GetGridObject(x, y + i);
                if (gridLogic.MatchLinkCheeklist(nextGridItemPosition))
                {
                    if (nextGridItemPosition.GetGridItem().GetItem().itemID == gridItemPosition.GetGridItem().GetItem().itemID)
                    {
                        upLinkAmount++; 
                    }
                    else
                    {
                        break; 
                    }
                }
                else
                {
                    break; 
                }
            }
            else
            {
                break; 
            }
        }

        int verticalLinkAmount = 1 + downLinkAmount + upLinkAmount;
        if (verticalLinkAmount >= 3)
        {
            List<GridItemPosition> verticalLinkedGridItemPositionList = new List<GridItemPosition>();
            int mostDownY = y - downLinkAmount;
            for (int i = 0; i < verticalLinkAmount; i++)
            {
                GridItemPosition gridPosition = grid.GetGridObject(x, mostDownY + i);
                verticalLinkedGridItemPositionList.Add(gridPosition);
            }
            return verticalLinkedGridItemPositionList;
        }
        return null; 
    }

    public bool MatchLinkCheeklist(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.IsEmpty() || gridItemPosition.IsGridPositionInsulator() || gridItemPosition.IsRegister() 
            || gridItemPosition.IsPowerBooster() || gridItemPosition.GetHasBlocker() || gridItemPosition.HasCell()) return false;
        return true; 
    }
}
