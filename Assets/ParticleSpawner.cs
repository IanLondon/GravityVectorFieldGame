using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    GameObject[] particlePool;
    GameObject particleParent;
    float timeSinceLastRecycle;
    int recycleIndex = 0;
    [SerializeField] GameObject particlePrefab;
    [SerializeField] float xPositionRange;
    [SerializeField] float yPositionRange;
    [SerializeField] float zPositionRange;
    [SerializeField] bool activateOnInstantiate = true;
    [Range(0,1000)][SerializeField] int poolSize = 100;
    [SerializeField] float particleRecycleFrequencySecs = 0.05f;

    Vector3 RandomPosition()
    {
        var offset = new Vector3(Random.Range(0, xPositionRange), Random.Range(0, yPositionRange), Random.Range(0, zPositionRange));
        // using particleParent instead of having this ParticleSpawner GameObject means we can change the spawn origin
        // by moving the ParticleSpawner around without affecting the position of the existing particles
        return transform.position + offset;
    }

    void InstatiateParticles()
    {
        particlePool = new GameObject[poolSize];
        for (int i = 0; i < particlePool.Length; i++)
        {

            particlePool[i] = Instantiate(particlePrefab, RandomPosition(), Quaternion.identity, particleParent.transform);
            if (activateOnInstantiate)
            {
                particlePool[i].SetActive(true);
            }
        }
    }
    void Awake()
    {
        particleParent = new GameObject("ParticleParent");
        InstatiateParticles();
    }

    void ResetParticle (GameObject particle)
    {
        particle.transform.position = RandomPosition();
        // TODO cache this??
        var rb = particle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        timeSinceLastRecycle += Time.deltaTime;
        if (timeSinceLastRecycle >= particleRecycleFrequencySecs)
        {
            ResetParticle(particlePool[recycleIndex]);
            timeSinceLastRecycle = 0f;
            
            recycleIndex++;
            if (recycleIndex >= particlePool.Length)
            {
                recycleIndex = 0;
            }
        } 
    }
}
