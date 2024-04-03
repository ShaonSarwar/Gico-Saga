using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class FunctionTimer 
{
    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action OnUpdate;

        private void Update()
        {
            if (OnUpdate != null)
                OnUpdate(); 
        }
    }

    private static List<FunctionTimer> timerList;
    private static GameObject initGameObject; 

    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FunctionTimer_Global");
            timerList = new List<FunctionTimer>(); 
        }
    }


    private GameObject gameObject;
    private Action action;
    private float timer;
    private bool useUnscaleDeltaTime;
    private string functionName;
    private bool active; 

    public FunctionTimer(GameObject gameObject, Action action, float timer, bool useUnscaleDeltaTime, string functionName)
    {
        this.gameObject = gameObject;
        this.action = action;
        this.timer = timer;
        this.useUnscaleDeltaTime = useUnscaleDeltaTime;
        this.functionName = functionName;
    }

    public static FunctionTimer Create(Action action, float timer)
    {
        return Create(action, timer, "", false, false); 
    }

    public static FunctionTimer Create(Action action, float timer, string functionName)
    {
        return Create(action, timer, functionName, false, false); 
    }

    public static FunctionTimer Create(Action action, float timer, string functionName, bool useUnscaleDeltaTime)
    {
        return Create(action, timer, functionName, useUnscaleDeltaTime, false); 
    }

    public static FunctionTimer Create( Action action, float timer, string functionName, bool useUnscaleDeltaTime, bool stopAllWithSameName)
    {
        InitIfNeeded();
        if (stopAllWithSameName)
        {
            StopAllWithName(functionName); 
        }

        GameObject obj = new GameObject("Functiontimer_Object" + functionName, typeof(MonoBehaviourHook));
        FunctionTimer funcTimer = new FunctionTimer(obj, action, timer, useUnscaleDeltaTime, functionName);
        obj.GetComponent<MonoBehaviourHook>().OnUpdate = funcTimer.Update;
        timerList.Add(funcTimer);
        return funcTimer; 
    }

    public static void StopAllWithName(string functionName)
    {
        InitIfNeeded();
        for (int i = 0; i < timerList.Count; i++)
        {
            if (timerList[i].functionName == functionName)
            {
                timerList[i].DestroySelf();
                i--; 
            }
        }
    }

    public static void RemoveTimer(FunctionTimer funcTimer)
    {
        InitIfNeeded();
        timerList.Remove(funcTimer); 
    }

    public static List<FunctionTimer> GetTimerList()
    {
        return timerList; 
    }

    private void DestroySelf()
    {
        RemoveTimer(this);
        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject); 
        }
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

        if (timer <= 0)
        {
            action();
            DestroySelf(); 
        }
    }
}
