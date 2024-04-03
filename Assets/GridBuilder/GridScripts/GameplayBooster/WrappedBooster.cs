using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrappedBooster 
{
    private Grid<GridItemPosition> grid;
    private GridLogic gridLogic;
    private GridLogicVisual gridLogicVisual;
    private Gridpooler gridpooler; 
    public WrappedBooster(Grid<GridItemPosition> grid, GridLogic gridLogic, GridLogicVisual gridLogicVisual, Gridpooler gridpooler)
    {
        this.grid = grid;
        this.gridLogic = gridLogic;
        this.gridLogicVisual = gridLogicVisual;
        this.gridpooler = gridpooler; 
    }

    public void TryDestroyWrappedBooster(GridItem gridItem)
    {
        if (gridItem.IsBooster())
        {
            List<Vector2Int> possibleGridPositionDestroyList = new List<Vector2Int>();

            int x = gridItem.GetX();
            int y = gridItem.GetY();

            //possibleGridPositionDestroyList.Add(new Vector2Int(x, y)); 
            for (int i = 0; i < grid.GetHeight(); i++)
            {
                possibleGridPositionDestroyList.Add(new Vector2Int(x + i, y + i));
                possibleGridPositionDestroyList.Add(new Vector2Int(x - i, y + i));
                possibleGridPositionDestroyList.Add(new Vector2Int(x + i, y - i));
                possibleGridPositionDestroyList.Add(new Vector2Int(x - i, y - i)); 
            }

            GameObject effectObject1 = gridpooler.GetPooledGridObject(PoolType.Wrapped, grid.GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), Quaternion.identity);
            effectObject1.transform.eulerAngles = effectObject1.transform.forward * (45f - 90f); 
            if (effectObject1 != null)
            {
                ParticleSystem cross1 = effectObject1.GetComponent<ParticleSystem>();
                cross1.Play();
            }

            GameObject effectObject2 = gridpooler.GetPooledGridObject(PoolType.Wrapped, grid.GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), Quaternion.identity);
            effectObject2.transform.eulerAngles = effectObject2.transform.forward * (135f - 90f);
            if (effectObject2 != null)
            {
                ParticleSystem cross2 = effectObject2.GetComponent<ParticleSystem>();
                cross2.Play();
            }

            GameObject effectObject3 = gridpooler.GetPooledGridObject(PoolType.Wrapped, grid.GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), Quaternion.identity);
            effectObject3.transform.eulerAngles = effectObject3.transform.forward * (225f - 90f);
            if (effectObject3 != null)
            {
                ParticleSystem cross3 = effectObject3.GetComponent<ParticleSystem>();
                cross3.Play();
            }

            GameObject effectObject4 = gridpooler.GetPooledGridObject(PoolType.Wrapped, grid.GetWorldPosition(x, y) + new Vector3(0.5f, 0.5f), Quaternion.identity);
            effectObject4.transform.eulerAngles = effectObject4.transform.forward * (315f - 90f);
            if (effectObject4 != null)
            {
                ParticleSystem cross4 = effectObject4.GetComponent<ParticleSystem>();
                cross4.Play();
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
                            gridLogic.TryDestroyCell(gridPosition); 
                        }
                        else
                        {
                            gridLogic.TryDestroyGridItem(gridPosition);
                        }
                    }
                }
            }

            GridItemPosition boosterOrigin = grid.GetGridObject(x, y);
            gridLogic.TryDestroyGridItem(boosterOrigin); 

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.Wrapped, effectObject1);
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.Wrapped, effectObject2);
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.Wrapped, effectObject3);
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.Wrapped, effectObject4);
            }, 0.5f);
        }
    }
}
