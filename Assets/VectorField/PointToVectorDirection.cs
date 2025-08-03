using UnityEngine;

enum Direction {X,Y,Z}

// Gradually points an object to a vector sampled from the vector field.
public class PointToVectorDirection : MonoBehaviour
{
    IVectorField vectorField;
    Rigidbody rigidBody;
    [SerializeField] Direction direction = Direction.Y;
    [SerializeField] GameObject vectorFieldGameObject;
    [Tooltip("If checked, object will point against the force direction, instead of in the same direction of the force.")]
    [SerializeField] bool opposeForceDirection = false;
    [SerializeField][Range(0f,2f)] float torqueRatio = 0.8f;
    [SerializeField][Range(0f, 10f)] float dampingFactor = 1f;
    void Start()
    {
        vectorField = vectorFieldGameObject.GetComponent<IVectorField>();
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // this is the direction the object will point from, eg Y poin.
        Vector3 fromDirection = transform.up;

        switch (direction)
        {
            case Direction.X: fromDirection = transform.right; break;
            case Direction.Y: fromDirection = transform.up; break;
            case Direction.Z: fromDirection = transform.forward; break;
        }

        if (opposeForceDirection)
        {
            fromDirection *= -1f;
        }

        var force = vectorField.GetForce(transform.position);

        // cross product gives perpendicular axis, its magnitude is the force from the vector field
        var axis = Vector3.Cross(fromDirection, force);
        // must dampen angular velocity of the rigidBody or it will bob back and forth
        var torque = axis * torqueRatio - rigidBody.angularVelocity * dampingFactor;
        rigidBody.AddTorque(torque);

        // Some math notes in https://stackoverflow.com/a/64095207
    }
}

//
// OLD VERSION: had erratic physics behavior bc of non-physics lerp
//
//
//public class PointToVectorDirection : MonoBehaviour
//{
//    IVectorField vectorField;
//    [SerializeField] Direction direction = Direction.Y;
//    [SerializeField] bool opposeForceDirection = false;
//    [SerializeField] GameObject vectorFieldGameObject;
//    [SerializeField][Range(0f, 1f)] float torqueRatio = 0.15f;
//    void Start()
//    {
//        vectorField = vectorFieldGameObject.GetComponent<IVectorField>();
//    }

//    void FixedUpdate()
//    {
//        Vector3 fromDirection = transform.up;

//        switch (direction)
//        {
//            case Direction.X: fromDirection = transform.right; break;
//            case Direction.Y: fromDirection = transform.up; break;
//            case Direction.Z: fromDirection = transform.forward; break;
//        }

//        if (opposeForceDirection)
//        {
//            fromDirection *= -1f;
//        }

//        Vector3 force = vectorField.GetForce(transform.position);

//        // TODO: make an alternative version of this component that applies a rotational force to a rigidbody
//        // instead of Lerp. Because the Lerp here can add a lot of force and make physics unstable.
//        Quaternion targetRotation = transform.rotation * Quaternion.FromToRotation(fromDirection, force);
//        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, torqueRatio);
//    }
//}