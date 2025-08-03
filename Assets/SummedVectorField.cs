using UnityEngine;

// Simply sums a list of vector fields, to create a new vector field as a result.
public class SummedVectorField : MonoBehaviour, IVectorField
{
    IVectorField[] vectorFields;
    [SerializeField] GameObject[] vectorFieldGameObjects;

    void CacheRequiredObjectComponents()
    {
        vectorFields = new IVectorField[vectorFieldGameObjects.Length];

        for (int i = 0; i < vectorFieldGameObjects.Length; i++)
        {
            vectorFields[i] = vectorFieldGameObjects[i].GetComponent<IVectorField>();
        }
    }
    void Start()
    {
        // NOTE: this does not dynamically respond to updates to the vector field list after Start
        CacheRequiredObjectComponents();
    }

    // HACK for the editor to stay current. Not sure how to do this more nicely.
    void OnValidate()
    {
        CacheRequiredObjectComponents();
    }

    public Vector3 GetForce(Vector3 position)
    {
        // no fields, no force
        if (vectorFieldGameObjects.Length == 0) return Vector3.zero;

        Vector3 force = Vector3.zero;
        for (int i = 0; i < vectorFields.Length; i++)
        {
            force += vectorFields[i].GetForce(position);
        }

        return force;
    }
}
