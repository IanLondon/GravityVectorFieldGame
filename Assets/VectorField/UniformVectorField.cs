using UnityEngine;

public class UniformVectorField : MonoBehaviour, IVectorField
{
    Bounds cachedBounds;
    [SerializeField] bool useLocalTransformToMakeBoundingBox = false;
    [SerializeField] Vector3 force = Vector3.zero;

    Bounds GetBounds() {
        var x = Mathf.Abs(transform.localScale.x);
        var y = Mathf.Abs(transform.localScale.y);
        var z = Mathf.Abs(transform.localScale.z);

        return new Bounds(transform.position, new Vector3(x, y, z));
    }

    void Awake() {
        cachedBounds = GetBounds();
    }

    public Vector3 GetForce(Vector3 position)
    {
        if (useLocalTransformToMakeBoundingBox)
        {        
            if (cachedBounds.Contains(position))
            {
                return force;
            } else
            {
                return Vector3.zero;
            }
        } else
        {
            return force;
        }
    }

    void OnDrawGizmos()
    {
        if (useLocalTransformToMakeBoundingBox)
        {
            // HACK to make gizmos work
            cachedBounds = GetBounds();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (useLocalTransformToMakeBoundingBox)
        {
            Gizmos.DrawWireCube(transform.position, cachedBounds.size);
        }
    }
}
