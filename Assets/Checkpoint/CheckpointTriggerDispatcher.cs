using UnityEngine;

// NOTE: the name serves as its identifier.
public class CheckpointTriggerDispatcher : MonoBehaviour
{
    bool active;
    MeshRenderer meshRenderer;
    [SerializeField] GameObject platform;
    [SerializeField] Material activeMaterial;
    [SerializeField] Material inactiveMaterial;
    [SerializeField] bool initiallyActive;
    
    void Awake()
    {
        active = initiallyActive;
        meshRenderer = platform.GetComponent<MeshRenderer>();
        meshRenderer.material = active ? activeMaterial : inactiveMaterial;
    }
    void Start()
    {
        GameEventsSingleton.instance.OnCheckpointActivated += OnCheckpointActivated;
    }

    void OnCheckpointActivated(GameObject checkpoint)
    {
        if (gameObject.GetInstanceID() == checkpoint.GetInstanceID())
        {
            ActivateCheckpoint();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        // HACK: detect "if player" some more robust way. Maybe use collider layers, since player is the only thing that will collide with checkpoint that we care about.
        if (!active && other.gameObject.name == "Player")
        {
            GameEventsSingleton.instance.CheckpointActivated(gameObject);
        }
    }

    void ActivateCheckpoint()
    {
        active = true;
        meshRenderer.material = activeMaterial;
    }
}
