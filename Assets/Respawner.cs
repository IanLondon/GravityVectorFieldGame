using UnityEngine;
using UnityEngine.InputSystem;

public class Respawner : MonoBehaviour
{
    InputAction respawnAction;
    GameObject activeCheckpoint;
    [Tooltip("Must be given the name of the initial active checkpoint")]
    [SerializeField] string initialActiveCheckpointName;
    
    [SerializeField] Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    private void Awake()
    {
        respawnAction = InputSystem.actions.FindAction("Respawn");

        var initialActiveCheckpoint = GameObject.Find(initialActiveCheckpointName);
        activeCheckpoint = initialActiveCheckpoint;
    }
    void Start()
    {
        GameEventsSingleton.instance.OnCheckpointActivated += OnCheckpointActivated;
        GameEventsSingleton.instance.OnKillPlayer += OnKillPlayer;
    }

    void Update()
    {
        if (respawnAction.WasPressedThisFrame())
        {
            GameEventsSingleton.instance.KillPlayer(PlayerDeathReason.ManualRespawn);
        }
    }

    void OnCheckpointActivated(GameObject checkpoint)
    {
        activeCheckpoint = checkpoint;
    }

    void OnKillPlayer(PlayerDeathReason reason)
    {
        var respawnPosition = activeCheckpoint.transform.position + activeCheckpoint.transform.TransformDirection(spawnOffset);
        GameEventsSingleton.instance.Respawn(respawnPosition, activeCheckpoint.transform.rotation);
    }
}
