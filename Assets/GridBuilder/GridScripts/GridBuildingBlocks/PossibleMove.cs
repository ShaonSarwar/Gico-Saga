using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleMove 
{
    private int startX;
    private int startY;
    private int endX;
    private int endY;

    public List<MatchLink> allLinkedGridItemPositionList; 
    public PossibleMove(int startX, int startY, int endX, int endY)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY; 
    }

    public int GetStartX() { return startX; }
    public int GetStartY() { return startY; }
    public int GetEndX() { return endX; }
    public int GetEndY() { return endY; }
}
