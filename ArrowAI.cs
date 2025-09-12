using UnityEngine;

public class ArrowAI : MonoBehaviour
{
    public int MaxTime = 5000;
    public bool ShouldDestroy = false;
    bool HasCollidedOnce = false;

    Rigidbody RB;

    int Time = 0;

    void Start()
        => RB = GetComponent<Rigidbody>();

    void Update()
    {
        if (ShouldDestroy && Time > MaxTime)
            Destroy(gameObject);
        if (!HasCollidedOnce)
            transform.LookAt(transform.position + RB.velocity);
        Time++;
    }

    private void OnCollisionEnter(Collision collision)
        => HasCollidedOnce = true;
}
