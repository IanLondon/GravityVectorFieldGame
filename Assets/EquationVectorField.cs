using System;
using UnityEngine;

enum Equation
{
    Static,
    Squish,
    LocalSquish,
    Twirl,
    Flush
}

public class EquationVectorField : MonoBehaviour, IVectorField
{
    [Tooltip("If true, the vector field will be translated by this object's world transform x y z.")]
    [SerializeField] bool applyLocalPositionDelta = true;
    [Tooltip("Scale the magnitude of the force.")]
    [SerializeField] float scale = 1f;
    [Tooltip("Scale the decay rate (applied to LocalSquish only.")]
    [Range(0f, 10f)][SerializeField] float decayRate = 0.2f;
    [SerializeField] Equation equation;

    // NOTE: https://anvaka.github.io/fieldplay is a cool place to play with vector spaces in 2d. Try "randomize".
    public Vector3 GetForce(Vector3 position)
    {
        if (applyLocalPositionDelta)
        {
            position -= transform.position;
        }

        float x = position.x;
        float y = position.y;
        float z = position.z;

        Vector3 result;

        switch (equation)
        {
            case Equation.Static:
                // use this GameObject's forward (z+) orientation to apply static force
                result = transform.forward;
                break;
            case Equation.Squish:
                result = -position; // negative = inwards
                break;
            case Equation.LocalSquish:
                result = -position * Mathf.Exp(-decayRate * position.magnitude); // negative = inwards, with log decay
                break;
            case Equation.Twirl:
                result = new Vector3(-z, 0, x * z);
                break;
            case Equation.Flush:
                result = new Vector3(-Mathf.Sin(x) + z, -4f, -0.25f * Mathf.Cos(z) - x);
                break;
            default:
                Console.WriteLine("no equation selected??");
                return Vector3.zero;
        }

        return result * scale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.75f, 0.0f, 0.0f, 0.75f);

        // Convert the local coordinate values into world
        // coordinates for the matrix transformation.
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * scale);
    }
}
