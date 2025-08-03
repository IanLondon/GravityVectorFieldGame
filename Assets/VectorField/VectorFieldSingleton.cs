using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

// A Singleton that sums all the vector fields that are registered with it.
public class VectorFieldSingleton : MonoBehaviour, IVectorField
{
    HashSet<IVectorField> vectorFields = new HashSet<IVectorField>();
    static VectorFieldSingleton instance;
    //void Awake()
    //{
    //    instance = this;
    //}

    public static VectorFieldSingleton Instance
    {
        get {
            if (instance == null)
            {
                // allow loading before Awake, eg for OnDrawGizmos
                instance = FindAnyObjectByType<VectorFieldSingleton>();
            }
            return instance;
        }
    }

    void Reset()
    {
        vectorFields.Clear();
    }

    /// <summary>
    /// It is ok to activate multiple times because we don't allow duplicates.
    /// </summary>
    public void ActivateField(IVectorField field)
    {
        vectorFields.Add(field);
    }

    public void DeactivateField(IVectorField field)
    {
        vectorFields.Remove(field);
    }

    public Vector3 GetForce(Vector3 position)
    {
        // no fields, no force
        //if (vectorFields) return Vector3.zero;

        Vector3 force = Vector3.zero;
        foreach (IVectorField field in vectorFields)
        {
            force += field.GetForce(position);
        }

        return force;
    }
}
