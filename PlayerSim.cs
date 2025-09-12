using Assets.T_Unit_Assets.Scripts;

using UnityEngine;


public class PlayerSim : MonoBehaviour
{
    public Sims ActiveSim = Sims.None;

    public float SpinSpeed = 5;
    public float SpinDist = 5;

    public BackAndForthAxis BAFAxis = BackAndForthAxis.X;
    public float BAFSpeed = 2;
    public float BAFMaxDist = 3;
    float DirectionMul = 1;



    public Vector3 StartPos = Vector3.zero;

    public Vector3 Velocity = Vector3.zero;
    //public float Speed = 0;
    public Vector3 LastPos = Vector3.zero;

    float Rot = 0f;

    public bool DeConstantifyVelocity = false;
    public int DCVMul = 1;

    void Start()
    {
        StartPos = transform.position;
        LastPos = transform.position;
    }

    void Update()
    {
        if (ActiveSim == Sims.Spin)
        {
            Vector2 RotatedPos = Util.RotateAroundPoint(StartPos.x, StartPos.z, Rot, SpinDist);
            transform.position = new Vector3(RotatedPos.x, transform.position.y, RotatedPos.y);

            Rot += SpinSpeed;
        }
        else if (ActiveSim == Sims.BackAndForth)
        {
            // What a mess
            if (BAFAxis == BackAndForthAxis.X)
            {
                if (DirectionMul == 1)
                {
                    if (transform.position.x > StartPos.x + BAFMaxDist)
                        DirectionMul *= -1;
                }
                else
                {
                    if (transform.position.x < StartPos.x - BAFMaxDist)
                        DirectionMul *= -1;
                }
                transform.AddPosition((float)(BAFSpeed * DirectionMul * Time.deltaTime * (DeConstantifyVelocity ? Random.Range(0.5f, 2) * DCVMul : 1)), 0, 0);
            }
            else if (BAFAxis == BackAndForthAxis.Y)
            {
                if (DirectionMul == 1)
                {
                    if (transform.position.y > StartPos.y + BAFMaxDist)
                        DirectionMul *= -1;
                }
                else
                {
                    if (transform.position.y < StartPos.y - BAFMaxDist)
                        DirectionMul *= -1;
                }
                transform.AddPosition(0, (float)(BAFSpeed * DirectionMul * Time.deltaTime * (DeConstantifyVelocity ? Random.Range(0.5f, 2) * DCVMul : 1)), 0);
            }
            else
            {
                if (DirectionMul == 1)
                {
                    if (transform.position.z > StartPos.z + BAFMaxDist)
                        DirectionMul *= -1;
                }
                else
                {
                    if (transform.position.z < StartPos.z - BAFMaxDist)
                        DirectionMul *= -1;
                }
                transform.AddPosition(0, 0, (float)(BAFSpeed * DirectionMul * Time.deltaTime * (DeConstantifyVelocity ? Random.Range(0.5f, 2) * DCVMul : 1)));
            }
        }
    }

    void FixedUpdate()
    {
        Velocity = (transform.position - LastPos) / Time.fixedDeltaTime;
        LastPos = transform.position;
    }

    public enum Sims
    {
        None,
        Spin,
        BackAndForth
    }

    public enum BackAndForthAxis
    {
        X,
        Y,
        Z
    }
}
