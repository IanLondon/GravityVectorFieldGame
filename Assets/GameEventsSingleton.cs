using System;
using UnityEngine;

public class GameEventsSingleton : MonoBehaviour
{
    public static GameEventsSingleton instance;

    void Awake()
    {
        instance = this;
    }

    public event Action<PushOffState> OnPushOffStateChange;
    public void PushOffStateChange(PushOffState state)
    {
        if (OnPushOffStateChange != null)
        {
            OnPushOffStateChange(state);
        }
    }
}
