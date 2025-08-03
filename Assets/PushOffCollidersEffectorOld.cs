using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PushOffCollidersEffectorOld : MonoBehaviour
{
    Rigidbody rigidBody;
    Camera mainCamera;
    SpringJoint springJoint; // spring joint that makes the "tractor" attraction
    bool holdingTractor = false;
    InputAction pushOffAction;
    List<Collider> activeColliders = new List<Collider>(); // TODO this should be something more memory/CPU-efficient
    [SerializeField] float pushForceMagnitude = 1f;
    [SerializeField] float raycastMaxDistance = 2f;

    void Start()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
        pushOffAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        if (pushOffAction.WasPressedThisFrame())
        {
            AttemptTractor();
        }
        else if (pushOffAction.WasReleasedThisFrame() && holdingTractor)
        {
            TeardownTractor();
            AttemptPushOff();
        }
    }

    void AttemptTractor()
    {
        Debug.Log("Tractor attempted!");

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance))
        {
            Debug.Log("tractor hit " + hit.collider);
            Debug.DrawLine(ray.origin, hit.point, Color.green, raycastMaxDistance);

            var colliderRigidBody = hit.collider.attachedRigidbody;
            springJoint = gameObject.AddComponent<SpringJoint>();
            springJoint.connectedBody = colliderRigidBody;

            holdingTractor = true;
        }
        else
        {
            Debug.Log("tractor did not hit anything");
        }
    }

    void AttemptPushOff()
    {
        Debug.Log("attempted pushoff! " + activeColliders.Count + " colliders in range");

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance))
        {
            Debug.Log("pushoff hit " + hit.collider);
            Debug.DrawLine(ray.origin, hit.point, Color.blue, raycastMaxDistance);

            // apply force to the collider, if it has a rigid body. Walls etc won't have it, so we'll do nothing to them.
            var colliderRigidBody = hit.collider.attachedRigidbody;
            if (colliderRigidBody != null)
            {
                colliderRigidBody.AddForceAtPosition(ray.direction * pushForceMagnitude, hit.point, ForceMode.Impulse);
            }

            // apply the opposite force to the player. It's slightly offset bc we're pushing from the camera but applying it to the player RB origin
            rigidBody.AddForce(-ray.direction * pushForceMagnitude, ForceMode.Impulse);

        }
        else
        {
            Debug.Log("no pushoff");
        }
    }

    void TeardownTractor()
    {
        if (springJoint != null)
        {
            Destroy(springJoint);
        }
        holdingTractor = false;
    }

}
