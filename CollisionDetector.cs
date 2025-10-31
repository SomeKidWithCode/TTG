using Assets.T_Unit_Assets.Scripts;

using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool CollisionDetected = false;

    // You can't swing a sword through a wall (unless the sword is sharp and/or the wall is soft)

    void OnCollisionEnter(Collision collision)
        => CollisionDetected = true;

    void OnCollisionExit(Collision collision)
        => CollisionDetected = false;
}
