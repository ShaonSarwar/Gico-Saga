using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro; 

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] private float cellSize; 
    [SerializeField] private LevelSO levelSO;
    [SerializeField] private GameObject originObject;
    [SerializeField] private GameObject boardBG; 
    [SerializeField] private Transform gridItemTransformPrefab;
    [SerializeField] private Transform gridGlassTransformPrefab;
    [SerializeField] private Transform gridBackgroundTransformPrefab;
    [SerializeField] private Transform gridBlankTransformPrefab;
    [SerializeField] private Transform gridWheelTransformPrefab;
    [SerializeField] private TMP_InputField moveCountInputField;
    [SerializeField] private TMP_InputField objectiveInputField; 

    [Header("Item Explosion Color Refferences")]
    [SerializeField] private Color item1BlastColor;
    [SerializeField] private Color item2BlastColor;
    [SerializeField] private Color item3BlastColor;
    [SerializeField] private Color item4BlastColor;
    [SerializeField] private Color item5BlastColor;

    private Grid<GridItemPosition> grid;
    private Dictionary<GridItemPosition, Transform> gridGlassDictionary;
    private Dictionary<GridItemPosition, Transform> gridItemDictionary;
    private Dictionary<GridItemPosition, Transform> gridBackgroundDictionary;
    private Dictionary<GridItemPosition, Transform> gridWheelDictionary;

    
    private void Awake()
    {
        Setup();       
    }

    private void Setup()
    {
        //originObject.transform.localScale = new Vector3(levelSO.width, levelSO.height);
        GameObject go = new GameObject("local");
        go.transform.parent = originObject.transform;
        go.transform.localPosition = new Vector3(-0.5f, -0.5f);
        Vector3 originPosition = go.transform.position;
        levelSO.levelGridPositionList = new List<LevelSO.LevelGridPosition>();
        gridGlassDictionary = new Dictionary<GridItemPosition, Transform>();
        gridItemDictionary = new Dictionary<GridItemPosition, Transform>();
        gridBackgroundDictionary = new Dictionary<GridItemPosition, Transform>();
        gridWheelDictionary = new Dictionary<GridItemPosition, Transform>();

        SetItemBlastColor(); 

        grid = new Grid<GridItemPosition>(levelSO.width, levelSO.height, cellSize, null, originPosition, (Grid<GridItemPosition> g, int x, int y) => new GridItemPosition(g, x, y));

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                CreateVisual(gridBlankTransformPrefab, grid.GetGridObject(x, y)); 
                
            }
        }
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

        boardBG.transform.position = grid.GetWorldPosition(meanX, meanY) + new Vector3(offsetX, offsetY, 0);  
        boardBG.transform.localScale = new Vector3(bgWidth + 1, bgHeight + 1, 1);  
    }
    
    public void SetLevelMoveCount()
    {
        if (int.TryParse(moveCountInputField.text, out int moveCount))
        {
            levelSO.moveAmount = moveCount;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }
    }

    public void SetLevelObjective()
    {
        if (int.TryParse(objectiveInputField.text, out int targetCellCount))
        {
            levelSO.targetCellCount = targetCellCount;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }
    }

    private void SetItemBlastColor()
    {
        levelSO.itemColorList[0] = item1BlastColor;
        levelSO.itemColorList[1] = item2BlastColor;
        levelSO.itemColorList[2] = item3BlastColor;
        levelSO.itemColorList[3] = item4BlastColor;
        levelSO.itemColorList[4] = item5BlastColor; 
    }

    private void Update()
    {
        Vector3 mousePosition = GravitimeUtility.GetMouseWorldPosition();
        GridItemPosition gridItemPosition = grid.GetGridObject(mousePosition);
        if (gridItemPosition != null)
        {
            int x, y;
            grid.GetXY(mousePosition, out x, out y);

            // Enable Or Disable Grid Position 
            if (Input.GetMouseButtonDown(1))
            {
                CreateVisualBackground(gridBackgroundTransformPrefab, gridItemPosition); 
            }

            // Set or Remove Glass 
            if (Input.GetKeyDown(KeyCode.G))
            {
                SetGlass(gridItemPosition); 
            }

            // Set Insulator 
            if (Input.GetKeyDown(KeyCode.I))
            {
                SetInsulator(gridItemPosition); 
            }

            // Set Wheel 
            if (Input.GetKeyDown(KeyCode.W))
            {
                SetWheel(gridItemPosition); 
            }

            // Set Level One Blocker 
            if (Input.GetKeyDown(KeyCode.B))
            {
                SetLevelOneBlocker(gridItemPosition); 
            }

            // Set Level Two Blocker 
            if (Input.GetKeyDown(KeyCode.D))
            {
                SetLevelTwoBlocker(gridItemPosition); 
            }

            // Set Cell 
            if (Input.GetKeyDown(KeyCode.C))
            {
                SetCell(gridItemPosition); 
            }

            // Set Items by index-1 
            if (Input.GetKeyDown(KeyCode.Alpha1)) { SetItem(gridItemPosition, 1); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { SetItem(gridItemPosition, 2); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { SetItem(gridItemPosition, 3); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { SetItem(gridItemPosition, 4); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { SetItem(gridItemPosition, 5); }
        }
    }

    private void SetGlass(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        CreateVisualGlass(gridGlassTransformPrefab, gridItemPosition);
       
    } 

    private void SetWheel(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        CreateWheelVisual(gridWheelTransformPrefab, gridItemPosition); 
    }

    private void SetLevelOneBlocker(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        CreateLevelOneBlockerVisual(gridItemTransformPrefab, gridItemPosition); 
    }

    private void SetLevelTwoBlocker(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        CreateLevelTwoBlockerVisual(gridItemTransformPrefab, gridItemPosition); 
    }

    private void SetInsulator(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        CreateInsulatorVisual(gridItemTransformPrefab, gridItemPosition); 
    }

    private void SetItem(GridItemPosition gridItemPosition, int index)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        ItemSO itemSO = levelSO.itemList[index - 1];
        CreateVisualItem(gridItemTransformPrefab, gridItemPosition, itemSO);     
    }

    private void SetCell(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        CreateCellVisual(gridItemTransformPrefab, gridItemPosition); 
    }

    private void CreateVisual(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
    }

    // Create Cell Visual 
    private void CreateCellVisual(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        if (gridItemDictionary.ContainsKey(gridItemPosition))
        {
            ClearGridItemPosition(gridItemPosition);
            Destroy(gridItemDictionary[gridItemPosition].gameObject);
            gridItemDictionary.Remove(gridItemPosition); 
        }
        bool hasCell = true; 
        Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
        gridTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = levelSO.CellLayerTwo.sprite;
        gridItemDictionary[gridItemPosition] = gridTransform;

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.hasCell = hasCell;
                levelGridPosition.cellProtectionLayer = GridItem.CellProtectionLayer.levelTwo;
                levelGridPosition.itemSO = levelSO.CellLayerTwo; 
# if UNITY_EDITOR 
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    // Create Insulator Visual 
    private void CreateInsulatorVisual(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        if (gridItemDictionary.ContainsKey(gridItemPosition))
        {
            ClearGridItemPosition(gridItemPosition);
            Destroy(gridItemDictionary[gridItemPosition].gameObject);
            gridItemDictionary.Remove(gridItemPosition);
        }

        Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
        gridTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = levelSO.insulator.sprite; 
        gridItemDictionary[gridItemPosition] = gridTransform;

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.isInsulator = true;              
                levelGridPosition.itemSO = levelSO.insulator;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    // Instantiate Item Visual 
    private void CreateVisualItem(Transform prefabTransform, GridItemPosition gridItemPosition, ItemSO itemSO)
    {
        if (gridItemDictionary.ContainsKey(gridItemPosition))
        {
            ClearGridItemPosition(gridItemPosition); 
            Destroy(gridItemDictionary[gridItemPosition].gameObject);
            gridItemDictionary.Remove(gridItemPosition);
        }
        Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
        gridTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = itemSO.sprite;
        gridItemDictionary[gridItemPosition] = gridTransform;

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.itemSO = itemSO; 

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    // Instantiate Glass Visual 
    private void CreateVisualGlass(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        bool hasGlass = false; 
        if (gridGlassDictionary.ContainsKey(gridItemPosition))
        {
            hasGlass = false; 
            Destroy(gridGlassDictionary[gridItemPosition].gameObject);
            gridGlassDictionary.Remove(gridItemPosition);
        }
        else
        {
            hasGlass = true; 
            Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
            gridGlassDictionary[gridItemPosition] = gridTransform;            
        }

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.hasGlass = hasGlass; 

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    // Create Wheel Visual 
    private void CreateWheelVisual(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        bool hasWheel = false;
        if (gridWheelDictionary.ContainsKey(gridItemPosition))
        {
            hasWheel = false;
            Destroy(gridWheelDictionary[gridItemPosition].gameObject);
            gridWheelDictionary.Remove(gridItemPosition);
        }
        else
        {
            hasWheel = true;
            Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
            gridWheelDictionary[gridItemPosition] = gridTransform; 
        }

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.hasWheel = hasWheel; 

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    // Create Blocker Visual Level 1 
    private void CreateLevelOneBlockerVisual(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        if (gridItemDictionary.ContainsKey(gridItemPosition))
        {
            ClearGridItemPosition(gridItemPosition);
            Destroy(gridItemDictionary[gridItemPosition].gameObject);
            gridItemDictionary.Remove(gridItemPosition);
        }

        bool isBlocker = true;
        GridItem.BlockerType blockerType = GridItem.BlockerType.levelOne;
        Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
        gridTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = levelSO.BlockerLevelOne.sprite;
        gridItemDictionary[gridItemPosition] = gridTransform;

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.isBlocker = isBlocker;
                levelGridPosition.blockerType = blockerType;
                levelGridPosition.itemSO = levelSO.BlockerLevelOne; 
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    // Create Blocker Visual Level 3 
    private void CreateLevelTwoBlockerVisual(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        if (gridItemDictionary.ContainsKey(gridItemPosition))
        {
            ClearGridItemPosition(gridItemPosition);
            Destroy(gridItemDictionary[gridItemPosition].gameObject);
            gridItemDictionary.Remove(gridItemPosition);
        }

        bool isBlocker = true;
        GridItem.BlockerType blockerType = GridItem.BlockerType.levelTwo; 
        Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
        gridTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = levelSO.BlockerLeveltwo.sprite; 
        gridItemDictionary[gridItemPosition] = gridTransform;

        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.isBlocker = isBlocker;
                levelGridPosition.blockerType = blockerType;
                levelGridPosition.itemSO = levelSO.BlockerLeveltwo; 
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
            }
        }
    }

    private void CreateVisualBackground(Transform prefabTransform, GridItemPosition gridItemPosition)
    {
        int x = gridItemPosition.GetX();
        int y = gridItemPosition.GetY();
        if (gridBackgroundDictionary.ContainsKey(gridItemPosition))
        {
            Destroy(gridBackgroundDictionary[gridItemPosition].gameObject);
            gridBackgroundDictionary.Remove(gridItemPosition);
            if (levelSO.levelGridPositionList.Count > 0)
            {
                for (int i = 0; i < levelSO.levelGridPositionList.Count; i++)
                {
                    if (levelSO.levelGridPositionList[i].x == x && levelSO.levelGridPositionList[i].y == y)
                    {
                        levelSO.levelGridPositionList.RemoveAt(i);
                    }
                }
            }
        }
        else
        {
            LevelSO.LevelGridPosition levelGridPosition = new LevelSO.LevelGridPosition { itemSO = null, x = x, y = y, hasGlass = false, isInsulator = false };
            levelSO.levelGridPositionList.Add(levelGridPosition);
            Transform gridTransform = Instantiate(prefabTransform, gridItemPosition.GetWorldPosition(), Quaternion.identity);
            gridBackgroundDictionary[gridItemPosition] = gridTransform;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(levelSO);
#endif

    }

    public void ClearGridItemPosition(GridItemPosition gridItemPosition)
    {
        if (levelSO.levelGridPositionList.Count < 1) return;
        foreach (LevelSO.LevelGridPosition levelGridPosition in levelSO.levelGridPositionList)
        {
            if (levelGridPosition.x == gridItemPosition.GetX() && levelGridPosition.y == gridItemPosition.GetY())
            {
                levelGridPosition.isInsulator = false;
                levelGridPosition.blockerType = GridItem.BlockerType.None; 
                levelGridPosition.itemSO = null;
                levelGridPosition.hasGlass = false;
                levelGridPosition.hasWheel = false;
                levelGridPosition.isBlocker = false;
                levelGridPosition.hasCell = false; 
            }
        }
    }

    public void MakeLevel()
    {
        if (levelSO.levelGridPositionList == null || levelSO.width * levelSO.height != levelSO.levelGridPositionList.Count)
        {
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                    if (gridItemPosition.GetEnabled())
                    {
                        ItemSO itemSO = levelSO.itemList[UnityEngine.Random.Range(0, levelSO.itemList.Count)];
                        LevelSO.LevelGridPosition levelGridPosition = new LevelSO.LevelGridPosition { itemSO = itemSO, x = x, y = y, hasGlass = false, isInsulator = false };
                        levelSO.levelGridPositionList.Add(levelGridPosition);

#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
                    }
                }
            }
        }
    }
}
