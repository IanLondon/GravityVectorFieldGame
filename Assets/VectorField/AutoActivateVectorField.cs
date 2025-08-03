using UnityEngine;

// Activate and deactivate vector field in the singleton field with lifecycle events
public class AutoActivateVectorField : MonoBehaviour
{
    IVectorField vectorField;
    [SerializeField] bool addDuringGizmosDraw = true;
    
    void Start()
    {
        vectorField = GetComponent<IVectorField>();
        VectorFieldSingleton.Instance.ActivateField(vectorField);
    }

    void OnDestroy()
    {
        VectorFieldSingleton.Instance?.DeactivateField(vectorField);
    }

    void OnDisable()
    {
        VectorFieldSingleton.Instance?.DeactivateField(vectorField);
    }

    private void OnDrawGizmos()
    {
        if (!addDuringGizmosDraw) { return; }
        if (vectorField == null)
        {
            vectorField = GetComponent<IVectorField>();
        }
        VectorFieldSingleton.Instance.ActivateField(vectorField);
    }
}
