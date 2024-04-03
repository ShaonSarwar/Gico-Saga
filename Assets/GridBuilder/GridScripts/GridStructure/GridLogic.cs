using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement; 

public class GridLogic : MonoBehaviour
{
    [SerializeField] private UserData userData; 
    [SerializeField] private float cellSize;
    [SerializeField] private List<LevelSO> levelSOs;
    [SerializeField] private GameObject originObject;
    [SerializeField] private GameObject boardBG; 
    [SerializeField] private GridLogicVisual gridLogicVisual;
    [SerializeField] private InsulatorLogic insulatorLogic;
    [SerializeField] private BoosterController boosterController; 

    private Vector3 originalPosition;
    private Grid<GridItemPosition> grid;
    private int gridWidth;
    private int gridHeight;
    private int moveCount;
    private LevelSO levelSO;
    private bool levelWin; 

    // user Swap Positions 
    private int startVisualX;
    private int startVisualY;
    private int endVisualX;
    private int endVisualY;

    // Score 
    private int levelScore;
    // Level Target
    private int remainingCellToCollect;
    private int targetedCellCountToCollect; 
    
    // Cached the level index from PlayerPrefs 
    private int levelIndex;

    // Selected GridPosition for boosters which previously add to the level 
    public List<GridItemPosition> blockedGridForSelectedBooster = new List<GridItemPosition>(); 

    // Events 
    public event EventHandler<OnLevelSetEventArgs> OnLevelSet; 
    public class OnLevelSetEventArgs : EventArgs
    {
        public LevelSO levelSO;
        public Grid<GridItemPosition> grid; 
    }

    public event EventHandler<OnBoosterCreatedEventArgs> OnBoosterCreated; 
    public event EventHandler<NewlySpawnedItemEventArgs> OnNewlyReplacedItem; 
    public event EventHandler<NewlySpawnedItemEventArgs> OnNewlySpawnedItem; 
    public class NewlySpawnedItemEventArgs: EventArgs
    {
        public GridItem gridItem;
        public GridItemPosition gridItemPosition; 
    }

    public class OnBoosterCreatedEventArgs: EventArgs
    {
        public GridItem gridItem;
        public GridItemPosition gridItemPosition;
        public Grid<GridItemPosition> grid; 
    }

    //public event EventHandler OnElectricSplash;

    public event EventHandler<OnCreateBoosterInstanceEventArgs> OnCreateBoosterInstance;
    public class OnCreateBoosterInstanceEventArgs: EventArgs
    {
        public int itemID;
        public int boosterID; 
    }

    public event EventHandler<OnReplaceItemWithBonusBoosterEventArgs> OnReplaceItemWithBonusBooster;
    public event EventHandler<OnReplaceItemWithBonusBoosterEventArgs> OnReplaceItemWithStripedBooster;
    public event EventHandler<OnReplaceItemWithBonusBoosterEventArgs> OnReplaceItemWithWrappedBooster;
    public event EventHandler<OnReplaceItemWithBonusBoosterEventArgs> OnReplaceItemWithPowerBooster; 
    public class OnReplaceItemWithBonusBoosterEventArgs : EventArgs
    {
        public Grid<GridItemPosition> grid; 
        public GridItemPosition gridItemPosition;
        public GridItem gridItem;
        public int boosterID; 
    }
    public event EventHandler OnGridItemDestroyed;
    public event EventHandler OnRegisterItemDestroyed;
    public event EventHandler OnInsulatorDestroyed;
    public event EventHandler OnLevelOneBlockerDestroyed;
    public event EventHandler OnLevelTwoBlockerDestroyed;
    public event EventHandler OnTwoLayerCellDestroyed;
    public event EventHandler OnOneLayerCellDestroyed; 
    public event EventHandler<NewlySpawnedItemEventArgs> OnGridItemReplaced;
    public event EventHandler OnOutOfMoves;
    public event EventHandler OnMoveUsed;
    public event EventHandler OnScoreChanged; 
   

    private void Awake()
    {
        userData.OnUserDataInitialize += UserData_OnUserDataInitialize;
    }

    private void UserData_OnUserDataInitialize(object sender, EventArgs e)
    {
        Debug.Log("Data Initialize");
        levelIndex = PlayerPrefs.GetInt("PlayerLevel"); 
        levelSO = levelSOs[levelIndex - 1];
        SetLevel();
    }

    private void SetLevel()
    {
        gridWidth = levelSO.width;
        gridHeight = levelSO.height;
        int levelWeight = LevelWeight(levelSO.levelGridPositionList);
        int levelHeight = LevelHeight(levelSO.levelGridPositionList);
        //originObject.transform.localScale = new Vector3(levelWeight, levelHeight);
       
        GameObject go = new GameObject("Origin"); 
        go.transform.parent = originObject.transform;
        go.transform.localPosition = new Vector3(-0.5f, -0.5f);

        originalPosition = go.transform.position;

        grid = new Grid<GridItemPosition>(gridWidth, gridHeight, cellSize, null, originalPosition, (Grid<GridItemPosition> g, int x, int y) => new GridItemPosition(g, x, y));

        gridLogicVisual.OnSwapPositions += GridLogicVisual_OnSwapPositions;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                LevelSO.LevelGridPosition levelGridPosition = null;
                foreach (LevelSO.LevelGridPosition tmpLevelGridPosition in levelSO.levelGridPositionList)
                {
                    if (tmpLevelGridPosition.x == x && tmpLevelGridPosition.y == y)
                    {
                        levelGridPosition = tmpLevelGridPosition;
                        if (levelGridPosition != null)
                        {
                            ItemSO itemSO = levelGridPosition.itemSO;
                            GridItem gridItem = new GridItem(grid, x, y, itemSO);
                            gridItem.SetIsBlocker(levelGridPosition.isBlocker, levelGridPosition.blockerType);
                            gridItem.SetHasCell(levelGridPosition.hasCell, levelGridPosition.cellProtectionLayer); 

                            grid.GetGridObject(x, y).SetGridItem(gridItem);
                            grid.GetGridObject(x, y).SetEnabled(true);
                            grid.GetGridObject(x, y).SetWheel(levelGridPosition.hasWheel);  
                            break;
                        }
                    }
                }
            }
        }

        ResetBG();

        targetedCellCountToCollect = levelSO.targetCellCount;
        remainingCellToCollect = targetedCellCountToCollect; 
        moveCount = levelSO.moveAmount;
        levelScore = 0; 
        OnLevelSet?.Invoke(this, new OnLevelSetEventArgs { levelSO = levelSO, grid = grid });
    }

    public void ResetBG()
    {
        List<int> widths = new List<int>();
        List<int> heights = new List<int>();

        foreach (var item in levelSO.levelGridPositionList)
        {
            widths.Add(item.x);
            heights.Add(item.y);
        }

        if (widths.Any() && heights.Any())
        {
            int bgWidth = widths.Distinct().Count();
            int bgHeight = heights.Distinct().Count();

            int minX = widths.Min();
            int maxX = widths.Max();

            int minY = heights.Min();
            int maxY = heights.Max();

            int sumX = minX + maxX;
            int sumY = minY + maxY;

            int meanX, meanY;
            float offsetX, offsetY;

            if (sumX % 2 != 0)
            {
                sumX = sumX + 1;
                meanX = sumX / 2;
                offsetX = 0.0f;
            }
            else
            {
                meanX = sumX / 2;
                offsetX = 0.5f;
            }

            if (sumY % 2 != 0)
            {
                sumY = sumY + 1;
                meanY = sumY / 2;
                offsetY = 0.0f;
            }
            else
            {
                meanY = sumY / 2;
                offsetY = 0.5f;
            }

            Debug.Log($"MeanX: {meanX} , MeanY: {meanY}");
            if (IsValidPosition(meanX, meanY))
            {
                boardBG.transform.position = grid.GetWorldPosition(meanX, meanY) + new Vector3(offsetX, offsetY, 0);
                boardBG.transform.localScale = new Vector3(bgWidth + 1, bgHeight + 1, 1);
            }
            else
            {
                Debug.Log("Inavlid Position");
            }
        }
        else
        {
            Debug.Log("Lists are empty");
        }
       
    }

    private int LevelWeight(List<LevelSO.LevelGridPosition> levelGridPositions)
    {
        int levelWeight = 0; 

        foreach (var item in levelGridPositions)
        {
            if (item.x > levelWeight)
            {
                levelWeight = item.x; 
            }
        }
        return levelWeight; 
    }

    private int LevelHeight(List<LevelSO.LevelGridPosition> levelGridPositions)
    {
        int levelHeight = 0;

        foreach (var item in levelGridPositions)
        {
            if (item.y > levelHeight)
            {
                levelHeight = item.y; 
            }
        }
        return levelHeight; 
    }
    private void GridLogicVisual_OnSwapPositions(object sender, GridLogicVisual.OnSwapPositionsEventArgs e)
    {
        startVisualX = e.startX;
        startVisualY = e.startY;
        endVisualX = e.endX;
        endVisualY = e.endY;
        //insulatorLogic.BuildRegister();
    }

    public bool CanSwapGridItems(int startX, int startY, int endX, int endY)
    {
        if (!SwapItemsCommonCheeklist(startX, startY, endX, endY)) return false; 
        SwapGridItems(startX, startY, endX, endY); // Swap (Not visually but Programmatically)
        bool hasLinkAfterSwap = HasMatchLink(startX, startY) || HasMatchLink(endX, endY); // Cheecking for find a match after swap, if didn't find a match Items can't be swap 
        SwapGridItems(startX, startY, endX, endY); // Swap back(Not visually but Programmatically)
        return hasLinkAfterSwap; 
    }

    public bool CanSwapGridItemsByTacticalBooster(int startX, int startY, int endX, int endY)
    {
        if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return false;                           // Invalid Position 
        if (startX == endX && startY == endY) return false;                                                           // Same Position 
        if (IsGridItemPositionInsulator(startX, startY) || IsGridItemPositionInsulator(endX, endY)) return false;     // Matching with Insulator isn't allow 
        if (grid.GetGridObject(startX, startY).IsEmpty() || grid.GetGridObject(endX, endY).IsEmpty()) return false;   // Blank Grid Position 
        return true; 
    }

    public bool CanSwapGameplayBooster(int startX, int startY, int endX, int endY)
    {
        if (!SwapItemsCommonCheeklist(startX, startY, endX, endY)) return false;  

        GridItemPosition startGrid = grid.GetGridObject(startX, startY);
        GridItemPosition endGrid = grid.GetGridObject(endX, endY); 

        int startGridBoosterID = startGrid.GetGridItem().GetBoosterID();
        int endGridBoosterID = endGrid.GetGridItem().GetBoosterID(); 

        if ((startGridBoosterID > 0 && startGridBoosterID < 3) && (endGridBoosterID > 0 && endGridBoosterID < 3)) return true;  // Both are Stripped or Wrapped Booster 

        if ((startGridBoosterID == 3 && endGrid.HasGridItem()) || (startGrid.HasGridItem() && endGridBoosterID == 3)) return true;  // Any Item with Power Booster 

        return false; 
    }

    public void GetGridXY(Vector3 position, out int x, out int y)
    {
        grid.GetXY(position, out x, out y); 
    }

    public Vector3 GetGridPositionXY(int x, int y)
    {
        return grid.GetGridObject(x, y).GetWorldPosition(); 
    }

    public GridItemPosition GetGridObject(int x, int y)
    {
        return grid.GetGridObject(x, y); 
    }

    public void SwapGridItems(int startX, int startY, int endX, int endY)
    {
        if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return;                                // Invalid Position 

        if (startX == endX && startY == endY) return;                                                                // Same Position

        if (IsGridItemPositionInsulator(startX, startY) || IsGridItemPositionInsulator(endX, endY)) return;          // Swaping with Insulator isn't allow 

        GridItemPosition startGridItemPosition = grid.GetGridObject(startX, startY);
        GridItemPosition endGridItemPosition = grid.GetGridObject(endX, endY);

        GridItem startGridItem = startGridItemPosition.GetGridItem();
        GridItem endGridItem = endGridItemPosition.GetGridItem();

        startGridItem.SetItemXY(endX, endY);
        endGridItem.SetItemXY(startX, startY);

        startGridItemPosition.SetGridItem(endGridItem);
        endGridItemPosition.SetGridItem(startGridItem);
    }

    public bool HasMatchLink(int x, int y)
    {
        MatchLink matchLink = new MatchLink(x, y, grid, this);
        return matchLink.HasAnyLink(); 
    }

    public List<MatchLink> GetAllMatchLinks()
    {
        List<MatchLink> allMatchLink = new List<MatchLink>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (HasMatchLink(x, y))
                {
                    MatchLink matchLink = new MatchLink(x, y, grid, this);
                    if (allMatchLink.Count == 0)
                    {
                        allMatchLink.Add(matchLink);
                    }
                    else
                    {
                        bool newLink = true;
                        List<MatchLink> linkToRemove = new List<MatchLink>();
                        foreach (MatchLink link in allMatchLink)
                        {
                            // is another link already contain this link 
                            bool sharedItemFound = matchLink.GetLinkedGridItemPositionList().Any(s => link.GetLinkedGridItemPositionList().Contains(s));
                            bool superiorLinkFound = true; 
                            if (sharedItemFound)
                            {
                                if ((int)link.GetLinkedPositionLayer() >= (int)matchLink.GetLinkedPositionLayer())
                                {
                                    superiorLinkFound = false; 
                                }
                            }

                            if (superiorLinkFound)
                            {
                                foreach (MatchLink prelink in allMatchLink)
                                {
                                    // is previous link contain in new link 
                                    bool previousLinkContainInNew = prelink.GetLinkedGridItemPositionList().Any(s => matchLink.GetLinkedGridItemPositionList().Contains(s));
                                    if (previousLinkContainInNew)
                                    {
                                        if (!linkToRemove.Contains(prelink))
                                        {
                                            linkToRemove.Add(prelink);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                newLink = false; 
                            }                                                   
                        }
                        if (newLink)
                        {
                            allMatchLink.Add(matchLink);                       
                            if (linkToRemove.Count > 0)
                            {
                                foreach (MatchLink removeLink in linkToRemove)
                                {
                                    allMatchLink.Remove(removeLink); 
                                }
                            }
                        }
                    }
                }
            }
        }
        return allMatchLink; 
    }

    public bool TryFindMatchLinkAndDestroyThem()
    {
        bool foundLink = false;
        List<MatchLink> allMatchLinks = GetAllMatchLinks();
        if (allMatchLinks.Count < 1) return false;  
        foreach (MatchLink matchLink in allMatchLinks)
        {
            List<GridItemPosition> linkedGridItemPositionList = matchLink.GetLinkedGridItemPositionList(); 
            int x = -1, y = -1, id = -1;
            bool canStrippedBoosterCreate = false;
            bool canWrappedBoosterCreate = false; 
            bool canPowerBoosterCreate = false;           
            switch (matchLink.GetLinkedPositionLayer())
            {
                case MatchLink.LinkedPositionLayer.Power:
                    GridItemPosition powerBooster = linkedGridItemPositionList[2]; 
                    if (powerBooster != null)
                    {
                        x = powerBooster.GetX();
                        y = powerBooster.GetY();
                        id = 3;
                        canPowerBoosterCreate = true;
                    }
                    break;
                case MatchLink.LinkedPositionLayer.Wrapped:
                    GridItemPosition wrappedBooster = matchLink.GetWrappedSpawnGridPosition();                   
                    if (wrappedBooster != null)
                    {
                        x = wrappedBooster.GetX();
                        y = wrappedBooster.GetY();
                        id = wrappedBooster.GetGridItem().GetItem().itemID; 
                        canWrappedBoosterCreate = true; 
                    }
                    break;
                case MatchLink.LinkedPositionLayer.Stripped:
                    GridItemPosition startSwapGridPosition = grid.GetGridObject(startVisualX, startVisualY);
                    GridItemPosition endSwapGridPosition = grid.GetGridObject(endVisualX, endVisualY);
                    GridItemPosition boosterGridPosition = linkedGridItemPositionList[1]; 

                    if (startSwapGridPosition != null)
                    {
                        if (linkedGridItemPositionList.Any(gridPosition => (gridPosition.GetX() == startSwapGridPosition.GetX() && gridPosition.GetY() == startSwapGridPosition.GetY())) && startSwapGridPosition.HasGridItem())
                        {
                            x = startSwapGridPosition.GetX();
                            y = startSwapGridPosition.GetY();
                            id = startSwapGridPosition.GetGridItem().GetItem().itemID;
                            //Debug.Log("Booster Create at Start Position");
                            canStrippedBoosterCreate = true;

                        }
                        else if (linkedGridItemPositionList.Any(gridPosition => (gridPosition.GetX() == endSwapGridPosition.GetX() && gridPosition.GetY() == endSwapGridPosition.GetY())) && endSwapGridPosition.HasGridItem())
                        {
                            x = endSwapGridPosition.GetX();
                            y = endSwapGridPosition.GetY();
                            id = endSwapGridPosition.GetGridItem().GetItem().itemID;
                            //Debug.Log("Booster Create at End Position");
                            canStrippedBoosterCreate = true;

                        }
                        else
                        {
                            if (boosterGridPosition.HasGridItem())
                            {
                                x = boosterGridPosition.GetX();
                                y = boosterGridPosition.GetY();
                                id = boosterGridPosition.GetGridItem().GetItem().itemID;
                                //Debug.Log("Booster Create at Random Position");
                                canStrippedBoosterCreate = true;

                            }
                        }
                    }
                    break;
                case MatchLink.LinkedPositionLayer.Normal:
                    break;
                case MatchLink.LinkedPositionLayer.None:
                    break;
            }

            foreach (GridItemPosition gridItemPosition in linkedGridItemPositionList)
            {
                if (gridItemPosition.HasGridItem())
                {
                    if (gridItemPosition.HasBooster())
                    {
                        TryDestroyBoosterAfterDelay(gridItemPosition); 
                    }
                    else
                    {
                        TryDestroyGridItem(gridItemPosition);
                    }
                }
            }

            if (canStrippedBoosterCreate)
            {
                CreateStrippedBooster(x, y, id); 
            }

            if (canWrappedBoosterCreate)
            {
                CreateWrappedBooster(x, y, id); 
            }

            if (canPowerBoosterCreate)
            {
                CreatePowerBooster(x, y, id); 
            }
            foundLink = true; 
        }
        return foundLink; 
    }

    /* All normal item boosterID will be 0 
     * Stripped Booster ID is 1 
     * Wrapped Booster ID is 2 
     * Power Booster ID is 3 
     * SO ON...... 
     */
    #region Creating Booster 
    public void CreateStrippedBooster(int x, int y, int itemID)
    {      
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition != null && gridItemPosition.IsEmpty())
        {
            ItemSO boosterItem = levelSO.strippedBoosterList[itemID - 1];
            GridItem gridItem = new GridItem(grid, x, y, boosterItem);
            gridItem.SetBoosterID(1); 
            gridItemPosition.SetGridItem(gridItem);
            OnBoosterCreated?.Invoke(gridItem, new OnBoosterCreatedEventArgs { grid = grid, gridItem = gridItem, gridItemPosition = gridItemPosition }); 
        }
        //Debug.Log("Booster Created"); 
    }

    public void CreateWrappedBooster(int x, int y, int itemID)
    {
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition != null && gridItemPosition.IsEmpty())
        {
            ItemSO boosterItem = levelSO.wrappedBoosterList[itemID - 1];
            GridItem gridItem = new GridItem(grid, x, y, boosterItem);
            gridItem.SetBoosterID(2);
            gridItemPosition.SetGridItem(gridItem);
            OnBoosterCreated?.Invoke(gridItem, new OnBoosterCreatedEventArgs { grid = grid, gridItem = gridItem, gridItemPosition = gridItemPosition }); 
        }
    }

    public void CreatePowerBooster(int x, int y, int itemID)
    {
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition != null && gridItemPosition.IsEmpty())
        {
            ItemSO boosterItem = levelSO.powerBooster;
            GridItem gridItem = new GridItem(grid, x, y, boosterItem);
            gridItem.SetBoosterID(3);
            gridItemPosition.SetGridItem(gridItem);
            OnBoosterCreated?.Invoke(gridItem, new OnBoosterCreatedEventArgs { grid = grid, gridItem = gridItem, gridItemPosition = gridItemPosition }); 
        }
    }

    public void CreateBoosterInstance(int x, int y, int itemID, int boosterID)
    {
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            OnCreateBoosterInstance?.Invoke(gridItemPosition, new OnCreateBoosterInstanceEventArgs { itemID = itemID, boosterID = boosterID }); 
        }
    }

    #endregion Creating Booster

    #region Destroy System 
    public void TryDestroyGridItem(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition != null)
        {
            if (gridItemPosition.HasGridItem())
            {
                if (gridItemPosition.HasGlass())
                {
                    levelScore += 100;
                    OnScoreChanged?.Invoke(this, EventArgs.Empty); 
                    gridItemPosition.DestroyGlass();
                    return; 
                }

                List<Vector2Int> possibleBlockerGridList = new List<Vector2Int>();

                int blockerSearchOriginX = gridItemPosition.GetX();
                int blockerSearchOriginY = gridItemPosition.GetY();

                possibleBlockerGridList.Add(new Vector2Int(blockerSearchOriginX, blockerSearchOriginY - 1));
                possibleBlockerGridList.Add(new Vector2Int(blockerSearchOriginX, blockerSearchOriginY + 1));
                possibleBlockerGridList.Add(new Vector2Int(blockerSearchOriginX - 1, blockerSearchOriginY));
                possibleBlockerGridList.Add(new Vector2Int(blockerSearchOriginX + 1, blockerSearchOriginY));
             
                levelScore += 100;
                OnScoreChanged?.Invoke(this, EventArgs.Empty); 
                OnGridItemDestroyed?.Invoke(gridItemPosition, EventArgs.Empty);
                gridItemPosition.ClearGridItem();

                foreach (Vector2Int possibleBlocker in possibleBlockerGridList)
                {
                    if (IsValidPosition(possibleBlocker.x, possibleBlocker.y))
                    {
                        GridItemPosition possibleBlockerGrid = grid.GetGridObject(possibleBlocker.x, possibleBlocker.y);
                        if (possibleBlockerGrid.IsRegister())
                        {
                            TryDestroyRegister(possibleBlockerGrid);
                        }

                        if (possibleBlockerGrid.HasGridItem())
                        {
                            if (possibleBlockerGrid.GetHasBlocker())
                            {
                                TryDestroyBlocker(possibleBlockerGrid);
                            }
                            else if (possibleBlockerGrid.GetGridItem().GetCellProtectionLayer() == GridItem.CellProtectionLayer.levelOne)
                            {
                                TryDestroyCell(possibleBlockerGrid); 
                            }
                        }
                    }
                }
            }
        }
    }

    public void TryDestroyBlocker(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.HasGridItem())
        {
            if (gridItemPosition.HasGlass())
            {
                levelScore += 100;
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
                gridItemPosition.DestroyGlass();
                return;
            }

            if (gridItemPosition.GetGridItem().GetBlockerType() == GridItem.BlockerType.levelTwo)
            {
                OnLevelTwoBlockerDestroyed?.Invoke(gridItemPosition, EventArgs.Empty);
                return; 
            }

            if (gridItemPosition.GetGridItem().GetBlockerType() == GridItem.BlockerType.levelOne)
            {
                OnLevelOneBlockerDestroyed?.Invoke(gridItemPosition, EventArgs.Empty);
                gridItemPosition.ClearGridItem(); 
            }
        }
    }

    public void TryDestroyCell(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.HasGridItem())
        {
            if (gridItemPosition.HasGlass())
            {
                levelScore += 100;
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
                gridItemPosition.DestroyGlass();
                return;
            }

            if (gridItemPosition.GetGridItem().GetCellProtectionLayer() == GridItem.CellProtectionLayer.levelTwo)
            {
                Debug.Log("TryDestroyLevel2Cell"); 
                OnTwoLayerCellDestroyed?.Invoke(gridItemPosition, EventArgs.Empty);
                return; 
            }

            if (gridItemPosition.GetGridItem().GetCellProtectionLayer() == GridItem.CellProtectionLayer.levelOne)
            {
                Debug.Log("TryDestroyLevel2Cell");
                remainingCellToCollect -= 1; 
                //OnElectricSplash?.Invoke(gridItemPosition, EventArgs.Empty);
                OnOneLayerCellDestroyed?.Invoke(gridItemPosition, EventArgs.Empty);
                gridItemPosition.ClearGridItem(); 
            }
        }
    }

    public void TryDestroyRegister(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.HasGridItem())
        {
            OnRegisterItemDestroyed?.Invoke(gridItemPosition, EventArgs.Empty); 
            gridItemPosition.ClearGridItem(); 
        }
    }

    public void TryDestroyInsulator(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.HasGridItem())
        {
            levelScore += 1000;
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
            OnInsulatorDestroyed?.Invoke(gridItemPosition, EventArgs.Empty);
            gridItemPosition.ClearGridItem(); 
        }
    }

    // method is async because of avoiding recursion 
    public void TryDestroyBoosterAfterDelay(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.HasGridItem())
        {
            if (gridItemPosition.HasGlass())
            {
                levelScore += 100;
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
                gridItemPosition.DestroyGlass();
                return;
            }

            int id = gridItemPosition.GetGridItem().GetBoosterID();
            switch (id)
            {
                case 1:
                    boosterController.ActivateBooster(gridItemPosition.GetGridItem(), 1, null);
                    break;
                case 2:
                    boosterController.ActivateBooster(gridItemPosition.GetGridItem(), 2, null);
                    break; 
                case 3:
                    boosterController.ActivateBooster(gridItemPosition.GetGridItem(), 3, null);
                    break;
                default:
                    break;
            }
        }      
    }

    // Destroy Booster by Swap 
    public void TryDestroyBooster(int startX, int startY, int endX, int endY)
    {
        GridItemPosition startBoosterItemPosition = grid.GetGridObject(startX, startY);
        GridItemPosition endBoosterItemPosition = grid.GetGridObject(endX, endY);
        int startItemBoosterID = startBoosterItemPosition.GetGridItem().GetBoosterID();
        int endItemBoosterID = endBoosterItemPosition.GetGridItem().GetBoosterID(); 

        List<GridItemPosition> boosterList = new List<GridItemPosition>();
        boosterList.Add(startBoosterItemPosition);
        boosterList.Add(endBoosterItemPosition);

        if ((startItemBoosterID > 0 && startItemBoosterID < 3) && (endItemBoosterID > 0 && endItemBoosterID < 3))
        {
            boosterController.ActivateBooster(startBoosterItemPosition.GetGridItem(), startItemBoosterID, null);
            boosterController.ActivateBooster(endBoosterItemPosition.GetGridItem(), endItemBoosterID, null);
            return;
        }

        if (startItemBoosterID == 3 || endItemBoosterID == 3)
        {
            if (startItemBoosterID == 3)
            {
                boosterController.ActivateBooster(startBoosterItemPosition.GetGridItem(), startItemBoosterID, endBoosterItemPosition.GetGridItem());

            }
            else
            {
                boosterController.ActivateBooster(endBoosterItemPosition.GetGridItem(), endItemBoosterID, startBoosterItemPosition.GetGridItem()); 
            }
        }      
    }

    #endregion Destroy System 

    #region ReplaceItemsOrSuffle
    public void ReplaceAllGridItem()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (gridItemPosition.HasGridItem())
                    {
                        if (ItemSuffleChecklist(gridItemPosition))
                        {
                            TryReplaceGridItem(gridItemPosition);
                        }
                    }
                }
            }
        }
    }

    public void TryReplaceGridItem(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition != null)
        {
            if (gridItemPosition.HasGridItem())
            {
                OnGridItemReplaced?.Invoke(gridItemPosition, new NewlySpawnedItemEventArgs { gridItem = gridItemPosition.GetGridItem(), gridItemPosition = gridItemPosition });
                gridItemPosition.ClearGridItem(); 
            }
        }
    }
    #endregion ReplaceItemsOrSuffle

    #region Item Fall System 
  
    public void FallItemsIntoDirectEmptyPositions() 
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (ItemFallCheeklist(gridItemPosition))
                    {
                        for (int i = y - 1; i >= 0; i--)
                        {
                            if (IsValidPosition(x, i))
                            {
                                GridItemPosition newGridItemPosition = grid.GetGridObject(x, i);
                                if (newGridItemPosition.IsEmpty())
                                {
                                    gridItemPosition.GetGridItem().SetItemXY(newGridItemPosition.GetX(), newGridItemPosition.GetY());
                                    newGridItemPosition.SetGridItem(gridItemPosition.GetGridItem());
                                    gridItemPosition.ClearGridItem();
                                    gridItemPosition = newGridItemPosition;
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
                    }                  
                }
            }
        }
    }

    public void FallSingleItemInDirectPosition(int x, int y)
    {
        if (IsValidPosition(x, y) && !IsGridItemPositionInsulator(x, y))
        {
            GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
            if (gridItemPosition.HasGridItem())
            {
                for (int i = y - 1; i >= 0; i--)
                {
                    if (IsValidPosition(x, i) && !IsGridItemPositionInsulator(x, i))
                    {
                        GridItemPosition newGridItemPosition = grid.GetGridObject(x, i);
                        if (newGridItemPosition.IsEmpty())
                        {
                            gridItemPosition.GetGridItem().SetItemXY(newGridItemPosition.GetX(), newGridItemPosition.GetY());
                            newGridItemPosition.SetGridItem(gridItemPosition.GetGridItem());
                            gridItemPosition.ClearGridItem();
                            gridItemPosition = newGridItemPosition;                           
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
            }
        }
    }

    public bool CanSingleItemDirectFall(int x, int y)
    {
        bool canFall = false; 
        if (IsValidPosition(x, y))
        {
            GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
            if (gridItemPosition.HasGridItem())
            {
                for (int i = y - 1; i >= 0; i--)
                {
                    if (IsValidPosition(x, i))
                    {
                        GridItemPosition newGridItemPosition = grid.GetGridObject(x, i);
                        if (newGridItemPosition.IsEmpty())
                        {
                            canFall = true; 
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
            }
        }

        return canFall; 
    }

    public bool HaveItemForDirectFall()
    {
        bool hasItem = false; 
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (ItemFallCheeklist(gridItemPosition))
                    {
                        if (IsValidPosition(x, y - 1))
                        {
                            GridItemPosition newGridItemPosition = grid.GetGridObject(x, y - 1);
                            if (newGridItemPosition.IsEmpty())
                            {
                                hasItem = true;
                                return hasItem;
                            }
                        }
                    }                   
                }
            }
        }
        return hasItem; 
    }

    public void SideFall()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = gridHeight - 1; y >= 0; y--)
            {
                if (ItemFallCheeklist(x, y))
                {
                    if (CanSingleItemSideFall(x, y))
                    {
                        FallItemsIntoSideEmptyPositions(x, y);
                        return;
                    }
                }
            }           
        }

    }

    public void FallItemsIntoSideEmptyPositions(int i, int j)
    {
        if (!ItemFallCheeklist(i, j)) return; 

        // Find two possible side coloum empty positions (-1 down)

        if (CanItemFallRightSide(i, j))
        {
            FallItemsInRightSide(i, j);
            return; 
        }
        else if(CanItemFallLeftSide(i, j))
        {
            FallItemsInLeftSide(i, j);
            return;
        }
    }

    public void FallItemsInRightSide(int x, int y)
    {
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        GridItemPosition rightSideDownGridPosition = null;

        if (IsValidPosition(x + 1, y - 1))
        {
            rightSideDownGridPosition = grid.GetGridObject(x + 1, y - 1);
            if (gridItemPosition.HasGridItem() && rightSideDownGridPosition != null && rightSideDownGridPosition.IsEmpty())
            {
                MoveItemToEmptyPosition(gridItemPosition, rightSideDownGridPosition);
            }         
        }
    }

    public void FallItemsInLeftSide(int x, int y)
    {
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        GridItemPosition leftSideDownGridPosition = null;

        if (IsValidPosition(x - 1, y - 1))
        {
            leftSideDownGridPosition = grid.GetGridObject(x - 1, y - 1);
            if (gridItemPosition.HasGridItem() && leftSideDownGridPosition != null && leftSideDownGridPosition.IsEmpty())
            {
                MoveItemToEmptyPosition(gridItemPosition, leftSideDownGridPosition);
            }          
        }
    }

    public void MoveItemToEmptyPosition(GridItemPosition gridItemPosition, GridItemPosition newGridItemPosition)
    {
        gridItemPosition.GetGridItem().SetItemXY(newGridItemPosition.GetX(), newGridItemPosition.GetY());
        newGridItemPosition.SetGridItem(gridItemPosition.GetGridItem());
        gridItemPosition.ClearGridItem();
        gridItemPosition = newGridItemPosition;
    }

    public bool CanItemSideFall()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = gridHeight - 1; y >= 0; y--)
            {
                if (ItemFallCheeklist(x, y))
                {
                    if (CanItemFallRightSide(x, y) || CanItemFallLeftSide(x, y))
                    {
                        return true; 
                    }
                }
            }
        }

        return false; 
    }

    public bool CanSingleItemSideFall(int x, int y)
    {
        if (ItemFallCheeklist(x, y))
        {
            if (CanItemFallRightSide(x, y) || CanItemFallLeftSide(x, y))
            {
                return true;
            }
        }

        return false; 
    }

    public bool CanItemFallRightSide(int x, int y)
    {
        bool canFall = false; 
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);

        if (IsValidPosition(x + 1, y - 1))
        {
            GridItemPosition nextGridPosition = grid.GetGridObject(x + 1, y - 1);
            if (gridItemPosition.HasGridItem() && nextGridPosition.IsEmpty())
            {
                canFall = true;
                return canFall;
            }
        }

        return canFall; 
    }

    public bool CanItemFallLeftSide(int x, int y)
    {
        bool canFall = false;
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);

        if (IsValidPosition(x - 1, y - 1))
        {
            GridItemPosition nextGridPosition = grid.GetGridObject(x - 1, y - 1);
            if (gridItemPosition.HasGridItem() && nextGridPosition.IsEmpty())
            {
                canFall = true;
                return canFall;
            }
        }

        return canFall; 
    }

    #endregion Item Fall System

    #region Item Spawn System 
    public void SpawnItemsInMissingPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            // All coloum calculations start here 
            if (IsColoumContainsAnyBlockage(x, gridHeight))
            {
                int blockageColoumIndex = HigestBlockageGridColoumIndex(x, gridHeight);

                for (int i = blockageColoumIndex + 1; i < gridHeight; i++)
                {
                    if (IsValidPosition(x, i) && grid.GetGridObject(x, i).IsEmpty())
                    {
                        SpawnItemInSingleGridPosition(x, i);
                    }
                }
            }
            else
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (IsValidPosition(x, y) && grid.GetGridObject(x, y).IsEmpty())
                    {
                        SpawnItemInSingleGridPosition(x, y);
                    }
                }
            }
        }
    }

    public bool HaveAnyGridPositionForItemSpawn()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            // All coloum calculations start here 
            if (IsColoumContainsAnyBlockage(x, gridHeight))
            {
                int blockageColoumIndex = HigestBlockageGridColoumIndex(x, gridHeight);

                for (int i = gridHeight - 1; i >= blockageColoumIndex + 1; i--)
                {
                    if (IsValidPosition(x, i) && grid.GetGridObject(x, i).IsEmpty())
                    {
                        return true; 
                    }
                }
            }
            else
            {
                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    if (IsValidPosition(x, y) && grid.GetGridObject(x, y).IsEmpty())
                    {
                        return true; 
                    }
                }
            }
        }

        return false; 
    }

    public void SpawnItemInSingleGridPosition(int x, int y)
    {
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition.IsEmpty())
        {
            ItemSO itemSO = levelSO.itemList[UnityEngine.Random.Range(0, levelSO.itemList.Count)];
            GridItem gridItem = new GridItem(grid, x, y, itemSO);
            gridItemPosition.SetGridItem(gridItem);
            OnNewlySpawnedItem?.Invoke(gridItem, new NewlySpawnedItemEventArgs { gridItem = gridItem, gridItemPosition = gridItemPosition });
           
        }
    }

    public int HigestBlockageGridColoumIndex(int rowIndex, int coloumHeight)
    {
        int index = -1; 
        GridItemPosition gridItemPosition = null; 
        for (int i = 0; i < coloumHeight; i++)
        {
            // Cheek This grid position contains a blockage or insulator 
            if (ColoumBlockageCheeklist(rowIndex, i))
            {
                gridItemPosition = grid.GetGridObject(rowIndex, i);
                // after finding a insulator we don't break the loop, because we need Hightest blockage or insulator position 
            }
        }
        if (gridItemPosition == null)
        {
            return -1; 
        }

        index = gridItemPosition.GetY();
        return index; 
    }

    public bool IsColoumContainsAnyBlockage(int rowIndex, int coloumHeight)
    {
        bool hasBlockage = false; 
        for (int i = 0; i < coloumHeight; i++)
        {
            if (ColoumBlockageCheeklist(rowIndex, i))
            {
                hasBlockage = true;
                break; 
            }
        }
        return hasBlockage; 
    }

    #region ItemCompleteFallSystem
    public void ItemCompleteFall()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (ItemFallCheeklist(gridItemPosition))
                    {
                        for (int i = y - 1; i >= 0; i--)
                        {
                            if (IsValidPosition(x, i))
                            {
                                GridItemPosition newGridItemPosition = grid.GetGridObject(x, i);
                                if (newGridItemPosition.IsEmpty())
                                {
                                    gridItemPosition.GetGridItem().SetItemXY(newGridItemPosition.GetX(), newGridItemPosition.GetY());
                                    newGridItemPosition.SetGridItem(gridItemPosition.GetGridItem());
                                    gridItemPosition.ClearGridItem();
                                    gridItemPosition = newGridItemPosition;

                                    if (!CanSingleItemDirectFall(gridItemPosition.GetX(), gridItemPosition.GetY()))
                                    {
                                        for (int p = 0; p < gridWidth; p++)
                                        {
                                            for (int q = gridHeight - 1; q >= 0; q--)
                                            {
                                                if (ItemFallCheeklist(p, q))
                                                {
                                                    if (CanSingleItemSideFall(p, q))
                                                    {
                                                        FallItemsIntoSideEmptyPositions(p, q);
                                                        return;
                                                    }
                                                }
                                            }
                                        }
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
                    }
                }
            }
        }
    }
    #endregion ItemCompleteFallSystem

    public void SpawnItemInReplacedPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (gridItemPosition.IsEmpty())
                    {
                        ItemSO itemSO = levelSO.itemList[UnityEngine.Random.Range(0, levelSO.itemList.Count)];
                        GridItem gridItem = new GridItem(grid, x, y, itemSO);
                        gridItemPosition.SetGridItem(gridItem);
                        OnNewlyReplacedItem?.Invoke(gridItem, new NewlySpawnedItemEventArgs { gridItem = gridItem, gridItemPosition = gridItemPosition }); 
                    }
                }
            }
        }
    }

    #endregion Item Spawn System 

    public GridItemPosition PickRandomGridPosition()
    {
        List<LevelSO.LevelGridPosition> levelGridPositions = new List<LevelSO.LevelGridPosition>();
        if (this.levelSO != null && this.levelSO.levelGridPositionList != null && this.grid != null)
        {
            levelGridPositions = this.levelSO.levelGridPositionList.FindAll(lgp => BonusBoosterChecklist(this.grid.GetGridObject(lgp.x, lgp.y)));
        }
        else
        {
            Debug.LogError("Null object reference detected.");
        }
        System.Random random = new System.Random();
        LevelSO.LevelGridPosition levelGridPosition = levelGridPositions[random.Next(levelGridPositions.Count)];
        return this.grid.GetGridObject(levelGridPosition.x, levelGridPosition.y); 
    }

    //public void ReplaceItemWithBonusStrippedBooster()
    //{
    //    GridItemPosition gridItemPosition = PickRandomGridPosition(); 
    //    if (BonusBoosterChecklist(gridItemPosition))
    //    {
    //        blockedGridForSelectedBooster.Add(gridItemPosition);
    //        OnReplaceItemWithStripedBooster?.Invoke(gridItemPosition, new OnReplaceItemWithBonusBoosterEventArgs { grid = grid, boosterID = 1, gridItem = gridItemPosition.GetGridItem(), gridItemPosition = gridItemPosition }); 
    //    }
    //}

    //public void ReplaceItemWithBonusWrappedBooster()
    //{
    //    GridItemPosition gridItemPosition = PickRandomGridPosition(); 

    //    if (BonusBoosterChecklist(gridItemPosition))
    //    {
    //        blockedGridForSelectedBooster.Add(gridItemPosition);
    //        OnReplaceItemWithWrappedBooster?.Invoke(gridItemPosition, new OnReplaceItemWithBonusBoosterEventArgs { grid = grid, boosterID = 2, gridItem = gridItemPosition.GetGridItem(), gridItemPosition = gridItemPosition });
    //    }
    //}

    //public void ReplaceItemWithBonusPowerBooster()
    //{
    //    GridItemPosition gridItemPosition = PickRandomGridPosition(); 
    //    if (BonusBoosterChecklist(gridItemPosition))
    //    {
    //        blockedGridForSelectedBooster.Add(gridItemPosition);
    //        OnReplaceItemWithPowerBooster?.Invoke(gridItemPosition, new OnReplaceItemWithBonusBoosterEventArgs { grid = grid, boosterID = 3, gridItem = gridItemPosition.GetGridItem(), gridItemPosition = gridItemPosition });
    //    }
    //}

    //public void ReplaceItemWithBoosterDuringGameplay(int boosterId)
    //{
    //    GridItemPosition gridItemPosition = PickRandomGridPosition();

    //    blockedGridForSelectedBooster.Add(gridItemPosition);
    //    OnReplaceItemWithBonusBooster?.Invoke(gridItemPosition, new OnReplaceItemWithBonusBoosterEventArgs { grid = grid, boosterID = boosterId, gridItem = gridItemPosition.GetGridItem(), gridItemPosition = gridItemPosition });
    //}

    public bool IsSuffle()
    {
        bool isSuffle = false;
        List<PossibleMove> possibleMoves = GetAllPossibleMove();
        if (possibleMoves.Count > 0)
        {
            isSuffle = false;
        }
        else
        {
            isSuffle = true; 
        }
        return isSuffle; 
    }

    public List<PossibleMove> GetAllPossibleMove()
    {
        List<PossibleMove> allPossibleMove = new List<PossibleMove>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (MatchLinkCheeklist(gridItemPosition))
                    {
                        List<PossibleMove> testPossibleMove = new List<PossibleMove>();

                        testPossibleMove.Add(new PossibleMove(x, y, x + 1, y));
                        testPossibleMove.Add(new PossibleMove(x, y, x - 1, y));
                        testPossibleMove.Add(new PossibleMove(x, y, x, y + 1));
                        testPossibleMove.Add(new PossibleMove(x, y, x, y - 1));

                        for (int i = 0; i < testPossibleMove.Count; i++)
                        {
                            PossibleMove possibleMove = testPossibleMove[i];
                            bool skipPossibleMove = false;

                            for (int j = 0; j < allPossibleMove.Count; j++)
                            {
                                PossibleMove tmpPossibleMove = allPossibleMove[j];

                                if (tmpPossibleMove.GetStartX() == possibleMove.GetStartX() && tmpPossibleMove.GetStartY() == possibleMove.GetStartY()
                                    && tmpPossibleMove.GetEndX() == possibleMove.GetEndX() && tmpPossibleMove.GetEndY() == possibleMove.GetEndY())
                                {
                                    // already tested 
                                    skipPossibleMove = true;
                                    break;
                                }

                                if (tmpPossibleMove.GetStartX() == possibleMove.GetEndX() && tmpPossibleMove.GetStartY() == possibleMove.GetEndY()
                                    && tmpPossibleMove.GetEndX() == possibleMove.GetStartX() && tmpPossibleMove.GetEndY() == possibleMove.GetStartY())
                                {
                                    // already tested 
                                    skipPossibleMove = true;
                                    break;
                                }
                            }

                            if (skipPossibleMove)
                            {
                                continue;
                            }

                            if (CanSwapGridItems(possibleMove.GetStartX(), possibleMove.GetStartY(), possibleMove.GetEndX(), possibleMove.GetEndY()))
                            {
                                SwapGridItems(possibleMove.GetStartX(), possibleMove.GetStartY(), possibleMove.GetEndX(), possibleMove.GetEndY()); // swap not visually

                                List<MatchLink> allLinkedGridItemPositionList = GetAllMatchLinks();
                                if (allLinkedGridItemPositionList.Count > 0)
                                {
                                    possibleMove.allLinkedGridItemPositionList = allLinkedGridItemPositionList;
                                    allPossibleMove.Add(possibleMove);
                                }

                                SwapGridItems(possibleMove.GetStartX(), possibleMove.GetStartY(), possibleMove.GetEndX(), possibleMove.GetEndY()); // swap back
                            }

                        }
                    }
                   
                }           
            }
        }
        return allPossibleMove; 
    }

    public bool CheekForEmptyGridPositions()
    {
        bool existEmptyGridPositions = false;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (!gridItemPosition.HasGridItem())
                    {
                        existEmptyGridPositions = true;
                        break;
                    }
                }
            }
        }
        return existEmptyGridPositions; 
    }

    // Cheeklist for Match 
    public bool MatchLinkCheeklist(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.IsEmpty() || gridItemPosition.IsGridPositionInsulator() || gridItemPosition.IsRegister()
            || gridItemPosition.IsPowerBooster() || gridItemPosition.GetHasBlocker() || gridItemPosition.HasCell()) return false;
        return true;
    }

    public bool MatchLinkCheeklist(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false;
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (!MatchLinkCheeklist(gridItemPosition)) return false;
        return true; 
    }

    // Cheeklist for Swap 
    public bool SwapItemsCommonCheeklist(int startX, int startY, int endX, int endY)
    {                                                                          
        if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return false;                             // Invalid Position 
        if (startX == endX && startY == endY) return false;                                                             // Same Position  
        GridItemPosition startGridItemPosition = grid.GetGridObject(startX, startY);
        GridItemPosition endGridItemPosition = grid.GetGridObject(endX, endY);
        if (startGridItemPosition.IsEmpty() || startGridItemPosition.IsGridPositionInsulator() || startGridItemPosition.IsRegister() || startGridItemPosition.HasCell() || startGridItemPosition.GetHasBlocker()) return false;
        if (endGridItemPosition.IsEmpty() || endGridItemPosition.IsGridPositionInsulator() || endGridItemPosition.IsRegister() || endGridItemPosition.HasCell() || endGridItemPosition.GetHasBlocker()) return false;
        return true; 
    }

    // Cheeklist for Items Direct and Side Fall 
    public bool ItemFallCheeklist(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.IsEmpty() || gridItemPosition.IsGridPositionInsulator() ||
            (gridItemPosition.HasCell() && gridItemPosition.GetGridItem().GetCellProtectionLayer() != GridItem.CellProtectionLayer.levelOne)) return false;

        return true; 
    }

    public bool ItemFallCheeklist(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false;
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (!ItemFallCheeklist(gridItemPosition)) return false;
        return true; 
    }

    // Cheeklist for Searching Blockage in Coloum 
    public bool ColoumBlockageCheeklist(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false; 
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition.HasGridItem() && (IsGridItemPositionInsulator(x, y) || (gridItemPosition.HasCell() && gridItemPosition.GetGridItem().GetCellProtectionLayer() == GridItem.CellProtectionLayer.levelTwo))) return true;
        return false; 
    }

    public bool BonusBoosterChecklist(GridItemPosition gridItemPosition)
    {
        if (IsGridBlockedBySelectedBooster(gridItemPosition) || gridItemPosition.IsEmpty() || gridItemPosition.IsGridPositionInsulator() || gridItemPosition.HasBooster()
            || gridItemPosition.HasCell() || gridItemPosition.GetHasBlocker() || gridItemPosition.IsRegister()) return false;
        return true; 
    }

    public bool ItemSuffleChecklist(GridItemPosition gridItemPosition)
    {
        if (gridItemPosition.IsGridPositionInsulator() || gridItemPosition.HasBooster() || gridItemPosition.HasCell()
            || gridItemPosition.GetHasBlocker() || gridItemPosition.IsRegister()) return false;
        return true; 
    }

    public bool HammerBoosterChecklist(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false;
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition.IsGridPositionInsulator() || gridItemPosition.HasCell() || gridItemPosition.IsEmpty()) return false;
        return true; 
    }

    public bool IsGridBlockedBySelectedBooster(GridItemPosition gridItemPosition)
    {
        foreach (GridItemPosition item in blockedGridForSelectedBooster)
        {
            if (item.GetX() == gridItemPosition.GetX() && item.GetY() == gridItemPosition.GetY())
            {
                return true; 
            }
        }
        return false; 
    }

    public bool IsGridItemPositionInsulator(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false; 
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition.IsEmpty()) return false; 
        GridItem gridItem = gridItemPosition.GetGridItem();
        if (gridItem.IsInsulator())
        {
            return true; 
        }
        return false; 
    }

    public ItemSO GetItemSO(int x, int y)
    {
        if (!IsValidPosition(x, y)) return null;
        GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
        if (gridItemPosition.GetGridItem() == null) return null;
        return gridItemPosition.GetGridItem().GetItem(); 
    }

    public bool IsValidPosition(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= gridWidth && y <= gridHeight)
        {
            GridItemPosition gridItemPosition = grid.GetGridObject(x, y); 
            if (gridItemPosition != null && gridItemPosition.GetEnabled())
            {
                return true; 
            }
        }
        return false; 
    }

    public LevelSO GetLevelSO() { return levelSO; }
    public int GetLevelIndex() { return levelIndex; }
    public int GetScore() { return levelScore; }
    public int GetMoveCount() { return moveCount; }
    public bool HasMoveAvailable() { return moveCount > 0; }
    public void UseMove()
    {
        moveCount--;
        OnMoveUsed?.Invoke(this, EventArgs.Empty); 
    }

    public int GetRemainingCellToCollect() { return remainingCellToCollect; }
    public int GetTargetedCellCountToCollect() { return targetedCellCountToCollect; }
    public bool TryIsGameOver()
    {
        if (remainingCellToCollect < 1)
        {
            levelWin = true; 
            return true;
        }
        else if (!HasMoveAvailable())
        {
            OnOutOfMoves?.Invoke(this, EventArgs.Empty);
            levelWin = false;
            return true;
        }
        return false; 
    }

    public bool IsLevelWin() { return levelWin; }
}
