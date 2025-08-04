using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    Rigidbody rigidBody;
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        GameEventsSingleton.instance.OnRespawn += OnRespawn;
    }

    void OnRespawn(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.linearVelocity = Vector3.zero;
        Camera.main.transform.localRotation = Quaternion.identity; // TODO this doesn't change the tilt bc that script isn't set
    }
}
