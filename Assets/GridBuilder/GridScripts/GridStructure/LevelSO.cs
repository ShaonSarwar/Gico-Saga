using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 
[CreateAssetMenu]
public class LevelSO : ScriptableObject
{
    public int width;
    public int height;
    public List<ItemSO> itemList;
    public List<ItemSO> strippedBoosterList;
    public List<ItemSO> wrappedBoosterList; 
    public List<Color> itemColorList; 
    public List<LevelGridPosition> levelGridPositionList;
    public ItemSO blankItem;
    public ItemSO insulator;
    public ItemSO register; 
    public ItemSO powerBooster;
    public ItemSO BlockerLevelOne;
    public ItemSO BlockerLeveltwo;
    public ItemSO CellLayerOne;
    public ItemSO CellLayerTwo; 


    [Serializable]
    public class LevelGridPosition
    {
        public int x;
        public int y;
        public ItemSO itemSO;
        public bool hasGlass;
        public bool isInsulator;
        public bool hasWheel;
        public bool isBlocker;
        public GridItem.BlockerType blockerType;
        public bool hasCell;
        public GridItem.CellProtectionLayer cellProtectionLayer; 
    }
    public int moveAmount;
    public int targetCellCount; 
}
