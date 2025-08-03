using UnityEngine;

public class ControlPointGizmo : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * transform.localScale.z);
    }
}
