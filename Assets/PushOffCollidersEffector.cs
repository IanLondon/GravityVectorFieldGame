using UnityEngine;
using UnityEngine.InputSystem;

public enum PushOffState
{
    OutOfRangeOfLookPoint, // tractor beam is off. Whatever the player is looking at is out of tractor range. The player cannot activate the tractor.
    InRangeOfLookPoint, // tractor beam is off, but can be activated, because the current object the player is looking at is in tractor range.
    TractorEngagedCannotPushOff, // tractor beam is actively being held. The player cannot push off bc their trajectory angle is too close.
                                 // If they let go, it will just turn off the tractor beam.
    TractorEngagedCanPushOff // tractor beam is actively being held. The player CAN push off and will if they let go.

}

public class PushOffCollidersEffector : MonoBehaviour
{
    Camera mainCamera;
    Rigidbody rigidBody;
    InputAction pushOffAction;
    VectorFieldFollower vectorFieldFollower;

    Vector3 tractorNormal;
    bool tractorIsEngaged = false;
    bool? lookTargetIsInTractorRange = null;
    bool canCurrentlyPushOff = false;
    float? pushoffAngleFromTractorNormal;

    SpringJoint springJoint; // spring joint that makes the "tractor" attraction
    RaycastHit? tractorHit;
    
    [SerializeField] float pushForceMagnitude = 1f;
    [SerializeField] float raycastMaxDistance = 2f;
    [SerializeField] GameObject tractorBeamOrigin;
    [Tooltip("Maximum angle (in degrees) between the tractor beam normal and the target pushoff direction. Probably under 90 for realism/reasonableness")]
    [Range(0f, 180f)][SerializeField] float maxAllowedPushoffAngle = 85f;
    [SerializeField] float spring = 15f;
    [SerializeField] float springDamper = 5f;
    [SerializeField] float springMinDistance = 0.2f;
    [SerializeField] float springMaxDistance = 0.4f;
    [SerializeField] bool enableCollisionSpring = true;

    void Awake()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
        vectorFieldFollower = GetComponent<VectorFieldFollower>();
        pushOffAction = InputSystem.actions.FindAction("Jump");
    }
    bool CanPushOff()
    {
        return pushoffAngleFromTractorNormal <= maxAllowedPushoffAngle;
    }

    void Update()
    {
        if (tractorIsEngaged)
        {
            var prevCanCurrentlyPushOff = canCurrentlyPushOff;
            UpdatePushoffAngleFromTractorNormal();
            if (prevCanCurrentlyPushOff != canCurrentlyPushOff)
            {
                GameEventsSingleton.Instance.PushOffStateChange(canCurrentlyPushOff ? PushOffState.TractorEngagedCanPushOff : PushOffState.TractorEngagedCannotPushOff);
            }
        } else {
            // tractor not engaged.
            // we raycast continuously in order to update the HUD about whether look target is in range or not.
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            bool isInRange = (Physics.Raycast(ray, raycastMaxDistance));
            if (lookTargetIsInTractorRange == null || isInRange != lookTargetIsInTractorRange)
            {
                GameEventsSingleton.Instance.PushOffStateChange(isInRange ? PushOffState.InRangeOfLookPoint : PushOffState.OutOfRangeOfLookPoint);
            }
            lookTargetIsInTractorRange = isInRange;
        }

        if (pushOffAction.WasPressedThisFrame())
        {
            AttemptTractor();
        }
        else if (pushOffAction.WasReleasedThisFrame() && tractorIsEngaged)
        {
            AttemptPushOff();
            TeardownTractor();
        }
    }

    void AttemptTractor()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);

            tractorHit = hit;

            var colliderRigidBody = hit.collider.attachedRigidbody;
            springJoint = gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedBody = colliderRigidBody;

            // Put the origin at the tractor beam origin empty
            springJoint.anchor = gameObject.transform.InverseTransformPoint(tractorBeamOrigin.transform.position);
            
            if (colliderRigidBody)
            {
                // spring joint connectedAnchor is local coords of connected body, if there is one
                springJoint.connectedAnchor = hit.collider.transform.InverseTransformPoint(hit.point); // connect spring to hit location in connected object's local coords
            } else
            {
                // if there is no connected body, it's world coords
                springJoint.connectedAnchor = hit.point;
            }

            springJoint.spring = spring;
            springJoint.damper = springDamper;
            springJoint.minDistance = springMinDistance;
            springJoint.maxDistance = springMaxDistance;
            springJoint.enableCollision = enableCollisionSpring;

            tractorIsEngaged = true;
            lookTargetIsInTractorRange = null;

            var prevCanCurrentlyPushOff = canCurrentlyPushOff;
            tractorNormal = hit.normal;
            UpdatePushoffAngleFromTractorNormal();
            GameEventsSingleton.Instance.PushOffStateChange(canCurrentlyPushOff ? PushOffState.TractorEngagedCanPushOff : PushOffState.TractorEngagedCannotPushOff);

            // disable player's VectorFieldFollower while tractor beam is engaged
            vectorFieldFollower.enabled = false;

            // neutralize player's inertia
            rigidBody.angularVelocity = Vector3.zero;

            if (colliderRigidBody)
            {
                // account for attaching to a moving body, we don't want to stop the target if the target is moving
                rigidBody.linearVelocity = colliderRigidBody.linearVelocity; // TODO maybe scale this down by a factor
            } else
            {
                rigidBody.linearVelocity = Vector3.zero;
            }
        }
        // else do nothing, tractor didn't hit anything
    }

    void AttemptPushOff()
    {
        if (tractorHit is RaycastHit hit)
        {
            if (!CanPushOff()) {
                tractorIsEngaged = false;
                return;
            }

            var pushoffDirection = mainCamera.transform.forward;

            // apply force to the collider, if it has a rigid body. Walls etc won't have it, so we'll do nothing to them.
            var colliderRigidBody = hit.collider.attachedRigidbody;
            if (colliderRigidBody != null)
            {
                colliderRigidBody.AddForceAtPosition(-pushoffDirection * pushForceMagnitude, hit.point, ForceMode.Impulse);
            }

            // apply the opposite force to the player. It's slightly offset bc we're pushing from the camera but applying it to the player RB origin
            rigidBody.AddForce(pushoffDirection * pushForceMagnitude, ForceMode.Impulse);
        } else
        {
            Debug.LogError("Tried to push off with null tractorHit " + tractorHit);
        }
    }

    void TeardownTractor()
    {
        if (springJoint)
        {
            Destroy(springJoint);
        }
        tractorIsEngaged = false;
        tractorHit = null;
        tractorNormal = Vector3.zero;
        pushoffAngleFromTractorNormal = null;
        canCurrentlyPushOff = false;

        vectorFieldFollower.enabled = true;
    }

    // We have to do this continuously in order to detect changes so we can send the event.
    void UpdatePushoffAngleFromTractorNormal()
    {
        var pushoffDirection = mainCamera.transform.forward;

        pushoffAngleFromTractorNormal = Vector3.Angle(pushoffDirection, tractorNormal);

        canCurrentlyPushOff = !(pushoffAngleFromTractorNormal > maxAllowedPushoffAngle);

        //Debug.DrawRay(transform.position, pushoffDirection, Color.green, 3f);
        //Debug.DrawRay(transform.TransformPoint(springJoint.anchor), tractorNormal, Color.orange, 3f);

        // Draw the spring
        Debug.DrawLine(transform.TransformPoint(springJoint.anchor), (
            springJoint.connectedBody
                    ? springJoint.connectedBody.transform.TransformPoint(springJoint.connectedAnchor) // connectedbody local to world coords
                    : springJoint.connectedAnchor // no connected body, connectedAnchor is already in world coords
            ), Color.orange);

        Debug.DrawRay(transform.position, tractorNormal * 3, Color.aliceBlue);
    }
}
