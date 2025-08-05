using UnityEngine;

enum Direction {X,Y,Z}

// Gradually points an object to a vector sampled from the vector field.
public class PointToVectorDirection : MonoBehaviour
{
    bool derivativeInitialized = false;
    Vector3 prevFromDirection = Vector3.zero;
    float accumulatedError = 0f;
    IVectorField vectorField;
    Rigidbody rigidBody;
    [SerializeField] Direction direction = Direction.Y;
    [SerializeField] GameObject vectorFieldGameObject;
    [Tooltip("If checked, object will point against the force direction, instead of in the same direction of the force.")]
    [SerializeField] bool opposeForceDirection = false;
    [SerializeField][Range(0f, 1000f)] float proportionalGain = 25f;
    [SerializeField][Range(0f, 1000f)] float integralGain = 0f;
    [SerializeField][Range(0f, 1000f)] float derivativeGain = 5f;
    [SerializeField][Range(0f, 10000)] float torqueCeiling = 200f;
    [Tooltip("Ignore error if below this threshold")]
    void Start()
    {
        vectorField = vectorFieldGameObject.GetComponent<IVectorField>();
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //this is the direction the object will point from
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

        var error = Vector3.Angle(fromDirection, force) / 180f; // error => range [0, 1]

        // PROPORTIONAL
        var proportionalInput = error * proportionalGain;

        // INTEGRAL
        // TODO I don't think I implemented this correctly?
        accumulatedError += error * Time.fixedDeltaTime;
        var integralInput = accumulatedError * integralGain;

        // DERIVATIVE
        var derivativeInput = -(Vector3.Angle(fromDirection, prevFromDirection) / 180f) / Time.fixedDeltaTime * derivativeGain; // "derivative on measurement" ie value rate of change

        if (!derivativeInitialized)
        {
            // skip derivative kick on first update, derivative btw zero and any value is steep but shouldn't be
            derivativeInput = 0f;
            derivativeInitialized = true;
        }

        var controllerInput = proportionalInput + integralInput + derivativeInput;

        // cross product gives perpendicular axis, its magnitude is the force from the vector field
        var axis = Vector3.Cross(fromDirection, force);

        // neutralizing angular velocity is important, otherwise it will wobble btw local x and y
        // torque is clamped so the controller doesn't impose massive forces
        var torque = Vector3.ClampMagnitude(axis * controllerInput - rigidBody.angularVelocity, torqueCeiling);

        Debug.Log((axis * controllerInput - rigidBody.angularVelocity).magnitude);

        rigidBody.AddTorque(torque);
        Debug.DrawRay(transform.position, torque, Color.yellow);

        prevFromDirection = fromDirection;
        
        // Some math notes in https://stackoverflow.com/a/64095207
        // and PID controller overview in https://vazgriz.com/621/pid-controllers/ (this is a PD controller right now, no integral compnent)
        // and https://digitalopus.ca/site/pd-controllers/
    }
}
