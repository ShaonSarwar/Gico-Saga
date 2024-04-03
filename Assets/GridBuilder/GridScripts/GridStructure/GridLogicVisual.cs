using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Linq;

public class GridLogicVisual : MonoBehaviour
{
    // Events 
    public event EventHandler OnStateChanged;
    public event EventHandler OnGameOver;

    // This event for Cached all the match link before spawning register

    public event EventHandler OnBeforeRegisterSpawned;

    public event EventHandler<OnVisualLevelSetEventAgrs> OnVisualLevelSet;
    public class OnVisualLevelSetEventAgrs : EventArgs
    {
        public Grid<GridItemPosition> grid;
        public LevelSO levelSO;
    }

    public event EventHandler<OnSwapPositionsEventArgs> OnSwapPositions;
    public class OnSwapPositionsEventArgs : EventArgs
    {
        public int startX;
        public int startY;
        public int endX;
        public int endY;
    }

    public event EventHandler<OnRegisterBoosterEventArgs> OnRegisterBooster; 
    public class OnRegisterBoosterEventArgs : EventArgs
    {
        public int boosterID;
        public Grid<GridItemPosition> grid;
        public GridItem gridItem; 
    }

    // Enum 
    public enum State
    {
        Busy,
        Liazu,
        Suffle, 
        WaitingForUser,
        TryingFindMatch,
        Wheel, 
        GameOver
    }

    public enum ItemState { Direct, Spawn, Side, None }


    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private Transform gridItemVisualTransformPrefab;
    [SerializeField] private Transform gridItemBackgroundTransformPrefab;
    [SerializeField] private Gridpooler gridpooler;
    [SerializeField] private GridLevelUI gridLevelUI;
    [SerializeField] private TacticalBooster tacticalBooster; 
    [SerializeField] private InsulatorLogic insulatorLogic;
    [SerializeField] private Camera mCamera;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SoundCollection soundSFXCollection;
    [SerializeField] private Transform gridWheelVisualTransformPrefab;
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private TextMeshProUGUI itemFallObservation; 

    private Grid<GridItemPosition> grid;
    private LevelSO levelSO;
    private Dictionary<GridItem, GridItemVisual> gridVisualDictionary;
    private Dictionary<GridItemPosition, GridWheel> gridWheelDictionary;
    private bool isSetup;
    private State state;
    private bool busyFunc; 
    private ItemState itemState;
    private int startDragX;
    private int startDragY;
    private float busyTimer;
    private Action OnBusyTimerElapsedAction;
    private bool canCreateRegister;
    private bool canItemHighlighted;
    private bool canSpinWheel;
    private bool selectedBoosterCheck; 

    private float globalTimer = 0.4f;
    private float firstTimer = 0.3f;
  
    public Dictionary<GridItem, GridItemVisual> GridVisualDictionary { get { return gridVisualDictionary; } }

    private void Awake()
    {
        isSetup = false;
        gridLogic.OnLevelSet += GridLogic_OnLevelSet;
    }

    private void GridLogic_OnLevelSet(object sender, GridLogic.OnLevelSetEventArgs e)
    {
        levelSO = e.levelSO;
        FunctionTimer.Create(() => { SetLevel(sender as GridLogic, e.grid); }, 0.1f);

    }

    private void SetLevel(GridLogic gridLogic, Grid<GridItemPosition> grid)
    {
        this.gridLogic = gridLogic;
        this.grid = grid;
        OnVisualLevelSet?.Invoke(this, new OnVisualLevelSetEventAgrs { grid = grid, levelSO = levelSO });
        Input.multiTouchEnabled = false;
        gridLogic.OnNewlySpawnedItem += GridLogic_OnNewlySpawnedItem;
        gridLogic.OnGridItemDestroyed += GridLogic_OnGridItemDestroyed;

        gridLogic.OnNewlyReplacedItem += GridLogic_OnNewlyReplacedItem;
        gridLogic.OnGridItemReplaced += GridLogic_OnGridItemReplaced;

        insulatorLogic.OnReplaceItemWithRegister += InsulatorLogic_OnReplaceItemWithRegister;
        gridLogic.OnRegisterItemDestroyed += GridLogic_OnRegisterItemDestroyed;
        gridLogic.OnInsulatorDestroyed += GridLogic_OnInsulatorDestroyed;

        gridLogic.OnLevelOneBlockerDestroyed += GridLogic_OnLevelOneBlockerDestroyed;
        gridLogic.OnLevelTwoBlockerDestroyed += GridLogic_OnLevelTwoBlockerDestroyed;

        gridLogic.OnOneLayerCellDestroyed += GridLogic_OnOneLayerCellDestroyed;
        gridLogic.OnTwoLayerCellDestroyed += GridLogic_OnTwoLayerCellDestroyed;

        gridLogic.OnCreateBoosterInstance += GridLogic_OnCreateBoosterInstance;
        gridLogic.OnBoosterCreated += GridLogic_OnBoosterCreated;

        //gridLogic.OnReplaceItemWithBonusBooster += GridLogic_OnReplaceItemWithBonusBooster;

        gridVisualDictionary = new Dictionary<GridItem, GridItemVisual>();
        gridWheelDictionary = new Dictionary<GridItemPosition, GridWheel>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
                if (gridItemPosition != null && gridItemPosition.GetEnabled() && gridItemPosition.HasGridItem())
                {
                    GridItem gridItem = gridItemPosition.GetGridItem();

                    Vector3 position = grid.GetWorldPosition(x, y);
                    position = new Vector3(position.x, 12); // Item fall from 12 unit up 
                                                            // Pool GridItem Object 

                    GameObject gridItemTransformObject = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity);

                    Transform gridItemTransform = gridItemTransformObject.transform;
                    gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItem.GetItem().sprite;
                    Material material = new Material(dissolveMaterial);
                    gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
                    GridItemVisual gridItemVisual = new GridItemVisual(gridItemTransform, gridItem);
                    gridVisualDictionary[gridItem] = gridItemVisual;

                    // Pool Background Object 
                    Transform gridBackgroundvisualTransform = gridpooler.GetPooledGridObject(PoolType.Background, grid.GetWorldPosition(x, y), Quaternion.identity).transform;
                    // Wheel if have any 
                    if (gridItemPosition.GetHasWheel())
                    {
                        Transform gridWheelVisualTransform = Instantiate(gridWheelVisualTransformPrefab, grid.GetWorldPosition(x, y), Quaternion.identity);
                        GridWheel gridWheel = new GridWheel(x, y, grid, gridLogic);
                        gridWheelDictionary[gridItemPosition] = gridWheel;
                        Debug.Log("Wheel Found");
                    }

                    // Pool Glass Visual 
                    if (gridItemPosition.HasGlass())
                    {
                        Transform glassTransform = gridpooler.GetPooledGridObject(PoolType.Glass, grid.GetWorldPosition(x, y), Quaternion.identity).transform;
                        GridGlassVisual gridGlassVisual = new GridGlassVisual(gridpooler, glassTransform, gridItemPosition);
                    }
                }
            }
        }

        isSetup = true;
        if (PlayerPrefs.HasKey("BonusStripped"))
        {
            if (PlayerPrefs.GetInt("BonusStripped") == 1)
            {
                GridItemPosition gridItemPosition = gridLogic.PickRandomGridPosition();
                if (gridLogic.BonusBoosterChecklist(gridItemPosition))
                {
                    gridLogic.blockedGridForSelectedBooster.Add(gridItemPosition);
                    //GridLogic_OnReplaceItemWithStripedBooster(gridItemPosition, gridItemPosition.GetGridItem(), 1);
                    GridLogic_OnReplaceItemWithBonusBooster(gridItemPosition, gridItemPosition.GetGridItem(), 1);
                    PlayerPrefs.DeleteKey("BonusStripped");
                }
            }
        }

        if (PlayerPrefs.HasKey("BonusWrapped"))
        {
            if (PlayerPrefs.GetInt("BonusWrapped") == 1)
            {
                GridItemPosition gridItemPosition = gridLogic.PickRandomGridPosition();
                if (gridLogic.BonusBoosterChecklist(gridItemPosition))
                {
                    gridLogic.blockedGridForSelectedBooster.Add(gridItemPosition);
                    //GridLogic_OnReplaceItemWithWrappedBooster(gridItemPosition, gridItemPosition.GetGridItem(), 2);
                    GridLogic_OnReplaceItemWithBonusBooster(gridItemPosition, gridItemPosition.GetGridItem(), 2);
                    PlayerPrefs.DeleteKey("BonusWrapped");
                }
            }
        }

        if (PlayerPrefs.HasKey("BonusPower"))
        {
            if (PlayerPrefs.GetInt("BonusPower") == 1)
            {
                GridItemPosition gridItemPosition = gridLogic.PickRandomGridPosition();
                if (gridLogic.BonusBoosterChecklist(gridItemPosition))
                {
                    gridLogic.blockedGridForSelectedBooster.Add(gridItemPosition);
                    //GridLogic_OnReplaceItemWithPowerBooster(gridItemPosition, gridItemPosition.GetGridItem(), 3);
                    GridLogic_OnReplaceItemWithBonusBooster(gridItemPosition, gridItemPosition.GetGridItem(), 3);
                    PlayerPrefs.DeleteKey("BonusPower");
                }
            }
        }

        FunctionTimer.Create(() =>
        {
            SetBusyState(0.5f, () => SetState(State.TryingFindMatch), () => true);
        }, 0.5f);
    }

    #region GridLogic_Callbacks
    private void GridLogic_OnReplaceItemWithPowerBooster(GridItemPosition gridItemPosition, GridItem gridItem, int boosterID)
    {     
        if (gridItemPosition != null && gridItem != null)
        {
            if (gridVisualDictionary.ContainsKey(gridItem))
            {
                Debug.Log("Power Found"); 
            }
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItem];
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridVisualDictionary.Remove(e.gridItem);
            //gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridItemPosition.ClearGridItem();

            // Item dissolve shader 
            Material replacematerial = gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().material;
            float current = 0;
            FunctionUpdate.CreateUpdate(() =>
            {
                current = Mathf.MoveTowards(current, 1f, 2f * Time.deltaTime);
                replacematerial.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, curve.Evaluate(current)));
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
                gridVisualDictionary.Remove(gridItem);
                gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
                //material.SetFloat("_dissolveAmount", 0);
            }, 0.5f);

            Vector3 position = gridItemPosition.GetWorldPosition();

            ItemSO powerItemSO = levelSO.powerBooster;
            GridItem powerGridItem = new GridItem(this.grid, gridItemPosition.GetX(), gridItemPosition.GetY(), powerItemSO);
            powerGridItem.SetBoosterID(boosterID);
            gridItemPosition.SetGridItem(powerGridItem);

            OnRegisterBooster?.Invoke( gridItem, new OnRegisterBoosterEventArgs { boosterID = boosterID, grid = this.grid, gridItem = gridItemPosition.GetGridItem() });
            // Dissolve Shader 

            Transform gridBoosterTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;

            Color color = gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color;
            color.a = 0;
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItemPosition.GetGridItem().GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;


            FunctionTimer.Create(() =>
            {
                float newCurrent = 0;
                FunctionUpdate.CreateUpdate(() =>
                {
                    if (newCurrent <= 1f)
                    {
                        newCurrent = Mathf.MoveTowards(newCurrent, 1, 2.5f * Time.deltaTime);
                        color.a = Mathf.Lerp(0, 1, curve.Evaluate(newCurrent));
                        gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
                    }
                }, 0.5f);
            }, 0.5f);

            GridItemVisual newGridItemVisual = new GridItemVisual(gridBoosterTransform, gridItemPosition.GetGridItem());
            gridVisualDictionary[gridItemPosition.GetGridItem()] = newGridItemVisual;
        }
    }

    private void GridLogic_OnReplaceItemWithWrappedBooster(GridItemPosition gridItemPosition, GridItem gridItem, int boosterID)
    {
        if (gridItemPosition != null && gridItem != null)
        {
            if (gridVisualDictionary.ContainsKey(gridItem))
            {
                Debug.Log("Wrapped Found");
            }
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItem];
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridVisualDictionary.Remove(e.gridItem);
            //gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridItemPosition.ClearGridItem();

            // Item dissolve shader 
            Material replacematerial = gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().material;
            float current = 0;
            FunctionUpdate.CreateUpdate(() =>
            {
                current = Mathf.MoveTowards(current, 1f, 2f * Time.deltaTime);
                replacematerial.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, curve.Evaluate(current)));
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
                gridVisualDictionary.Remove(gridItem);
                gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
                //material.SetFloat("_dissolveAmount", 0);
            }, 0.5f);

            Vector3 position = gridItemPosition.GetWorldPosition();

            ItemSO wrappedItemSO = levelSO.wrappedBoosterList[UnityEngine.Random.Range(0, levelSO.wrappedBoosterList.Count)];
            GridItem wrappedGridItem = new GridItem(this.grid, gridItemPosition.GetX(), gridItemPosition.GetY(), wrappedItemSO);
            wrappedGridItem.SetBoosterID(boosterID);
            gridItemPosition.SetGridItem(wrappedGridItem);

            OnRegisterBooster?.Invoke( gridItem, new OnRegisterBoosterEventArgs { boosterID = boosterID, grid = this.grid, gridItem = gridItemPosition.GetGridItem() });
            // Dissolve Shader 

            Transform gridBoosterTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;

            Color color = gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color;
            color.a = 0;
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItemPosition.GetGridItem().GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;


            FunctionTimer.Create(() =>
            {
                float newCurrent = 0;
                FunctionUpdate.CreateUpdate(() =>
                {
                    if (newCurrent <= 1f)
                    {
                        newCurrent = Mathf.MoveTowards(newCurrent, 1, 2.5f * Time.deltaTime);
                        color.a = Mathf.Lerp(0, 1, curve.Evaluate(newCurrent));
                        gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
                    }
                }, 0.5f);
            }, 0.5f);

            GridItemVisual newGridItemVisual = new GridItemVisual(gridBoosterTransform, gridItemPosition.GetGridItem());
            gridVisualDictionary[gridItemPosition.GetGridItem()] = newGridItemVisual;
        }
    }

    private void GridLogic_OnReplaceItemWithStripedBooster(GridItemPosition gridItemPosition, GridItem gridItem, int boosterID)
    {
        if (gridItemPosition != null && gridItem != null)
        {
            if (gridVisualDictionary.ContainsKey(gridItem))
            {
                Debug.Log("Stripped Found");
            }
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItem];
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridVisualDictionary.Remove(e.gridItem);
            //gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridItemPosition.ClearGridItem();

            // Item dissolve shader 
            Material replacematerial = gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().material;
            float current = 0;
            FunctionUpdate.CreateUpdate(() =>
            {
                current = Mathf.MoveTowards(current, 1f, 2f * Time.deltaTime);
                replacematerial.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, curve.Evaluate(current)));
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
                gridVisualDictionary.Remove(gridItem);
                gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
                //material.SetFloat("_dissolveAmount", 0);
            }, 0.5f);

            Vector3 position = gridItemPosition.GetWorldPosition();

            ItemSO strippedItemSO = levelSO.strippedBoosterList[UnityEngine.Random.Range(0, levelSO.strippedBoosterList.Count)];
            GridItem strippedGridItem = new GridItem(this.grid, gridItemPosition.GetX(), gridItemPosition.GetY(), strippedItemSO);
            strippedGridItem.SetBoosterID(boosterID);
            gridItemPosition.SetGridItem(strippedGridItem);

            OnRegisterBooster?.Invoke(gridItem, new OnRegisterBoosterEventArgs { boosterID = boosterID, grid = this.grid, gridItem = gridItemPosition.GetGridItem() });
            // Dissolve Shader 

            Transform gridBoosterTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;

            Color color = gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color;
            color.a = 0;
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItemPosition.GetGridItem().GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;


            FunctionTimer.Create(() =>
            {
                float newCurrent = 0;
                FunctionUpdate.CreateUpdate(() =>
                {
                    if (newCurrent <= 1f)
                    {
                        newCurrent = Mathf.MoveTowards(newCurrent, 1, 2.5f * Time.deltaTime);
                        color.a = Mathf.Lerp(0, 1, curve.Evaluate(newCurrent));
                        gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
                    }
                }, 0.5f);
            }, 0.5f);

            GridItemVisual newGridItemVisual = new GridItemVisual(gridBoosterTransform, gridItemPosition.GetGridItem());
            gridVisualDictionary[gridItemPosition.GetGridItem()] = newGridItemVisual;
        }
    }

    private void GridLogic_OnReplaceItemWithBonusBooster(GridItemPosition gridItemPosition, GridItem gridItem, int boosterID)
    {
        if (gridItemPosition != null && gridItem != null)
        {
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItem];
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            //gridVisualDictionary.Remove(e.gridItem);
            //gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridItemPosition.ClearGridItem();

            // Item dissolve shader 
            Material replacematerial = gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().material;
            float current = 0;
            FunctionUpdate.CreateUpdate(() =>
            {
                current = Mathf.MoveTowards(current, 1f, 2f * Time.deltaTime);
                replacematerial.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, curve.Evaluate(current)));
            }, 0.5f);

            FunctionTimer.Create(() =>
            {
                gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
                gridVisualDictionary.Remove(gridItem);
                gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
                //material.SetFloat("_dissolveAmount", 0);
            }, 0.5f);

            Vector3 position = gridItemPosition.GetWorldPosition();
            switch (boosterID)
            {
                case 1:
                    ItemSO strippedItemSO = levelSO.strippedBoosterList[UnityEngine.Random.Range(0, levelSO.strippedBoosterList.Count)];
                    GridItem strippedGridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), strippedItemSO);
                    strippedGridItem.SetBoosterID(1);
                    gridItemPosition.SetGridItem(strippedGridItem);
                    break;
                case 2:
                    ItemSO wrappedItemSO = levelSO.wrappedBoosterList[UnityEngine.Random.Range(0, levelSO.wrappedBoosterList.Count)];
                    GridItem wrappedGridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), wrappedItemSO);
                    wrappedGridItem.SetBoosterID(2);
                    gridItemPosition.SetGridItem(wrappedGridItem);
                    break;
                case 3:
                    ItemSO powerItemSO = levelSO.powerBooster;
                    GridItem powerGridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), powerItemSO);
                    powerGridItem.SetBoosterID(3);
                    gridItemPosition.SetGridItem(powerGridItem);
                    break;
                default:
                    break;
            }

            OnRegisterBooster?.Invoke(this, new OnRegisterBoosterEventArgs { boosterID = boosterID, grid = grid, gridItem = gridItemPosition.GetGridItem()});
            // Dissolve Shader 

            Transform gridBoosterTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;

            Color color = gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color;
            color.a = 0;
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItemPosition.GetGridItem().GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;

           
            FunctionTimer.Create(() =>
            {
                float newCurrent = 0;
                FunctionUpdate.CreateUpdate(() =>
                {
                    if (newCurrent <= 1f)
                    {
                        newCurrent = Mathf.MoveTowards(newCurrent, 1, 2.5f * Time.deltaTime);
                        color.a = Mathf.Lerp(0, 1, curve.Evaluate(newCurrent));
                        gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
                    }
                }, 0.5f);
            }, 0.5f); 
          
            GridItemVisual newGridItemVisual = new GridItemVisual(gridBoosterTransform, gridItemPosition.GetGridItem());
            gridVisualDictionary[gridItemPosition.GetGridItem()] = newGridItemVisual; 
        }
    }

    private void GridLogic_OnTwoLayerCellDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GameObject effectObject = gridpooler.GetPooledGridObject(PoolType.CellEffect, gridItemPosition.GetWorldPosition() + new Vector3(0.5f, 0.5f), Quaternion.identity);
            if (effectObject != null)
            {
                ParticleSystem particleSystem = effectObject.GetComponent<ParticleSystem>();
                particleSystem.Play(); 
            }
            // Remove the first Layer of cell 
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null; 
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridItemPosition.ClearGridItem();

            // Instantiate last Layer of Cell 
            Vector3 position = gridItemPosition.GetWorldPosition();
            GridItem gridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), levelSO.CellLayerOne);
            gridItem.SetHasCell(true, GridItem.CellProtectionLayer.levelOne); 
            gridItemPosition.SetGridItem(gridItem);
            Transform gridCellTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;
            gridCellTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItem.GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridCellTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
            GridItemVisual newGridItemVisual = new GridItemVisual(gridCellTransform, gridItem);
            gridVisualDictionary[gridItem] = newGridItemVisual;

            FunctionTimer.Create(() =>
            {
               gridpooler.ReturnGridObjectToPool(PoolType.CellEffect, effectObject);
            }, 1f); 
        }
    }

    private void GridLogic_OnOneLayerCellDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GameObject effectObject = gridpooler.GetPooledGridObject(PoolType.CellEffect, gridItemPosition.GetWorldPosition() + new Vector3(0.5f, 0.5f), Quaternion.identity);
            if (effectObject != null)
            {
                ParticleSystem particleSystem = effectObject.GetComponent<ParticleSystem>();
                particleSystem.Play();
            }

            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.CellEffect, effectObject);
            }, 1f);
        }
    }

    private void GridLogic_OnLevelTwoBlockerDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GameObject effectObject = gridpooler.GetPooledGridObject(PoolType.BlockageEffect, gridItemPosition.GetWorldPosition() + new Vector3(0.5f, 0.5f), Quaternion.identity);
            if (effectObject != null)
            {
                ParticleSystem particleSystem = effectObject.GetComponent<ParticleSystem>();
                particleSystem.Play();
            }
            // Remove Level Two Layer from Blocker 
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridItemPosition.ClearGridItem(); 

            // Add Level One Layer in Blocker 
            Vector3 position = gridItemPosition.GetWorldPosition();
            GridItem gridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), levelSO.BlockerLevelOne);
            gridItem.SetIsBlocker(true, GridItem.BlockerType.levelOne); 
            gridItemPosition.SetGridItem(gridItem);
            Transform gridBlockerTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;
            gridBlockerTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItem.GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridBlockerTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
            GridItemVisual newGridItemVisual = new GridItemVisual(gridBlockerTransform, gridItem);
            gridVisualDictionary[gridItem] = newGridItemVisual;

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.BlockageEffect, effectObject);
            }, 1f);
        }
    }

    private void GridLogic_OnLevelOneBlockerDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GameObject effectObject = gridpooler.GetPooledGridObject(PoolType.BlockageEffect, gridItemPosition.GetWorldPosition() + new Vector3(0.5f, 0.5f), Quaternion.identity);
            if (effectObject != null)
            {
                ParticleSystem particleSystem = effectObject.GetComponent<ParticleSystem>();
                particleSystem.Play();
            }

            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);

            FunctionTimer.Create(() =>
            {
                gridpooler.ReturnGridObjectToPool(PoolType.BlockageEffect, effectObject);
            }, 1f);
        }
    }

    private void GridLogic_OnGridItemReplaced(object sender, GridLogic.NewlySpawnedItemEventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null)
        {
            GridItemVisual gridItemVisual = gridVisualDictionary[e.gridItem];
            // Item dissolve shader 
            Material material = gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().material;
            float current = 0;
            FunctionUpdate.CreateUpdate(() =>
            {
                current = Mathf.MoveTowards(current, 1f, 1.2f * Time.deltaTime);
                material.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, curve.Evaluate(current)));
            }, 0.98f); 
            FunctionTimer.Create(() =>
            {
                gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
                gridVisualDictionary.Remove(e.gridItem);              
                gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
                //material.SetFloat("_dissolveAmount", 0);
            }, 0.99f);
            //gridVisualDictionary.Remove(e.gridItem);
        }
    }

    private void DissolveMaterial(Material material, float current, float target)
    {
        Debug.Log("Start Dissolving");
       
        current = Mathf.MoveTowards(current, target, 5f * Time.deltaTime);
        material.SetFloat("_dissolveAmount", Mathf.Lerp(0, 1, current));
        Debug.Log(material.GetFloat("_dissolveAmount"));
    }

    private void GridLogic_OnCreateBoosterInstance(object sender, GridLogic.OnCreateBoosterInstanceEventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];

            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridItemPosition.ClearGridItem();

            Vector3 position = gridItemPosition.GetWorldPosition();
            switch (e.boosterID)
            {
                case 1:
                    ItemSO strippedBoosterItem = levelSO.strippedBoosterList[e.itemID - 1];
                    GridItem strippedGridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), strippedBoosterItem);
                    strippedGridItem.SetBoosterID(1);
                    gridItemPosition.SetGridItem(strippedGridItem);
                    break;
                case 2:
                    ItemSO wrappedBoosterItem = levelSO.wrappedBoosterList[e.itemID - 1];
                    GridItem wrappedGridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), wrappedBoosterItem);
                    wrappedGridItem.SetBoosterID(2);
                    gridItemPosition.SetGridItem(wrappedGridItem);
                    break;
                default:
                    break;
            }

            Transform gridBoosterTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItemPosition.GetGridItem().GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridBoosterTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
            GridItemVisual newGridItemVisual = new GridItemVisual(gridBoosterTransform, gridItemPosition.GetGridItem());
            gridVisualDictionary[gridItemPosition.GetGridItem()] = newGridItemVisual;
            OnRegisterBooster?.Invoke(this, new OnRegisterBoosterEventArgs { boosterID = e.boosterID, grid = grid, gridItem = gridItemPosition.GetGridItem() });
        }
    }

    private void GridLogic_OnBoosterCreated(object sender, GridLogic.OnBoosterCreatedEventArgs e)
    {
        OnRegisterBooster?.Invoke(this, new OnRegisterBoosterEventArgs { boosterID = e.gridItem.GetBoosterID(), grid = grid, gridItem = e.gridItem});
        Vector3 position = e.gridItemPosition.GetWorldPosition();
        Transform gridItemTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = e.gridItem.GetItem().sprite;
        Material material = new Material(dissolveMaterial);
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
        GridItemVisual gridItemVisual = new GridItemVisual(gridItemTransform, e.gridItem);
        gridVisualDictionary[e.gridItem] = gridItemVisual;
    }

    private void GridLogic_OnInsulatorDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridpooler.ReturnGridObjectToPool(PoolType.Insulator, gridItemVisual.GetTransform().gameObject);
        }
    }

    private void GridLogic_OnRegisterItemDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridpooler.ReturnGridObjectToPool(PoolType.Register, gridItemVisual.GetTransform().gameObject);
        }
    }

    private void InsulatorLogic_OnReplaceItemWithRegister(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            // Item dissolve shader 

            gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
            gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);
            gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
            gridItemPosition.ClearGridItem();

            Vector3 position = gridItemPosition.GetWorldPosition();
            GridItem gridItem = new GridItem(grid, gridItemPosition.GetX(), gridItemPosition.GetY(), levelSO.register);
            gridItem.SetRegister(true);
            gridItemPosition.SetGridItem(gridItem);
            Transform gridRegisterTransform = gridpooler.GetPooledGridObject(PoolType.Register, position, Quaternion.identity).transform;
            gridRegisterTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gridItem.GetItem().sprite;
            Material material = new Material(dissolveMaterial);
            gridRegisterTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
            GridItemVisual newGridItemVisual = new GridItemVisual(gridRegisterTransform, gridItem);
            gridVisualDictionary[gridItem] = newGridItemVisual;
        }
    }

    private void GridLogic_OnNewlyReplacedItem(object sender, GridLogic.NewlySpawnedItemEventArgs e)
    {
        Vector3 position = e.gridItemPosition.GetWorldPosition();
        Transform gridItemTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;
       
        Color color = gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().color;
        color.a = 0;
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = e.gridItem.GetItem().sprite;

        Material material = new Material(dissolveMaterial);
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;

        GridItemVisual gridItemVisual = new GridItemVisual(gridItemTransform, e.gridItem);
        gridVisualDictionary[e.gridItem] = gridItemVisual;
        float current = 0;

        FunctionUpdate.CreateUpdate(() =>
        {
            if (current <= 1f)
            {
                current = Mathf.MoveTowards(current, 1, 1.3f * Time.deltaTime);
                color.a = Mathf.Lerp(0, 1, curve.Evaluate(current));
                gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
                Debug.Log(color.a);
            }
        }, 0.99f);
    }

    private void GridLogic_OnGridItemDestroyed(object sender, EventArgs e)
    {
        GridItemPosition gridItemPosition = sender as GridItemPosition;
        if (gridItemPosition != null && gridItemPosition.HasGridItem())
        {
            int itemID = gridItemPosition.GetGridItem().GetItem().itemID;
            GridItemVisual gridItemVisual = gridVisualDictionary[gridItemPosition.GetGridItem()];
            if (gridItemVisual != null)
            {
                gridItemVisual.GetTransform().Find("Sprite").GetComponent<SpriteRenderer>().sprite = null;
                gridVisualDictionary.Remove(gridItemPosition.GetGridItem());
                gridpooler.ReturnGridObjectToPool(PoolType.Item, gridItemVisual.GetTransform().gameObject);

                if (!gridItemPosition.IsRegister() && !gridItemPosition.IsGridPositionInsulator() && !gridItemPosition.HasBooster())
                {
                    GameObject gridItemBlastObject = gridpooler.GetPooledGridObject(PoolType.ItemBlast, gridItemPosition.GetWorldPosition() + new Vector3(0.5f, 0.5f), Quaternion.identity);

                    // Item Destroy explosion 
                    ParticleSystem itemBlastPS = gridItemBlastObject.GetComponent<ParticleSystem>();
                    ParticleSystem itemBlastPSChild = gridItemBlastObject.transform.Find("sparks").GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule mainModule = itemBlastPSChild.main;
                    mainModule.startColor = levelSO.itemColorList[itemID - 1];

                    //ParticleSystemRenderer particleSystemRenderer = itemBlastPS.GetComponent<ParticleSystemRenderer>();
                    //particleSystemRenderer.material.SetColor("_Color", levelSO.itemColorList[itemID]);
                    itemBlastPS.Play();

                    // Play Item Blast SFX 
                    audioManager.PlayOneShotSound(
                        soundSFXCollection.ItemBlastSFX.AudioGroup,
                        soundSFXCollection.ItemBlastSFX.audioClip,
                        gridItemPosition.GetWorldPosition(),
                        soundSFXCollection.ItemBlastSFX.Volume,
                        soundSFXCollection.ItemBlastSFX.SpatialBlend
                        );

                    FunctionTimer.Create(() =>
                    {
                        gridpooler.ReturnGridObjectToPoolAfterDelay(PoolType.ItemBlast, gridItemBlastObject);
                    }, 0.5f);
                }
            }
        }
    }

    private void GridLogic_OnNewlySpawnedItem(object sender, GridLogic.NewlySpawnedItemEventArgs e)
    {
        Vector3 position = e.gridItemPosition.GetWorldPosition();
        position = new Vector3(position.x, 12);
        Transform gridItemTransform = gridpooler.GetPooledGridObject(PoolType.Item, position, Quaternion.identity).transform;
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = e.gridItem.GetItem().sprite;
        Material material = new Material(dissolveMaterial);
        gridItemTransform.Find("Sprite").GetComponent<SpriteRenderer>().material = material;
        GridItemVisual gridItemVisual = new GridItemVisual(gridItemTransform, e.gridItem);
        gridVisualDictionary[e.gridItem] = gridItemVisual;
    }
    #endregion GridLogic_Callbacks

    private void Update()
    {
        if (!isSetup) return;
        UpdateVisual();
        switch (state)
        {
            case State.WaitingForUser:             
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            Vector3 startTouchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
                            grid.GetXY(startTouchPosition, out startDragX, out startDragY);
                            //Debug.Log(startTouchPosition);
                            break;
                        case TouchPhase.Moved:
                            break;
                        case TouchPhase.Stationary:
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            Vector3 endTouchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
                            grid.GetXY(endTouchPosition, out int x, out int y);

                            if (x != startDragX)
                            {
                                // different X 
                                y = startDragY;
                                if (x < startDragX)
                                {
                                    x = startDragX - 1;
                                }
                                else
                                {
                                    x = startDragX + 1;
                                }
                            }
                            else
                            {
                                // different Y 
                                x = startDragX;
                                if (y < startDragY)
                                {
                                    y = startDragY - 1;
                                }
                                else
                                {
                                    y = startDragY + 1;
                                }
                            }

                            if (gridLogic.CanSwapGameplayBooster(startDragX, startDragY, x, y) || gridLogic.CanSwapGridItems(startDragX, startDragY, x, y))
                            {
                                TrySwap(startDragX, startDragY, x, y); 
                            }
                            //Debug.Log(endTouchPosition);
                            break;
                        default:
                            break;
                    }
                }

                if (gridLogic.IsSuffle())
                {
                    SetState(State.Suffle);
                }
              
                //if (canCreateRegister)
                //{
                //    insulatorLogic.BuildRegister();
                //    canCreateRegister = false;
                //}

                break;
            case State.Suffle:
                SetBusyState(globalTimer, () =>
                {
                    gridLogic.ReplaceAllGridItem();
                    SetBusyState(1f, () =>
                    {
                        gridLogic.SpawnItemInReplacedPositions();
                        SetBusyState(1f, () =>
                        {
                            SetState(State.TryingFindMatch);
                        }, () => true);
                    }, () => true);
                }, () => true);
                break; 
            case State.TryingFindMatch:
                StopAllItemHighlightedAnimation(); 
                if (gridLogic.TryFindMatchLinkAndDestroyThem())
                {             
                    SetState(State.Liazu); 
                }
                else
                {
                    if (canSpinWheel)
                    {
                        SetState(State.Wheel);
                    }
                    else
                    {
                        SetStateWaitingForUser();
                    }
                }
                break;
            case State.Liazu:
                if (gridLogic.HaveItemForDirectFall() || gridLogic.HaveAnyGridPositionForItemSpawn() || gridLogic.CanItemSideFall())
                {
                    SetBusyState(0.15f, () =>
                    {
                        itemFallObservation.text = "DirectFall";
                        DirectFall();
                        //gridLogic.ItemCompleteFall(); 
                        SetBusyState(0.15f, () =>
                        {
                            itemFallObservation.text = "ItemSpawn";
                            SpawnItems();
                            SetBusyState(firstTimer, () =>
                            {
                                itemFallObservation.text = "SideFall";
                                ItemSideFall();
                                SetBusyState(0.15f, () =>
                                {
                                    SetState(State.Liazu);
                                }, () => true);
                            }, gridLogic.CanItemSideFall);
                        }, gridLogic.HaveAnyGridPositionForItemSpawn);
                    }, gridLogic.HaveItemForDirectFall);
                }
                else
                {
                    SetState(State.TryingFindMatch); 
                }               
                break; 
            case State.Wheel:
                canSpinWheel = false;
                SetBusyState(globalTimer, () =>
                {
                    UpdateWheel();
                    SetBusyState(globalTimer, () =>
                    {
                        SetState(State.TryingFindMatch);
                    }, () => true);
                }, () => true);
                break;
            case State.GameOver:
                break;
            case State.Busy:
                if (busyFunc)
                {
                    busyTimer -= Time.deltaTime;
                    if (busyTimer <= 0f)
                    {
                        busyFunc = false;
                        OnBusyTimerElapsedAction?.Invoke();
                    }
                }
                else
                {
                    busyFunc = false;
                    OnBusyTimerElapsedAction?.Invoke();
                }
                break;
            default:
                break;
        }
    }

    private bool CompletefallCheckList()
    {
        if (gridLogic.HaveItemForDirectFall() || gridLogic.CanItemSideFall())
        {
            return true; 
        }
        return false; 
    }
    private void DirectFall()
    {
        if (gridLogic.HaveItemForDirectFall())
        {
            gridLogic.FallItemsIntoDirectEmptyPositions();
        }
    }

    private void SpawnItems()
    {
        if (gridLogic.HaveAnyGridPositionForItemSpawn())
        {
            gridLogic.SpawnItemsInMissingPositions();
        }
    }

    private void ItemSideFall()
    {
        if (gridLogic.CanItemSideFall())
        {
            gridLogic.SideFall();
        }
    }

    public void BestLiazu()
    {
        SetState(State.Busy);
        StopAllItemHighlightedAnimation();
        

        FuncTimerUpdater.Create(() =>
        {
            if (gridLogic.HaveItemForDirectFall())
            {
                gridLogic.FallItemsIntoDirectEmptyPositions();
            }

            FuncTimerUpdater.Create(() =>
            {
                if (gridLogic.HaveAnyGridPositionForItemSpawn())
                {
                    gridLogic.SpawnItemsInMissingPositions();
                }

                FuncTimerUpdater.Create(() =>
                {
                    if (gridLogic.CanItemSideFall())
                    {
                        gridLogic.SideFall();
                    }

                    FunctionTimer.Create(() =>
                    {
                        SetState(State.Liazu);
                    }, 0.5f);
                }, firstTimer, gridLogic.CanItemSideFall);
            }, firstTimer, gridLogic.HaveAnyGridPositionForItemSpawn);
        }, firstTimer, gridLogic.HaveItemForDirectFall);
    }

    private void UpdateVisual()
    {
        foreach (GridItem item in gridVisualDictionary.Keys)
        {
            gridVisualDictionary[item].Update();
        }
    }

    private void UpdateWheel()
    {
        foreach (GridItemPosition gridItemPosition in gridWheelDictionary.Keys)
        {
            gridWheelDictionary[gridItemPosition].SpinWheel(); 
        }
    }

    public void SetStateWaitingForUser()
    {
        if (gridLogic.TryIsGameOver())
        {
            SetState(State.GameOver);
            OnGameOver?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            canItemHighlighted = true; 
            if (canItemHighlighted)
            {
                HighligtedItem();
                canItemHighlighted = false;
            }
            SetState(State.WaitingForUser);
        }       
    }

    public void TrySwap(int startX, int startY, int endX, int endY)
    {
        GridItemPosition startGridItemPosition = grid.GetGridObject(startX, startY);
        GridItemPosition endGridItemPosition = grid.GetGridObject(endX, endY);

        int startItemBoosterID = startGridItemPosition.GetGridItem().GetBoosterID();
        int endItemBoosterID = endGridItemPosition.GetGridItem().GetBoosterID();

        if (startItemBoosterID == 3 || endItemBoosterID == 3)
        {
            SwapPowerBooster(startX, startY, endX, endY);
        }
        else if ((startItemBoosterID > 0 && startItemBoosterID < 3) && (endItemBoosterID > 0 && endItemBoosterID < 3))
        {
            SwapGameplayBooster(startX, startY, endX, endY);
        }
        else
        {
            SwapGridItems(startX, startY, endX, endY); 
        }
    }

    public void SwapGridItems(int startX, int startY, int endX, int endY)
    {
        gridLogic.SwapGridItems(startX, startY, endX, endY);
        gridLogic.UseMove();

        // Cached the swap position (start and end position) for creating booster at right position 
        OnSwapPositions?.Invoke(this, new OnSwapPositionsEventArgs { startX = startX, startY = startY, endX = endX, endY = endY });
        OnBeforeRegisterSpawned?.Invoke(this, EventArgs.Empty);

        SetBusyState(globalTimer, () =>
        {
            SetState(State.TryingFindMatch);
            MakeWheelPropertyTrue();
        }, () => true);
    }

    public void SwapGameplayBooster(int startX, int startY, int endX, int endY)
    {
        gridLogic.SwapGridItems(startX, startY, endX, endY);
        gridLogic.UseMove();

        SetBusyState(globalTimer, () =>
        {
            gridLogic.TryDestroyBooster(startX, startY, endX, endY);
            SetBusyState(0.5f, () =>
            {
                SetState(State.Liazu);
                MakeWheelPropertyTrue();
            }, () => true);  
        }, () => true); 

    }

    public void SwapPowerBooster(int startX, int startY, int endX, int endY)
    {
        gridLogic.SwapGridItems(startX, startY, endX, endY);
        gridLogic.UseMove();

        SetBusyState(globalTimer, () =>
        {
            gridLogic.TryDestroyBooster(startX, startY, endX, endY);
            SetBusyState(2.2f, () =>
            {
                SetState(State.Liazu);
                MakeWheelPropertyTrue();
            }, () => true); 
        }, () => true); 
    }

    public void SwapGridItemsWithBooster(int startX, int startY, int endX, int endY)
    {
        gridLogic.SwapGridItems(startX, startY, endX, endY);
        OnSwapPositions?.Invoke(this, new OnSwapPositionsEventArgs { startX = startX, startY = startY, endX = endX, endY = endY });

        SetBusyState(0.5f, () =>
        {
            SetState(State.Liazu);
        }, () => true); 
    }

    public bool WheelCheeklist()
    {
        if (gridWheelDictionary.Keys.Count < 1)
        {
            return false; 
        }
        return true; 
    }

    private void MakeWheelPropertyTrue()
    {
        canCreateRegister = true;
        canItemHighlighted = true;
        if (WheelCheeklist())
        {
            canSpinWheel = true;
        }
    }

    public void SetBusyState(float busyTimer, Action OnBusyTimerElapsedAction, Func<bool> busyFunc)
    {
        this.busyFunc = busyFunc();
        this.busyTimer = busyTimer;
        this.OnBusyTimerElapsedAction = OnBusyTimerElapsedAction;
        SetState(State.Busy);     
    }

    public void SetState(State state)
    {
        this.state = state;
        OnStateChanged?.Invoke(this, EventArgs.Empty); 
    }

    public State GetState() { return state; }

    public List<CircleCollider2D> GetAllInsulatorCollider()
    {
        List<CircleCollider2D> insulatorCollider = new List<CircleCollider2D>();
        foreach (GridItemVisual gridItemVisual in gridVisualDictionary.Values)
        {
            if (gridItemVisual.GetTransform().gameObject.GetComponent<CircleCollider2D>() != null)
            {
                CircleCollider2D circleCollider2D = gridItemVisual.GetTransform().gameObject.GetComponent<CircleCollider2D>();
                insulatorCollider.Add(circleCollider2D); 
            }
        }
        return insulatorCollider; 
    }

    public void TryDemageInsulator(Vector3 position)
    {
        grid.GetXY(position, out int x, out int y);
        if (gridLogic.IsValidPosition(x, y) && gridLogic.IsGridItemPositionInsulator(x, y))
        {
            GridItemPosition gridItemPosition = grid.GetGridObject(x, y);
            if (!gridItemPosition.IsEmpty())
            {
                GridItem gridItem = gridItemPosition.GetGridItem();
                if (int.TryParse(gridItem.GetInsulatorLifeText().text.ToString(), out int remainingLife))
                {
                    remainingLife -= 1;
                    gridItem.GetInsulatorLifeText().text = remainingLife.ToString();
                    if (remainingLife <= 0)
                    {
                        gridLogic.TryDestroyInsulator(gridItemPosition); 
                    }
                } 
            }
        }
    }

    public void TryDamageInsulator(GridItemPosition gridItemPosition, int damageAmount)
    {
        if (gridItemPosition.HasGridItem() && gridItemPosition.IsGridPositionInsulator())
        {
            GridItem gridItem = gridItemPosition.GetGridItem();
            if (int.TryParse(gridItem.GetInsulatorLifeText().text.ToString(), out int remainingLife))
            {
                remainingLife -= damageAmount;
                gridItem.GetInsulatorLifeText().text = remainingLife.ToString();
                if (remainingLife <= 0)
                {
                    gridLogic.TryDestroyInsulator(gridItemPosition);
                }
            }
        }
    }

    public void StopAllItemHighlightedAnimation()
    {
        foreach (GridItem item in gridVisualDictionary.Keys)
        {
            gridVisualDictionary[item].PlayHighlightedAnimation(false);
        }
    }

    public void HighligtedItem()
    {
        StopAllItemHighlightedAnimation(); 
        BestPossibleMove bestPossibleMove = new BestPossibleMove(gridLogic); 
        PossibleMove possibleMove = bestPossibleMove.FindBestPossibleMove();

        if (possibleMove != null)
        {
            GridItem startGridItem = grid.GetGridObject(possibleMove.GetStartX(), possibleMove.GetStartY()).GetGridItem();
            GridItem endGridItem = grid.GetGridObject(possibleMove.GetEndX(), possibleMove.GetEndY()).GetGridItem();

            gridVisualDictionary[startGridItem].PlayHighlightedAnimation(true);
            gridVisualDictionary[endGridItem].PlayHighlightedAnimation(true);
        }
    }
}
