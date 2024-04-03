using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class FunctionUpdate 
{
    public static FunctionUpdate CreateUpdate(Action action, float timer)
    {
        return CreateUpdate(action, timer, "", false, false); 
    }

    public static FunctionUpdate CreateUpdate(Action action, float timer, string functionName)
    {
        return CreateUpdate(action, timer, functionName, false, false); 
    }

    public static FunctionUpdate CreateUpdate(Action action, float timer, string functionName, bool useUnscaleDeltaTime)
    {
        return CreateUpdate(action, timer, functionName, useUnscaleDeltaTime, false); 
    }

    public static FunctionUpdate CreateUpdate(Action action, float timer, string functionName, bool useUnscaleDeltaTime, bool stopAllWithSameName)
    {
        InitIfNeeded();
        if (stopAllWithSameName)
        {
            StopAllWithSameName(functionName); 
        }

        GameObject obj = new GameObject("FunctionUpdate_Object" + functionName, typeof(MonoBehaviourHook));
        FunctionUpdate functionUpdate = new FunctionUpdate(obj, action, timer, useUnscaleDeltaTime, functionName);
        obj.GetComponent<MonoBehaviourHook>().OnUpdate = functionUpdate.Update;
        updateList.Add(functionUpdate);
        return functionUpdate; 
    }

    private static List<FunctionUpdate> updateList;
    private static GameObject initGameObject;

    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FunctionUpdate_Global");
            updateList = new List<FunctionUpdate>(); 
        }
    }

    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action OnUpdate;
        private void Update()
        {
            OnUpdate?.Invoke();
        }
    }


    private GameObject gameObject;
    private Action action;
    private float timer;
    private bool useUnscaleDeltaTime;
    private string functionName;

    private FunctionUpdate(GameObject gameObject, Action action, float timer, bool useUnscaleDeltaTime, string functionName)
    {
        this.gameObject = gameObject;
        this.action = action;
        this.timer = timer;
        this.useUnscaleDeltaTime = useUnscaleDeltaTime;
        this.functionName = functionName;
    }

    public static void StopAllWithSameName(string functionName)
    {
        InitIfNeeded(); 
        for (int i = 0; i < updateList.Count; i++)
        {
            if (updateList[i].functionName == functionName)
            {
                updateList[i].DestroySelf();
                i--; 
            }
        }
    }

    private static void RemoveUpdate(FunctionUpdate functionUpdate)
    {
        InitIfNeeded();
        updateList.Remove(functionUpdate); 
    }

    private void DestroySelf()
    {
        RemoveUpdate(this);
        if (gameObject != null) UnityEngine.Object.Destroy(gameObject); 
    }

    private void Update()
    {
        if (useUnscaleDeltaTime)
        {
            timer -= Time.unscaledDeltaTime;
        }
        else
        {
            timer -= Time.deltaTime;
        }

        if (timer >= 0)
        {
            action();
        }
        else
        {
            DestroySelf();
        }
    }
}
