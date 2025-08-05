using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    int numContacts = 0;
    
    void OnTriggerEnter(Collider other)
    {
        numContacts++;
        Debug.Log("enter: " + other.gameObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        numContacts--;
    }

    public bool IsOnFloor() {
        return numContacts != 0;
    }
}
