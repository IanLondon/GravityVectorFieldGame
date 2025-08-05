using UnityEngine;

public class DeadlyCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // HACK, try collision layers w player
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            GameEventsSingleton.Instance.KillPlayer(PlayerDeathReason.DeadlyCollision);
        }
    }
}
