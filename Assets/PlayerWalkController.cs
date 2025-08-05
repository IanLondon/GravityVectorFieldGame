using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalkController : MonoBehaviour
{
    FloorDetector floorDetector;
    Rigidbody rigidBody;
    InputAction moveAction;
    bool tractorEngaged = false;
    [Tooltip("Max angle between local vector force and player 'up' axis. Tilting more than this angle will prohibit walking.")]
    [SerializeField] float maxWalkAngle = 10f;
    [SerializeField] float walkForceFactor = 1f;
    [SerializeField] GameObject floorDetectorObject;
    [SerializeField] float walkForceHeight = 1f;
    [SerializeField] float forceDistanceFromCenter = 2f;
    [SerializeField] bool canWalk = false;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");

        floorDetector = floorDetectorObject.GetComponent<FloorDetector>();

        //groundCollider = gameObject.AddComponent<BoxCollider>();
        //groundCollider.isTrigger = true;

        //var groundColliderHeight = 0.25f;
        //var playerHeight = 2f; // NOTE could get dynamically
        //groundCollider.center = Vector3.down * ((playerHeight - groundColliderHeight) / 2f);
        //groundCollider.size = new Vector3(1f, groundColliderHeight, 1f);
    }

    void Start()
    {
        GameEventsSingleton.Instance.OnPushOffStateChange += OnPushOffStateChange;    
    }

    void OnPushOffStateChange(PushOffState state)
    {
        if (state == PushOffState.TractorEngagedCannotPushOff || state == PushOffState.TractorEngagedCanPushOff)
        {
            tractorEngaged = true;
        } else
        {
            tractorEngaged = false;
        }
    }

    void UpdateCanWalk()
    {
        var force = VectorFieldSingleton.Instance.GetForce(transform.position);
        var angle = Vector3.Angle(-transform.up, force);

        var angleWithinRange = angle <= maxWalkAngle;

        var nextCanWalk = angleWithinRange && floorDetector.IsOnFloor();

        if (nextCanWalk != canWalk)
        {
            GameEventsSingleton.Instance.PlayerCanWalk(canWalk); // TODO: show some visual indicator eg on the HUD
            canWalk = nextCanWalk;
        }
    }

    void FixedUpdate()
    {
        if (!tractorEngaged)
        {
            UpdateCanWalk();

            if (canWalk)
            {
                var localMovement = moveAction.ReadValue<Vector2>();
                var worldMovement = transform.rotation * new Vector3(localMovement.x, 0f, localMovement.y);

                var forcePosition = transform.position + transform.up * walkForceHeight + worldMovement * forceDistanceFromCenter;
                Debug.DrawRay(forcePosition, worldMovement, Color.red);

                rigidBody.AddForceAtPosition(worldMovement * walkForceFactor, forcePosition, ForceMode.Force);
            }
        }
    }
}
