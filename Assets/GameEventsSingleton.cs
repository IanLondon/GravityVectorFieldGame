using System;
using UnityEngine;

public class GameEventsSingleton : MonoBehaviour
{
    public static GameEventsSingleton Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<PushOffState> OnPushOffStateChange;
    public void PushOffStateChange(PushOffState state)
    {
        if (OnPushOffStateChange != null)
        {
            OnPushOffStateChange(state);
        }
    }

    public event Action<GameObject> OnCheckpointActivated;
    public void CheckpointActivated(GameObject checkpoint)
    {
        Debug.Log("checkpoint '" + checkpoint.name + "' activated");
        if (OnCheckpointActivated != null)
        {
            OnCheckpointActivated(checkpoint);
        }
    }

    public event Action<PlayerDeathReason> OnKillPlayer;
    public void KillPlayer(PlayerDeathReason reason)
    {
        Debug.Log("player died! " + reason);
        if (OnKillPlayer != null)
        {
            OnKillPlayer(reason);
        }
    }

    public event Action<Vector3, Quaternion> OnRespawn;
    public void Respawn(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (OnRespawn != null)
        {
            OnRespawn(spawnPosition, spawnRotation);
        }
    }

    public event Action<bool> OnPlayerCanWalk;
    public void PlayerCanWalk(bool canWalk)
    {
        //Debug.Log("player can walk? " + canWalk);
        if (OnPlayerCanWalk != null)
        {
            OnPlayerCanWalk(canWalk);
        }
    }
}

public enum PlayerDeathReason
{
    ManualRespawn,
    DeadlyCollision
}

