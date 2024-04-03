using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

public class PowerBooster 
{
    private Grid<GridItemPosition> grid;
    private GridLogic gridLogic;
    private GridLogicVisual gridLogicVisual;
    List<Vector2Int> possibleGridPositionDestroyList;
    private Gridpooler gridpooler;
    private bool wasDoublePower = false; 

    private List<GameObject> splashEffectList = new List<GameObject>(); 
    public PowerBooster(Grid<GridItemPosition> grid, GridLogic gridLogic, GridLogicVisual gridLogicVisual, Gridpooler gridpooler)
    {
        this.grid = grid;
        this.gridLogic = gridLogic;
        this.gridLogicVisual = gridLogicVisual;
        this.gridpooler = gridpooler; 
        possibleGridPositionDestroyList = new List<Vector2Int>(); 
    }

    public void TryDestroyPowerBooster(GridItem gridItem, GridItem swapItem)
    {
        if (gridItem.IsBooster())
        {
            int gridPositionDestroyOriginX = gridItem.GetX();
            int gridPositionDestroyOriginY = gridItem.GetY();

            GridItemPosition boosterOrigin = grid.GetGridObject(gridPositionDestroyOriginX, gridPositionDestroyOriginY);
            // Destroy Same Color Item (Same color is which one was swap with power booster)  
            if (swapItem != null)
            {
                MakeValidGridPositionDestroyList(swapItem, gridPositionDestroyOriginX, gridPositionDestroyOriginY);
                CreateSplash(gridPositionDestroyOriginX, gridPositionDestroyOriginY);
                FunctionTimer.Create(() =>
                {
                    DestroySplashEffects();
                    FunctionTimer.Create(() =>
                    {
                        CreateInstance(swapItem);
                        FunctionTimer.Create(() =>
                        {
                            DestroyInstance();
                            gridLogic.TryDestroyGridItem(boosterOrigin);
                            if (wasDoublePower)
                            {
                                GridItemPosition swapOrigin = grid.GetGridObject(swapItem.GetX(), swapItem.GetY());
                                gridLogic.TryDestroyGridItem(swapOrigin);
                            }
                        }, 0.5f);
                    }, 0.1f);                   
                }, 1.5f);
            }
            else
            {
                gridLogic.TryDestroyGridItem(boosterOrigin); 
            }
        }
    }

    public void MakeValidGridPositionDestroyList(GridItem gridItem, int originX, int originY)
    {
        int itemID = gridItem.GetItem().itemID;
        bool isSwapItemWasBooster = gridItem.IsBooster();
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (gridLogic.IsValidPosition(x, y) || (x != originX && y != originY))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (gridItemPosition.HasGridItem())
                    {
                        if (isSwapItemWasBooster && gridItem.GetBoosterID() == 3)
                        {
                            wasDoublePower = true;
                            if (x != gridItem.GetX() && y != gridItem.GetY())
                            {
                                possibleGridPositionDestroyList.Add(new Vector2Int(x, y));
                            }
                        }
                        else
                        {
                            if (gridItemPosition.GetGridItem().GetItem().itemID == itemID)
                            {
                                possibleGridPositionDestroyList.Add(new Vector2Int(x, y)); 
                            }
                        }
                    }
                }
            }
        }
    }

    public void CreateInstance(GridItem gridItem)
    {
        int itemID = gridItem.GetItem().itemID;
        bool isSwapItemWasBooster = gridItem.IsBooster();
        if (!isSwapItemWasBooster || gridItem.GetBoosterID() == 3) return; 

        foreach (Vector2Int gridPosition in possibleGridPositionDestroyList)
        {
            int boosterID = gridItem.GetBoosterID();
            gridLogic.CreateBoosterInstance(gridPosition.x, gridPosition.y, itemID, boosterID);
        }
    }

    // Make Swap Item Instance 
    public void MakeInstance(GridItem gridItem)
    {
        int itemID = gridItem.GetItem().itemID;
        bool isSwapItemWasBooster = gridItem.IsBooster(); 
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (gridLogic.IsValidPosition(x, y))
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (gridItemPosition.HasGridItem())
                    {
                        if (isSwapItemWasBooster && gridItem.GetBoosterID() == 3)
                        {
                            possibleGridPositionDestroyList.Add(new Vector2Int(x, y));
                        }
                        else
                        {
                            if (gridItemPosition.GetGridItem().GetItem().itemID == itemID)
                            {
                                if (isSwapItemWasBooster)
                                {
                                    int boosterID = gridItem.GetBoosterID(); 
                                    gridLogic.CreateBoosterInstance(x, y, itemID, boosterID);
                                    possibleGridPositionDestroyList.Add(new Vector2Int(x, y));
                                }
                                else
                                {
                                    possibleGridPositionDestroyList.Add(new Vector2Int(x, y));
                                }
                            }
                        }                      
                    }
                }
            }
        }

        //DestroyInstance();

    }

    public void CreateSplash(int originX, int originY)
    {
        foreach (Vector2Int gridposition in possibleGridPositionDestroyList)
        {
            if (gridLogic.IsValidPosition(gridposition.x, gridposition.y))
            {
                Vector3 splashOrigin = grid.GetWorldPosition(originX, originY) + new Vector3(0.5f, 0.5f);
                Vector3 splashEnd = grid.GetWorldPosition(gridposition.x, gridposition.y) + new Vector3(0.5f, 0.5f);

                GridItemPosition itemPosition = grid.GetGridObject(gridposition.x, gridposition.y);

                GridItem item = itemPosition.GetGridItem();
                GridItemVisual itemVisual = gridLogicVisual.GridVisualDictionary[item];
                Transform itemVisualTransform = itemVisual.GetTransform(); 
                GameObject splashObject = gridpooler.GetPooledGridObject(PoolType.Power, splashOrigin, Quaternion.identity);

                //var normal = (splashEnd - splashOrigin);
                //float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
                //Quaternion rot = new Quaternion();
                //rot.eulerAngles = new Vector3(angle - 90, 90, -90);
                splashObject.transform.LookAt(splashEnd); 

                ParticleSystem particleSystem = splashObject.GetComponent<ParticleSystem>();
                particleSystem.trigger.RemoveCollider(0);
                particleSystem.trigger.AddCollider(itemVisualTransform);
                particleSystem.Play();

                splashEffectList.Add(splashObject);               
            }
        }
    }



    public void DestroyInstance()
    {
        foreach (Vector2Int gridPositionDestroy in possibleGridPositionDestroyList)
        {
            if (gridLogic.IsValidPosition(gridPositionDestroy.x, gridPositionDestroy.y))
            {
                GridItemPosition gridPosition = grid.GetGridObject(gridPositionDestroy.x, gridPositionDestroy.y);
                if (gridPosition.HasGridItem())
                {
                    if (gridPosition.IsRegister())
                    {
                        gridLogic.TryDestroyRegister(gridPosition);
                    }
                    else if (gridPosition.IsGridPositionInsulator())
                    {
                        gridLogicVisual.TryDamageInsulator(gridPosition, 1);
                    }
                    else if (gridPosition.HasBooster())
                    {
                        gridLogic.TryDestroyBoosterAfterDelay(gridPosition);
                    }
                    else if (gridPosition.GetHasBlocker())
                    {
                        gridLogic.TryDestroyBlocker(gridPosition);
                    }
                    else if (gridPosition.HasCell())
                    {
                        gridLogic.TryDestroyCell(gridPosition); 
                    }
                    else
                    {
                        gridLogic.TryDestroyGridItem(gridPosition);
                    }
                }
            }
        }
    }

    public void DestroySplashEffects()
    {
        if (splashEffectList.Count < 1) return;

        foreach (GameObject splashObj in splashEffectList)
        {
            gridpooler.ReturnGridObjectToPool(PoolType.Power, splashObj); 
        }
    }
}

