using UnityEngine;

enum CombinationMethod
{
    AddWithGaussianDecay,
    AddWithSharpDecay,
    Interpolate
}

// Convention: a control point's forward (z+) is its direction, and its local z scale is its magnitude.
public class ControlPointVectorField : MonoBehaviour, IVectorField
{
    [SerializeField] CombinationMethod combinationMethod;
    [Tooltip("Rate of decay, used only by AddWith* methods.")][Range(0.01f, 10f)][SerializeField] float decayFactor = 1.0f;
    [SerializeField] GameObject[] controlPoints;

    public Vector3 GetForce(Vector3 position)
    {
        if (controlPoints.Length == 0)
        {
            // No control points? No forces.
            return Vector3.zero;
        }

        float[] distances = new float[controlPoints.Length];
        float totalDistance = 0f;

        // compute all distances
        for (int i = 0; i < controlPoints.Length; i++)
        {
            float dist = Vector3.Distance(position, controlPoints[i].transform.position);
            totalDistance += dist;
            distances[i] = dist;
        }

        // combine influence of all control points weighted by distance
        Vector3 force = Vector3.zero;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            switch (combinationMethod)
            {
                case CombinationMethod.AddWithGaussianDecay:
                    force += Mathf.Exp(-decayFactor * Mathf.Pow(distances[i], 2f)) * controlPoints[i].transform.forward * controlPoints[i].transform.localScale.z;
                    break;
                case CombinationMethod.AddWithSharpDecay:
                    force += Mathf.Exp(-decayFactor * distances[i]) * controlPoints[i].transform.forward * controlPoints[i].transform.localScale.z;
                    break;
                case CombinationMethod.Interpolate:
                    // 1 - ratio of distance = inverse ratio of distance, so closer = proportionally stronger influence.
                    force += (1f - distances[i] / totalDistance) * controlPoints[i].transform.forward * controlPoints[i].transform.localScale.z;
                    break;

            }

        }

        return force;
    }
}

// Convention: a control point's forward (z+) is its direction, and its local z scale is its magnitude.
//
// OLD VERSION!!! This scales too smoothly
//
//public class ControlPointVectorField : MonoBehaviour, IVectorField
//{
//    [SerializeField] CombinationMethod combinationMethod;
//    [Tooltip("Rate of decay, used only by AddWithDecay.")][Range(0f, 10f)][SerializeField] float decayFactor = 1.0f;
//    [SerializeField] GameObject[] controlPoints;

//    public Vector3 GetForce(Vector3 position)
//    {
//        if (controlPoints.Length == 0)
//        {
//            // No control points? No forces.
//            return Vector3.zero;
//        }

//        float[] distances = new float[controlPoints.Length];
//        float totalDistance = 0f;

//        // compute all distances
//        for (int i = 0; i < controlPoints.Length; i++)
//        {
//            float dist = Vector3.Distance(position, controlPoints[i].transform.position);
//            totalDistance += dist;
//            distances[i] = dist;
//        }

//        // combine influence of all control points weighted by distance
//        Vector3 force = Vector3.zero;
//        for (int i = 0; i < controlPoints.Length; i++)
//        {
//            switch (combinationMethod)
//            {
//                case CombinationMethod.AddWithDecay:
//                    force += Mathf.Exp(-decayFactor * distances[i]) * controlPoints[i].transform.forward * controlPoints[i].transform.localScale.z;
//                    break;
//                case CombinationMethod.Interpolate:
//                    // 1 - ratio of distance = inverse ratio of distance, so closer = proportionally stronger influence.
//                    force += (1f - distances[i] / totalDistance) * controlPoints[i].transform.forward * controlPoints[i].transform.localScale.z;
//                    break;

//            }

//        }

//        return force;
//    }
//}
