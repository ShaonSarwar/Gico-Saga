using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] 
public class ShopItem : ScriptableObject
{
    public Sprite itemSprite;
    public string itemName; 
    public int amount;
    public int price;
}
