using Assets.T_Unit_Assets.Scripts;

using Unity.VisualScripting;

using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool CollisionDetected = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Makes sure we can only detect hits on the player
        if (collision.gameObject == Util.Player)
            CollisionDetected = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == Util.Player)
            CollisionDetected = false;
    }
}
