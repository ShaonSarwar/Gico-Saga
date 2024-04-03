using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class FuncTimerUpdater 
{
    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action OnUpdate;

        private void Update()
        {
            OnUpdate?.Invoke();
        }
    }

    private static List<FuncTimerUpdater> timerUpdaterList;
    private static GameObject initGameObject;

    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FuncTimerUpdater_Global");
            timerUpdaterList = new List<FuncTimerUpdater>();
        }
    }

    public static FuncTimerUpdater Create(Action action, float timer, Func<bool> updateFunc)
    {
        return Create(action, timer, updateFunc, false, "", false);
    }

    public static FuncTimerUpdater Create(Action action, float timer, Func<bool> updateFunc, bool useUnscaleDeltaTime, string functionName, bool stopAllWithSameName)
    {
        InitIfNeeded();
        if (stopAllWithSameName)
        {
            StopAllTimerUpdatersWithName(functionName);
        }

        GameObject gameObject = new GameObject("FuncTimerUpdater_Object" + functionName, typeof(MonoBehaviourHook));
        FuncTimerUpdater funcTimerUpdater = new FuncTimerUpdater(gameObject, action, timer, updateFunc, useUnscaleDeltaTime, functionName);
        gameObject.GetComponent<MonoBehaviourHook>().OnUpdate = funcTimerUpdater.Update;
        timerUpdaterList.Add(funcTimerUpdater);
        return funcTimerUpdater;
    }

    private void RemoveTimerUpdater(FuncTimerUpdater funcTimerUpdater)
    {
        InitIfNeeded();
        timerUpdaterList.Remove(funcTimerUpdater);
    }

    private void DestroyTimerUpdater(FuncTimerUpdater funcTimerUpdater)
    {
        InitIfNeeded();
        if (funcTimerUpdater != null)
        {
            funcTimerUpdater.DestroySelf();
        }
    }

    public static void StopTimerUpdaterWithName(string functionName)
    {
        InitIfNeeded();
        for (int i = 0; i < timerUpdaterList.Count; i++)
        {
            if (timerUpdaterList[i].functionName == functionName)
            {
                timerUpdaterList[i].DestroySelf();
                return;
            }
        }
    }

    public static void StopAllTimerUpdatersWithName(string functionName)
    {
        InitIfNeeded();
        for (int i = 0; i < timerUpdaterList.Count; i++)
        {
            if (timerUpdaterList[i].functionName == functionName)
            {
                timerUpdaterList[i].DestroySelf();
                i--;
            }
        }
    }

    public static List<FuncTimerUpdater> GetUpdaterTimerList()
    {
        return timerUpdaterList; 
    }

    private GameObject gameObject;
    private Action action;
    private float timer;
    private Func<bool> updateFunc;
    private bool useUnscaleDeltaTime;
    private string functionName;

    private FuncTimerUpdater(GameObject gameObject, Action action, float timer, Func<bool> updateFunc, bool useUnscaleDeltaTime, string functionName)
    {
        this.gameObject = gameObject;
        this.action = action;
        this.timer = timer;
        this.updateFunc = updateFunc;
        this.useUnscaleDeltaTime = useUnscaleDeltaTime;
        this.functionName = functionName;
    }

    private void Update()
    {
        if (updateFunc())
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
        else
        {
            action();
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        RemoveTimerUpdater(this);
        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
