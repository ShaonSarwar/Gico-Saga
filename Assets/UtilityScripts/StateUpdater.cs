using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class StateUpdater : MonoBehaviour
{
    public enum State { Busy, None}

    private State state;
    private float busyTimer;
    private Action OnBusyTimerElapsedTimer;
    private bool busyFunc; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.Busy:
                if (busyFunc)
                {
                    busyTimer -= Time.deltaTime;
                    if (busyTimer >= 0)
                    {
                        OnBusyTimerElapsedTimer?.Invoke();
                    }
                    else
                    {
                        SetState(State.None);
                    }
                }             
                break;
            case State.None:
                break;
            default:
                break;
        }
    }

    public void SetState(State state)
    {
        this.state = state; 
    }

    public State GetState() { return state; }

    public void SetBusyTimer(float busyTimer, Action OnBusyTimerElapsedTimer, Func<bool> updateFunc)
    {
        SetState(State.Busy);
        this.busyTimer = busyTimer;
        this.OnBusyTimerElapsedTimer = OnBusyTimerElapsedTimer;
        this.busyFunc = updateFunc(); 
    }
}
