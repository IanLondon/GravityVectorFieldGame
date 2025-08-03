using UnityEngine;

public class VectorFieldVisualizerGizmo : MonoBehaviour
{
    [Range(0f, 10f)][SerializeField] float scale = 1f;
    [Range(0f, 10f)][SerializeField] float spacing = 1f;
    [Range(1, 25)][SerializeField] int xResolution = 10;
    [Range(1, 25)][SerializeField] int yResolution = 10;
    [Range(1, 25)][SerializeField] int zResolution = 10;
    [SerializeField] GameObject vectorFieldGameObject;

    void OnDrawGizmos()
    {
        var show = gameObject.activeInHierarchy;
        if (show && vectorFieldGameObject && vectorFieldGameObject.TryGetComponent<IVectorField>(out IVectorField vectorField)) {
            for (int ix = 0; ix < xResolution; ix++)
            {
                for (int iy = 0; iy < yResolution; iy++)
                {
                    for (int iz = 0; iz < zResolution; iz++)
                    {

                        Vector3 origin = transform.position + new Vector3(ix * spacing, iy * spacing, iz * spacing);
                        Vector3 localForce = vectorField.GetForce(origin);


                        // Color each cube/ray with rotation XYZ mapped to RGB (add one and half to comp for negative values)
                        Vector3 normalizedForce = (Vector3.Normalize(localForce) + Vector3.one) * 0.5f;
                        Gizmos.color = new Color(normalizedForce.x, normalizedForce.y, normalizedForce.z);

                        Gizmos.DrawCube(origin, Vector3.one * 0.1f);
                        Gizmos.DrawRay(origin, localForce * scale);
                    }
                }
            }
        }
    }
}
