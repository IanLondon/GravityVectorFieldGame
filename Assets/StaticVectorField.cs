using UnityEngine;

public class StaticVectorField : MonoBehaviour, IVectorField
{
    [SerializeField] Vector3 m_Force = Vector3.zero;


    public Vector3 GetForce(Vector3 position)
    {
        // ignoring position right now
        return m_Force;
    }
}
