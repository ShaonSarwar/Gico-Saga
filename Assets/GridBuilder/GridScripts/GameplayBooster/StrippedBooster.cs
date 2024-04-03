using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class StrippedBooster 
{
    private Grid<GridItemPosition> grid; 
    private GridLogic gridLogic;
    private GridLogicVisual gridLogicVisual;
    private Gridpooler gridpooler; 
    public StrippedBooster(Grid<GridItemPosition> grid, GridLogic gridLogic, GridLogicVisual gridLogicVisual, Gridpooler gridpooler)
    {
        this.grid = grid;
        this.gridLogic = gridLogic;
        this.gridLogicVisual = gridLogicVisual;
        this.gridpooler = gridpooler; 
    }

    public void TryDestroyStrippedBooster( GridItem gridItem)
    {
        if (gridItem.IsBooster())
        {
            List<Vector2Int> possibleGridPositionDestroyList = new List<Vector2Int>();

            int gridPositionDestroyOriginX = gridItem.GetX();
            int gridPositionDestroyOriginY = gridItem.GetY();

            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX + 1, gridPositionDestroyOriginY));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX - 1, gridPositionDestroyOriginY));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX, gridPositionDestroyOriginY + 1));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX, gridPositionDestroyOriginY - 1));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX + 1, gridPositionDestroyOriginY + 1));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX - 1, gridPositionDestroyOriginY + 1));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX + 1, gridPositionDestroyOriginY - 1));
            possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX - 1, gridPositionDestroyOriginY - 1));
            //possibleGridPositionDestroyList.Add(new Vector2Int(gridPositionDestroyOriginX, gridPositionDestroyOriginY));

            GameObject effectObject = gridpooler.GetPooledGridObject(PoolType.Stripped, grid.GetWorldPosition(gridPositionDestroyOriginX, gridPositionDestroyOriginY) +new Vector3(0.5f, 0.5f), Quaternion.identity);
            if (effectObject != null)
            {
                ParticleSystem particleSystem = effectObject.GetComponent<ParticleSystem>();
                particleSystem.Play(); 
            }
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
                            FunctionTimer.Create(() =>
                            {
                                gridLogic.TryDestroyBoosterAfterDelay(gridPosition);
                            }, 0.1f);
                        }
                        else if (gridPosition.GetHasBlocker())
                        {
                            gridLogic.TryDestroyBlocker(gridPosition);
                        }
                        else if (gridPosition.HasCell())
                        {
                            Debug.Log("TryDestroyLevel2Cell");
                            gridLogic.TryDestroyCell(gridPosition); 
                        }
                        else
                        {
                            gridLogic.TryDestroyGridItem(gridPosition);
                        }
                    }
                }
            }

            GridItemPosition boosterOrigin = grid.GetGridObject(gridPositionDestroyOriginX, gridPositionDestroyOriginY);
            gridLogic.TryDestroyGridItem(boosterOrigin); 

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.Stripped, effectObject);
            }, 1f); 
        }       
    }
}
