using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class GridItemVisual 
{
    private Transform gridTransform;
    private GridItem gridItem;
    public GridItemVisual(Transform gridTransform, GridItem gridItem)
    {
        this.gridTransform = gridTransform;
        this.gridItem = gridItem;
    }

    public Transform GetTransform() { return gridTransform; }
    public GridItem GetGridItem() { return gridItem; }

    public void Update()
    {
        Vector3 targetPosition = gridItem.GetWorldPosition();
        Vector3 position = gridTransform.position;
        Vector3 moveDir = targetPosition - position;
        float moveSpeed = 10f;
        gridTransform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    public void PlayHighlightedAnimation(bool isPlay)
    {
        if (!gridItem.IsRegister() && !gridItem.IsInsulator())
        {
            if (isPlay)
            {
                gridTransform.GetComponent<Animation>().Play();
            }
            else
            {
                gridTransform.GetComponent<Animation>().Stop();
            }
        }   
    }
}
