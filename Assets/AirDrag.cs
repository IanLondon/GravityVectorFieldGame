using UnityEngine;

public class AirDrag : MonoBehaviour
{
    [SerializeField] float scale = 1.0f;
    [SerializeField] float crossSectionalArea = 1.0f;
    [SerializeField] float airDensity = 1.225f; // in kg/m^3
    Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        float speed = velocity.magnitude;
        float dragForceMagnitude = 0.5f * scale * crossSectionalArea * airDensity * Mathf.Pow(speed, 2f);
        Vector3 dragForce = -dragForceMagnitude * velocity.normalized;

        rigidBody.AddForce(dragForce);
    }
}
