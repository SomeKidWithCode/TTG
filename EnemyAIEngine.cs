using Assets.T_Unit_Assets.Scripts;
using System;
using Unity.VisualScripting;

using UnityEngine;

#pragma warning disable IDE0079     // Disable 'unnessesary supression' suggestion 
#pragma warning disable IDE0044     // Disable 'make readonly' suggestion
#pragma warning disable CS0414      // Disable 'unused variable' warning
#pragma warning disable IDE0059     // Disable 'unnessary assignment' suggestion
#pragma warning disable UNT0001     // Disable 'Unity method empty' suggestion
#pragma warning disable CS0162      // Disable 'Unreachable code' warning

public class EnemyAIEngine : MonoBehaviour
{
    GameObject Target;
    PlayerSim PS;

    public float Health = 100;
    public float Speed = 3;

    public GameObject Weapon;
    public WeaponType WeaponT = WeaponType.Sword;

    public GameObject Shield;

    public Transform WeaponRestPos;

    public Transform ProjSpawnLoc;

    ulong Frames = 0;


    #region Melee variables

    bool IsSwinging = false;
    public float MeleeAttackDist = 5;
    // These are used for the lerping positon
    Vector3 SwingingLerpPointA = new();
    Vector3 SwingingLerpPointB = new();
    float SwingingLerpVal = 0f;

    // The angle for the main swing of the baton, according to the handle fulcrum
    float MainSwingFulcrumAngle = 0.1f;

    // Variables used for the rotation around the point
    readonly float RAPDist = 0f;
    float RAPRot = 2f;

    CollisionDetector WeaponCD;



    public static float StartSwingAngle = 45;
    public float SwingXRot = StartSwingAngle;
    public float SwingYRot = StartSwingAngle;
    public float SwingZRot = 0;

    Vector3 MeleeSwingOrigin;
    public float SwingIncrement = 0.01f;

    
    public float UpDownSwing = 90f;
    public float LeftRightSwing = 0f;


    #endregion

    #region Ranged variables

    public float RangedAttackDist = 300;
    public ulong FiringInterval = 100;
    public float ProjSpeed = 10;
    public AimPredictionFactor AimIterations = AimPredictionFactor.High;

    #endregion


    public GameObject AimPosObj;



    public void Start()
    {
        Target = Util.Player;
        PS = Util.Player.GetComponent<PlayerSim>();

        /*GameObject.FindGameObjectWithTag("AAAAAAA").transform.position = new Vector3(-899, -518, -6);

        // Exp
        Vector3 P = Util.ThreeDRotateAroundPoint(
            new Vector3(-899, -518, -6),
            new Vector3(56, 95, 137),
            new Vector3(3, 5, 7)
        );
        // Returns the following values:
        // (-6.00, -515.00)
        // (-895.46, -514.46)
        // (0.95, -898.15)

        AimPosObj.transform.position = P;*/

		if (WeaponT == WeaponType.Sword && !Weapon.TryGetComponent(out WeaponCD))
		    throw new ArgumentNullException("Weapon must have a collision detector");
    }

    public void Update()
    {
        // Make the target look at the player
        transform.LookAt(Target.transform);


        // Distance between enemy and target (in meters)
        float Dist = Vector3.Distance(Target.transform.position, transform.position);


        if (WeaponT == WeaponType.Bow && Dist > RangedAttackDist)
            MoveTowardsTarget();
        else if (WeaponT == WeaponType.Sword && Dist > MeleeAttackDist)
            MoveTowardsTarget();


        MeleeSwingOrigin = transform.position + new Vector3(0, 0.5f, 0);


        if (WeaponT == WeaponType.Sword && Dist < MeleeAttackDist)
            Attack();
        else if (WeaponT == WeaponType.Bow && Dist < RangedAttackDist)
            Attack();
        /*else if (WeaponT == WeaponType.Sword)
            ReturnWeaponToRest();*/

        Frames++;
    }



    // Condense this in final build
    private void MoveTowardsTarget()
    {
        // Step one, record the position of the player and the object
        Vector3 PlayerPos = Target.transform.position;
        Vector3 MePos = transform.position;
        Vector3 NewPos = MePos.Clone();
        
        // Step two, obtain the direction to face the target
        float Rot = Mathf.Atan2(PlayerPos.z - MePos.z, PlayerPos.x - MePos.x);

        // Step three, get the direction of the rotation
        float x = Mathf.Cos(Rot);
        float z = Mathf.Sin(Rot);

        // Step four, add the direction * speed * deltaTime (for frame-consistent speed)
        NewPos.x += x * Speed * Time.deltaTime;
        NewPos.z += z * Speed * Time.deltaTime;

        // Step five, apply the adjusted position
        transform.position = NewPos;
    }

    private void Retreat()
    {
		
    }

    private void CircleTarget()
    {

    }

    private void DashAttack()
    {

    }

    private void Attack()
    {
        if (WeaponT == WeaponType.Sword)
        {
            if (ExpSwordAttackSys)
            {
                // Rotate the weapon about the fulcrum (the EP)
                /*Weapon.transform.Rotate(new Vector3(MainSwingFulcrumAngle, MainSwingFulcrumAngle, MainSwingFulcrumAngle), Space.Self);
                //Weapon.transform.AddLocalRotation(MainSwingFulcrumAngle, MainSwingFulcrumAngle, MainSwingFulcrumAngle);
                Debug.Log(Weapon.transform.localRotation.eulerAngles);


                // Magic maths that's not going to work because im thinking 2d in a 3d space
                Vector3 LerpedVec = Util.Lerp(SwingingLerpPointA, SwingingLerpPointB, SwingingLerpVal);
                //Vector3 LerpedVec = new(Me.transform.localPosition.x + 1, Me.transform.localPosition.y, Me.transform.localPosition.z);
                Vector2 RotatedVec = Util.RotateAroundPoint(LerpedVec.x, LerpedVec.z, RAPRot, RAPDist);

                // This function is going to be so cracked
                Weapon.transform.localPosition = new Vector3(RotatedVec.x, RotatedVec.y, LerpedVec.z);

                // Tick the variables
                MainSwingFulcrumAngle += 0.01f;
                RAPRot += 0.001f; // Low because i don't trust it
                if (Frames % 10 == 0)
                    SwingingLerpVal += 0.01f;*/



                // and now for something, completely different

                /*if (!WeaponCD.CollisionDetected)
                {
                    Vector3 SwordPos = Util.ThreeDRotateAroundPoint(
                        MeleeSwingOrigin,
                        new Vector3(transform.eulerAngles.x + SwingXRot, transform.eulerAngles.y + SwingYRot, transform.eulerAngles.z + SwingZRot),
                        new Vector3(1, 1, 1)
                    );
                    Weapon.transform.position = SwordPos;

                    Weapon.transform.LookAt(transform);

                    Weapon.transform.Rotate(new Vector3(0, 0, 0));

                    //SwingXRot += SwingIncrement;
                    SwingYRot += SwingIncrement;
                }
                else
                {

                }*/


                // i think i might actually be able to make this really simple.

                Vector2 V2A = Util.RotateAroundPoint(new Vector3(transform.position.x, transform.position.y), UpDownSwing, 2);
                Vector2 V2B = Util.RotateAroundPoint(new Vector3(transform.position.x, transform.position.z), LeftRightSwing, 2);

                Weapon.transform.position = new(V2A.x, V2A.y, Weapon.transform.position.z);

                Debug.Log(V2A);
                Debug.Log(V2B);
            }
            else
            {
                Quaternion WeaponRot = Weapon.transform.localRotation;

                WeaponRot.x += Util.Rand.Next(-10, 10);
                WeaponRot.y += Util.Rand.Next(-10, 10);
                WeaponRot.z += Util.Rand.Next(-10, 10);

                Weapon.transform.localRotation = WeaponRot;
            }
        }
        else
        {
            if (Frames % FiringInterval == 0)
            {
                // https://yal.cc/simplest-possible-predictive-aiming/


                Vector3 SpawnPos = Weapon.transform.position;

                Vector3 TStart = Target.transform.position.Clone();
                Vector3 TPose = Target.transform.position.Clone();

                for (int i = 0; i < (int)AimIterations; i++)
                {
                    float Dist = Vector3.Distance(TPose, SpawnPos);
                    float TTR = Dist / ProjSpeed;
                    TPose = TStart + PS.Velocity * TTR;
                }

                float Rot = Mathf.Atan2(TPose.x - SpawnPos.x, TPose.z - SpawnPos.z);

                GameObject Proj = Instantiate(Weapon, SpawnPos, Quaternion.identity);
                Rigidbody RB = Proj.GetComponent<Rigidbody>();
                RB.constraints = RigidbodyConstraints.None;
                RB.velocity = new Vector3(Mathf.Sin(Rot), 0.1f, Mathf.Cos(Rot)) * ProjSpeed;
                Proj.GetComponent<ArrowAI>().ShouldDestroy = true;
                Proj.GetComponent<MeshCollider>().enabled = true;
                Proj.SetActive(true);

                if (AimPosObj != null)
                    AimPosObj.transform.position = TPose;
            }
        }
    }

    private bool Block()
    {
        // If we can't block due to having no shield, return false
        // (Not yet sure what I'm going to use that return for yet, but its there)
        if (Shield == null)
            return false;

        return true;
    }

    private void ReturnWeaponToRest()
    {
        // Gradual return
        if (Frames % 5 == 0 && !Util.AreTransformsEqual(WeaponRestPos, Weapon.transform))
        {
            if (Weapon.transform.rotation.x > WeaponRestPos.rotation.x)
                Weapon.transform.AddRotation(0.5f * Time.deltaTime, 0, 0);
            if (Weapon.transform.rotation.y > WeaponRestPos.rotation.y)
                Weapon.transform.AddRotation(0, 0.5f * Time.deltaTime, 0);
            if (Weapon.transform.rotation.z > WeaponRestPos.rotation.z)
                Weapon.transform.AddRotation(0, 0, 0.5f * Time.deltaTime);

            if (Weapon.transform.rotation.x < WeaponRestPos.rotation.x)
                Weapon.transform.AddRotation(-0.5f * Time.deltaTime, 0, 0);
            if (Weapon.transform.rotation.y < WeaponRestPos.rotation.y)
                Weapon.transform.AddRotation(0, -0.5f * Time.deltaTime, 0);
            if (Weapon.transform.rotation.z < WeaponRestPos.rotation.z)
                Weapon.transform.AddRotation(0, 0, -0.5f * Time.deltaTime);

            if (Util.Between(Weapon.transform.rotation.x, WeaponRestPos.rotation.x - 0.5f, WeaponRestPos.rotation.x + 0.5f))
                Weapon.transform.rotation = Quaternion.Euler(
                    WeaponRestPos.rotation.eulerAngles.x,
                    Weapon.transform.rotation.eulerAngles.y,
                    Weapon.transform.rotation.eulerAngles.z
                );
            if (Util.Between(Weapon.transform.rotation.y, WeaponRestPos.rotation.y - 0.5f, WeaponRestPos.rotation.y + 0.5f))
                Weapon.transform.rotation = Quaternion.Euler(
                    Weapon.transform.rotation.eulerAngles.x,
                    WeaponRestPos.rotation.eulerAngles.y,
                    Weapon.transform.rotation.eulerAngles.z
                );
            if (Util.Between(Weapon.transform.rotation.z, WeaponRestPos.rotation.z - 0.5f, WeaponRestPos.rotation.z + 0.5f))
                Weapon.transform.rotation = Quaternion.Euler(
                    Weapon.transform.rotation.eulerAngles.x,
                    Weapon.transform.rotation.eulerAngles.y,
                    WeaponRestPos.rotation.eulerAngles.z
                );


        }
    }

    private static readonly bool ExpSwordAttackSys = true;

    private void OnCollisionEnter(Collision collision)
    {
        // Only run the following code if the collided object is another enemy
        if (!collision.gameObject.CompareTag("Enemy"))
            return;
    }
}
