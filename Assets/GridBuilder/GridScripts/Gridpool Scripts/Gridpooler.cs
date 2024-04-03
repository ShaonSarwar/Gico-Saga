using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PoolType
{
    Item,
    Insulator, 
    Register, 
    Background,
    Glass,
    ItemBlast, 
    Splash, 
    Stripped, 
    Wrapped, 
    Power, 
    BlockageEffect, 
    CellEffect
}
public class Gridpooler : MonoBehaviour
{
    // Pool Object Prefabs 
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject insulatorPrefab;
    [SerializeField] private GameObject registerPrefab; 
    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private GameObject glassPrefab;
    [SerializeField] private GameObject itemBlastPrefab; 
    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private GameObject strippedPrefab;
    [SerializeField] private GameObject wrappedPrefab;
    [SerializeField] private GameObject powerPrefab;
    [SerializeField] private GameObject blockageEffectPrefab;
    [SerializeField] private GameObject cellEffectPrefab; 

    // Pool Objects Holder(Transform) 
    [SerializeField] private Transform itemHolder;
    [SerializeField] private Transform insulatorHolder;
    [SerializeField] private Transform registerHolder; 
    [SerializeField] private Transform backgroundHolder;
    [SerializeField] private Transform glassHolder;
    [SerializeField] private Transform itemBlastHolder; 
    [SerializeField] private Transform splashHolder;
    [SerializeField] private Transform strippedHolder;
    [SerializeField] private Transform wrappedHolder;
    [SerializeField] private Transform powerHolder;
    [SerializeField] private Transform blockageEffectHolder;
    [SerializeField] private Transform cellEffectHolder; 

    // Pool Objects 
    private GameObject item;
    private GameObject insulator;
    private GameObject register; 
    private GameObject background;
    private GameObject glass;
    private GameObject itemBlast; 
    private GameObject splash;
    private GameObject stripped;
    private GameObject wrapped;
    private GameObject power;
    private GameObject blockageEffect;
    private GameObject cellEffect; 

    // How much pool for an object 
    [SerializeField] private int poolAmount;

    // Pool Queues 
    private Queue<GameObject> itemQueue = new Queue<GameObject>();
    private Queue<GameObject> insulatorQueue = new Queue<GameObject>();
    private Queue<GameObject> registerQueue = new Queue<GameObject>(); 
    private Queue<GameObject> backgroundQueue = new Queue<GameObject>();
    private Queue<GameObject> glassQueue = new Queue<GameObject>();
    private Queue<GameObject> itemBlastQueue = new Queue<GameObject>(); 
    private Queue<GameObject> splashQueue = new Queue<GameObject>();
    private Queue<GameObject> strippedQueue = new Queue<GameObject>();
    private Queue<GameObject> wrappedQueue = new Queue<GameObject>();
    private Queue<GameObject> powerQueue = new Queue<GameObject>();
    private Queue<GameObject> blockageEffectQueue = new Queue<GameObject>();
    private Queue<GameObject> cellEffectQueue = new Queue<GameObject>(); 

    private void Awake()
    {
        itemQueue.Clear();
        insulatorQueue.Clear();
        registerQueue.Clear(); 
        backgroundQueue.Clear();
        glassQueue.Clear();
        itemBlastQueue.Clear(); 
        splashQueue.Clear();
        strippedQueue.Clear();
        wrappedQueue.Clear();
        powerQueue.Clear();
        blockageEffectQueue.Clear();
        cellEffectQueue.Clear(); 

        ConfigureGridPool(itemPrefab, PoolType.Item);
        ConfigureGridPool(insulatorPrefab, PoolType.Insulator);
        ConfigureGridPool(registerPrefab, PoolType.Register); 
        ConfigureGridPool(backgroundPrefab, PoolType.Background);
        ConfigureGridPool(glassPrefab, PoolType.Glass);
        ConfigureGridPool(itemBlastPrefab, PoolType.ItemBlast); 
        ConfigureGridPool(splashPrefab, PoolType.Splash);
        ConfigureGridPool(strippedPrefab, PoolType.Stripped);
        ConfigureGridPool(wrappedPrefab, PoolType.Wrapped);
        ConfigureGridPool(powerPrefab, PoolType.Power);
        ConfigureGridPool(blockageEffectPrefab, PoolType.BlockageEffect);
        ConfigureGridPool(cellEffectPrefab, PoolType.CellEffect); 
    }

    private void ConfigureGridPool(GameObject objectPrefab, PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.Item:
                ConfigureGridObject(objectPrefab, poolAmount, itemHolder, itemQueue);
                item = objectPrefab;
                break;
            case PoolType.Insulator:
                ConfigureGridObject(objectPrefab, poolAmount, insulatorHolder, insulatorQueue);
                insulator = objectPrefab;
                break;
            case PoolType.Register:
                ConfigureGridObject(objectPrefab, poolAmount, registerHolder, registerQueue);
                register = objectPrefab;
                break; 
            case PoolType.Background:
                ConfigureGridObject(objectPrefab, poolAmount, backgroundHolder, backgroundQueue);
                background = objectPrefab;
                break;
            case PoolType.Glass:
                ConfigureGridObject(objectPrefab, poolAmount, glassHolder, glassQueue);
                glass = objectPrefab;
                break;
            case PoolType.ItemBlast:
                ConfigureGridObject(objectPrefab, poolAmount, itemBlastHolder, itemBlastQueue);
                itemBlast = objectPrefab;
                break;
            case PoolType.Splash:
                ConfigureGridObject(objectPrefab, poolAmount, splashHolder, splashQueue);
                splash = objectPrefab;
                break;
            case PoolType.Stripped:
                ConfigureGridObject(objectPrefab, poolAmount, strippedHolder, strippedQueue);
                stripped = objectPrefab; 
                break;
            case PoolType.Wrapped:
                ConfigureGridObject(objectPrefab, poolAmount, wrappedHolder, wrappedQueue);
                wrapped = objectPrefab; 
                break;
            case PoolType.Power:
                ConfigureGridObject(objectPrefab, poolAmount, powerHolder, powerQueue);
                power = objectPrefab; 
                break;
            case PoolType.BlockageEffect:
                ConfigureGridObject(objectPrefab, poolAmount, blockageEffectHolder, blockageEffectQueue);
                blockageEffect = objectPrefab; 
                break;
            case PoolType.CellEffect:
                ConfigureGridObject(objectPrefab, poolAmount, cellEffectHolder, cellEffectQueue);
                cellEffect = objectPrefab; 
                break; 
        }
    }

    private void ConfigureGridObject(GameObject objectPrefab, int poolAmount, Transform holder, Queue<GameObject> objectQueue)
    {
        for (int i = 0; i < poolAmount; i++)
        {
            GameObject objectToPooled = Instantiate(objectPrefab);
            objectToPooled.transform.parent = holder;
            objectToPooled.SetActive(false);
            objectQueue.Enqueue(objectToPooled);
        }
    }

    public GameObject GetPooledGridObject(PoolType poolType, Vector3 position, Quaternion quaternion)
    {
        switch (poolType)
        {
            case PoolType.Item:
                return GetGridObject(itemQueue, position, quaternion);
            case PoolType.Insulator:
                return GetGridObject(insulatorQueue, position, quaternion);
            case PoolType.Register:
                return GetGridObject(registerQueue, position, quaternion); 
            case PoolType.Background:
                return GetGridObject(backgroundQueue, position, quaternion);
            case PoolType.Glass:
                return GetGridObject(glassQueue, position, quaternion);
            case PoolType.ItemBlast:
                return GetGridObject(itemBlastQueue, position, quaternion); 
            case PoolType.Splash:
                return GetGridObject(splashQueue, position, quaternion);
            case PoolType.Stripped:
                return GetGridObject(strippedQueue, position, quaternion);
            case PoolType.Wrapped:
                return GetGridObject(wrappedQueue, position, quaternion);
            case PoolType.Power:
                return GetGridObject(powerQueue, position, quaternion);
            case PoolType.BlockageEffect:
                return GetGridObject(blockageEffectQueue, position, quaternion);
            case PoolType.CellEffect:
                return GetGridObject(cellEffectQueue, position, quaternion); 
        }
        return null;
    }

    private GameObject GetGridObject(Queue<GameObject> queue, Vector3 position, Quaternion quaternion)
    {
        if (queue.Count > 0)
        {
            GameObject pooledObject = queue.Dequeue();
            pooledObject.SetActive(true);
            pooledObject.transform.position = position;
            pooledObject.transform.rotation = quaternion;
            return pooledObject;
        }
        return null;
    }

    public void ReturnGridObjectToPool(PoolType poolType, GameObject gridObject)
    {
        if (gridObject.activeInHierarchy)
        {
            gridObject.SetActive(false);
        }

        switch (poolType)
        {
            case PoolType.Item:
                itemQueue.Enqueue(gridObject);
                break;
            case PoolType.Insulator:
                insulatorQueue.Enqueue(gridObject);
                break;
            case PoolType.Register:
                registerQueue.Enqueue(gridObject);
                break; 
            case PoolType.Background:
                backgroundQueue.Enqueue(gridObject);
                break;
            case PoolType.Glass:
                glassQueue.Enqueue(gridObject);
                break;
            case PoolType.ItemBlast:
                itemBlastQueue.Enqueue(gridObject);
                break; 
            case PoolType.Splash:
                splashQueue.Enqueue(gridObject);
                break;
            case PoolType.Stripped:
                strippedQueue.Enqueue(gridObject);
                break;
            case PoolType.Wrapped:
                wrappedQueue.Enqueue(gridObject);
                break;
            case PoolType.Power:
                powerQueue.Enqueue(gridObject);
                break;
            case PoolType.BlockageEffect:
                blockageEffectQueue.Enqueue(gridObject);
                break;
            case PoolType.CellEffect:
                cellEffectQueue.Enqueue(gridObject);
                break; 
        }
    }

    public void ReturnGridObjectToPoolAfterDelay(PoolType poolType, GameObject gridObject)
    {
        if (gridObject.activeInHierarchy)
        {
            gridObject.SetActive(false);
        }

        switch (poolType)
        {
            case PoolType.Item:
                itemQueue.Enqueue(gridObject);
                break;
            case PoolType.Insulator:
                insulatorQueue.Enqueue(gridObject);
                break;
            case PoolType.Register:
                registerQueue.Enqueue(gridObject);
                break; 
            case PoolType.Background:
                backgroundQueue.Enqueue(gridObject);
                break;
            case PoolType.Glass:
                glassQueue.Enqueue(gridObject);
                break;
            case PoolType.ItemBlast:
                itemBlastQueue.Enqueue(gridObject);
                break; 
            case PoolType.Splash:
                splashQueue.Enqueue(gridObject);
                break;
            case PoolType.Stripped:
                strippedQueue.Enqueue(gridObject);
                break;
            case PoolType.Wrapped:
                wrappedQueue.Enqueue(gridObject);
                break;
            case PoolType.Power:
                powerQueue.Enqueue(gridObject);
                break;
            case PoolType.BlockageEffect:
                blockageEffectQueue.Enqueue(gridObject);
                break;
            case PoolType.CellEffect:
                cellEffectQueue.Enqueue(gridObject);
                break; 
        }
    }
}
