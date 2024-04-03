using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class GridGlassVisual 
{
    private Transform glassTransform;
    private GridItemPosition gridItemPosition;
    private Gridpooler gridpooler; 

    public GridGlassVisual(Gridpooler gridpooler, Transform glassTransform, GridItemPosition gridItemPosition)
    {
        this.gridpooler = gridpooler; 
        this.glassTransform = glassTransform;
        this.gridItemPosition = gridItemPosition;
        gridItemPosition.OnGlassDestroyed += GridItemPosition_OnGlassDestroyed;
    }

    private void GridItemPosition_OnGlassDestroyed(object sender, EventArgs e)
    {
        FunctionTimer.Create(() =>
        {
            gridpooler.ReturnGridObjectToPool(PoolType.Glass, glassTransform.gameObject);
        }, 0.5f); 
    }
}
