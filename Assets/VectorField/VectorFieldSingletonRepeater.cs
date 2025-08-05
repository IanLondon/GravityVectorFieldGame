using UnityEngine;

// Gets the VectorFieldSingleton, avoiding cross-scene references of GameObject fields :(
public class VectorFieldSingletonRepeater : MonoBehaviour, IVectorField
{
    public Vector3 GetForce(Vector3 position)
    {
        return VectorFieldSingleton.Instance.GetForce(position);
    }
}
