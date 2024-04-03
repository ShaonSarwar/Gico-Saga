using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class FunctionLiazuUpdater 
{
    private class MonoBehaviourHook : MonoBehaviour
    {

        public Action OnUpdate;

        private void Update()
        {
            if (OnUpdate != null) OnUpdate();
        }

    }

    private static List<FunctionLiazuUpdater> updaterList; // Holds a reference to all active updaters
    private static GameObject initGameObject; // Global game object used for initializing class, is destroyed on scene change

    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FunctionUpdater_Global");
            updaterList = new List<FunctionLiazuUpdater>();
        }
    }

    public static FunctionLiazuUpdater Create(Action action1, Action action2, Func<bool> updateFunc1, Func<bool> updateFunc2)
    {
        return Create(action1, action2, "", true, updateFunc1, updateFunc2);
    }

    public static FunctionLiazuUpdater Create(Action action1, Action action2, string functionName, bool active, Func<bool> updateFunc1, Func<bool> updateFunc2)
    {
        InitIfNeeded();
        GameObject obj = new GameObject("FunctionLiazuUpdate" + functionName, typeof(MonoBehaviourHook));
        FunctionLiazuUpdater functionLiazuUpdate = new FunctionLiazuUpdater(obj, action1, action2, functionName, active, updateFunc1, updateFunc2);
        obj.GetComponent<MonoBehaviourHook>().OnUpdate = functionLiazuUpdate.Update;
        updaterList.Add(functionLiazuUpdate);
        return functionLiazuUpdate;
    }

    private GameObject gameObject;
    private Action action1;
    private Action action2;
    private string functionName;
    private bool active;
    private Func<bool> updateFunc1;
    private Func<bool> updateFunc2;

    private FunctionLiazuUpdater(GameObject gameObject, Action action1, Action action2, string functionName, bool active, Func<bool> updateFunc1, Func<bool> updateFunc2)
    {
        this.gameObject = gameObject;
        this.action1 = action1;
        this.action2 = action2;
        this.functionName = functionName;
        this.active = active;
        this.updateFunc1 = updateFunc1;
        this.updateFunc2 = updateFunc2;
    }

    private static void RemoveUpdater(FunctionLiazuUpdater funcUpdater)
    {
        InitIfNeeded();
        updaterList.Remove(funcUpdater);
    }

    private void Update()
    {
        if (!active) return;
        if (updateFunc1() && !updateFunc2())
        {
            action1();
        }

        if (!updateFunc1() && updateFunc2())
        {
            action2();
        }

        if (!updateFunc1() && !updateFunc2())
        {
            DestroySelf();
        }
    }

    public void DestroySelf()
    {
        RemoveUpdater(this);
        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
