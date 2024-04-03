using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    private int itemID;
    private int mount;
    public InventoryItem(int itemID, int mount)
    {
        this.itemID = itemID;
        this.mount = mount;
    }

    public int GetItemID() { return itemID; }
    public int GetMount() { return mount; }
    public void SetMount(int mount)
    {
        this.mount = mount;
    }
}
