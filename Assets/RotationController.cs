using UnityEngine;
using UnityEngine.InputSystem;

public class RotationController : MonoBehaviour
{

    InputAction lookAction;
    float tiltDegrees = 0f;
    [SerializeField] bool invertTilt = false;
    [SerializeField] float sensitivity = 0.2f;
    [Tooltip("GameObject to pan (left/right rotation). It will be rotated about its Y axis.")]
    [SerializeField] GameObject panTarget;
    [Tooltip("GameObject to tilt (up/down rotation). This should be a child of the panTarget! Its local axes will be rewritten.")]
    [SerializeField] GameObject tiltTarget;
    [Tooltip("Maximum allowed tilt in degrees, in either direction")]
    [SerializeField][Range(0f,360f)] float tiltClampAbsMaxDeg = 80f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");
    }

    void Update()
    {
        var lookInput = lookAction.ReadValue<Vector2>() * sensitivity;
        if (invertTilt)
        {
            lookInput *= new Vector2(1f, -1f);
        }
        // pan
        panTarget.transform.Rotate(0f, lookInput[0], 0f);

        // tilt (clamped)
        tiltDegrees += lookInput[1];
        tiltDegrees = Mathf.Clamp(tiltDegrees, -tiltClampAbsMaxDeg, tiltClampAbsMaxDeg);

        tiltTarget.transform.localEulerAngles = new Vector3(tiltDegrees, 0f, 0f);

        //tiltTarget.transform.Rotate(tiltDegrees, 0f, 0f);

        //
        //var tiltAngle = Quaternion.Angle(tiltTarget.transform.localRotation, tiltTarget.transform.forward);
        //Debug.Log("tilt angle " + tileAngle);

        //if (Mathf.Abs(tiltTarget.transform.localEulerAngles.x) > tiltClampAbsMaxDeg)
        //{
        //    Debug.Log("too much tilt!! " + tiltTarget.transform.localEulerAngles.x);
        //    //tiltTarget.transform.localEulerAngles  = new Vector3(
        //    //    tiltClampAbsMaxDeg, 
        //    //    tiltTarget.transform.localEulerAngles.y, 
        //    //    tiltTarget.transform.localEulerAngles.z);
        //}
    }
}
