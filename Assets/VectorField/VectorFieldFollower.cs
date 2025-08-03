using UnityEngine;

public class VectorFieldFollower : MonoBehaviour
{
    Rigidbody rigidBody;
    IVectorField vectorField;
    [SerializeField] GameObject vectorFieldGameObject;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        vectorField = vectorFieldGameObject.GetComponent<IVectorField>();
    }

    void FixedUpdate()
    {
        rigidBody.AddForce(vectorField.GetForce(rigidBody.position));
    }
}
