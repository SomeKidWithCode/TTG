using System;
using System.Collections;

using Assets.T_Unit_Assets.Scripts;

using Unity.VisualScripting;

using UnityEngine;

#pragma warning disable IDE0079     // Disable 'unnessesary supression' suggestion 
#pragma warning disable IDE0044     // Disable 'make readonly' suggestion
#pragma warning disable CS0414      // Disable 'unused variable' warning
#pragma warning disable IDE0059     // Disable 'unnessary assignment' suggestion
#pragma warning disable UNT0001     // Disable 'Unity method empty' suggestion
#pragma warning disable CS0162      // Disable 'Unreachable code' warning

// What a mess this file is
// Now I know how actual game devs feel
public class EnemyAIEngine : MonoBehaviour
{
    // You get a public and you get a public, everyone gets a public (organisation is overrated anyway)
    public GameObject Target;
    public PlayerSim PS;

    public float MaxHealth = 100;
    public float Health = 100;
    public bool IsDead => Health <= 0;
    public float HealthPercentage => Health / MaxHealth * 100;
    public float Speed = 3;

    public GameObject WeaponHandle;
    public WeaponTypes WeaponType = WeaponTypes.None;
    public DashSpeeds DashSpeed = DashSpeeds.Medium;

    public EnemyTypes EnemyType = EnemyTypes.Human;

    public GameObject Shield;

    public Transform WeaponRestPosTarget;
    public Vector3 WeaponRestPos;
    public Vector3 WeaponRestRot;

    public Transform ProjSpawnLoc;

    public Transform SwordSwingCenter;

    public ulong Frames = 0;

    public float DistFromTarget = float.MaxValue;


    // All chances is presented as a fraction of 1/n

    #region Human stuff

    public int ChanceToDash = 6_420;
    public bool IsDashing = false;
    public bool ManualDashTrigger = false;

    public int ChanceToCircle = 100_000;
    public int ChanceToStopCircling = 1_000_000;
    public bool IsCircling = false;
    public bool ManualCircleTrigger = false;

    public int ChanceToRetreat = 10_000;
    public bool IsRetreating = false;

    public float CirclingDist = 10f;
    public float CirclingDegree = 0f;
    public int CirclingDirectionMul = 0;
    public float CirclingSpeed = 0.1f;

    #endregion

    #region Melee variables

    public CollisionDetector WeaponCD;
    public bool IsSwinging = false;
    public SwingingStates SwingState = SwingingStates.AtRest;
    public SwingDirections SwingDir = SwingDirections.None;
    public float MeleeAttackDist = 2;
    public float SwingSpeed = 0.5f;
    public float UpDownSwingDist = 1f;
    public float LeftRightSwingDist = 1f;
    
    public float UpDownSwing = 0f;
    public float LeftRightSwing = 90f;

    public float LeftSwingingMax = 40f;
    public float RightSwingingMax = 140f;
    // 40 from left to right
    // 140 from right to left
    public float DownSwingingMax = 200f;
    public float WeaponToRestSpeed = 10f;

    #endregion

    #region Ranged variables

    public float RangedAttackDist = 20;
    public ulong FiringInterval = 800;
    public float ProjSpeed = 20;
    public AimPredictionFactors AimIterations = AimPredictionFactors.High;
    public float ProjUpVelocity = 0.5f;

    #endregion

    #region Bull vars

    public float BullAttackDist = 2;
    public int ChanceToCharge = 5_000;
    public float DistRequiredToCharge = 20f;
    public bool IsBullCharging = false;
    public bool ManualChargeTrigger = false;
    public bool BullTurningToCharge = false;
    public float BullTurningSpeed = 10f;
    public bool BullPuttingHeadDown = false;

    public Vector3 BullChargeStart = Vector3.zero;
    public float BullChargeThroughMul = 1.25f;
    public float BullChargeAddition = 5f;
    public float BullInitalChargingDistance = 0f;
    public float BaseBullSpeed = 0f;
    public float BullChargeRotation = 0f;


    public GameObject BullHead;
    public float HeadDownInterpolant = 0f;

    public Vector3 HeadUpPos = new(-1.1920929e-07f, 0.646972656f, 1.35699475f);
    public Vector3 HeadUpRot = new(0f, 90f, 117.968628f);

    public Vector3 HeadDownPos = new(0f, 0.245000005f, 1.60099995f);
    public Vector3 HeadDownRot = new(0f, 90f, 148.800003f);

    #endregion



    public void Start()
    {
        Debug.Log("\"Kael stinks\" - Micah");
        Target = Util.Player;
        PS = Util.Player.GetComponent<PlayerSim>();

        if (EnemyType == EnemyTypes.Human && WeaponType == WeaponTypes.Sword)
        {
            WeaponRestPos = WeaponRestPosTarget.localPosition.Clone();
            WeaponRestRot = WeaponRestPosTarget.localRotation.eulerAngles.Clone();

            WeaponCD = WeaponHandle.GetComponentInChildren<CollisionDetector>();
            if (WeaponCD == null)
                throw new ArgumentNullException("Weapon for human must have a collision detector");
        }
    }

    public void Update()
    {
        // Distance between enemy and target (in meters)
        DistFromTarget = Vector3.Distance(Target.transform.position, transform.position);

        // The Big If
        if (EnemyType == EnemyTypes.Human)
        {
            // Make the target look at the player
            // We can't use the direct Transforms because otherwise the entire enemy faces up towards the target and we don't want that
            transform.LookAt(new Vector3(Target.transform.position.x, transform.position.y, Target.transform.position.z));

            // Chances to do things (these are extremely unstable and there are no cooldowns)
            if (!IsDashing && Util.Random(1, ChanceToDash) == 1 || ManualDashTrigger)
            {
                print("Dash triggered");
                StartCoroutine(DashAttack());
                ManualDashTrigger = false;
            }

            if (!IsCircling && Util.Random(1, ChanceToCircle) == 1 || ManualCircleTrigger)
            {
                print("Circling triggered");
                IsCircling = true;
                // im gonna change this so it adjusts based on where it started
                CirclingDegree = Util.Random(1, 360); // You could also have 0-359 but this looks nicer (0 is equal to 360 when measuring degrees)
                CirclingDirectionMul = Util.Random(1, 2) == 1 ? -1 : 1;
                ManualCircleTrigger = false;
            }

            if (!IsRetreating && HealthPercentage < 25 && Util.Random(1, ChanceToRetreat) == 1)
            {
                print("Retreat triggered");
                IsRetreating = true;
            }

            // We priotise retreating over circling (cowards)
            if (IsRetreating)
            {
                Retreat();
            }
            else if (IsCircling)
            {
                CircleTarget();
            }
            else
            {
                if (WeaponType == WeaponTypes.Bow && DistFromTarget > RangedAttackDist)
                    MoveTowardsTarget();
                else if (WeaponType == WeaponTypes.Sword && DistFromTarget > MeleeAttackDist)
                    MoveTowardsTarget();
                else if (EnemyType == EnemyTypes.Bull)
                    MoveTowardsTarget();
            }


            if (WeaponType == WeaponTypes.Sword && DistFromTarget < MeleeAttackDist)
                Attack();
            else if (WeaponType == WeaponTypes.Bow && DistFromTarget < RangedAttackDist)
                Attack();
            else if (WeaponType == WeaponTypes.Sword)
                ReturnWeaponToRest(); // Currently cooked, but that's OK
        }
        else if (EnemyType == EnemyTypes.Bull)
        {
            if (!BullTurningToCharge && !BullPuttingHeadDown && !IsBullCharging && DistFromTarget <= DistRequiredToCharge && Util.Random(1, ChanceToCharge) == 1 || ManualChargeTrigger)
            {
                print("Charge triggered");
                BullTurningToCharge = true;
                ManualChargeTrigger = false;
            }

            // First, is checking for bull charge prep
            if (BullTurningToCharge)
            {
                // Code based on https://discussions.unity.com/t/slowly-rotate-toward-an-object/763526
                Vector3 PlayerPos = Target.transform.position;
                Vector3 MePos = transform.position;

                Vector3 RelativePos = PlayerPos - MePos;
                // This is the rotation to the target
                Quaternion TargetRotation = Quaternion.LookRotation(RelativePos, Vector3.up);

                float TargetRot = TargetRotation.eulerAngles.y;
                float CurrentRot = transform.rotation.eulerAngles.y;

                if (!Util.WithinRange(CurrentRot, TargetRot, 0.01f))
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, Time.deltaTime * BullTurningSpeed);
                }
                else
                {
                    BullTurningToCharge = false;
                    BullPuttingHeadDown = true;
                    BullChargeRotation = Mathf.Atan2(PlayerPos.z - MePos.z, PlayerPos.x - MePos.x);
                }
            }
            // Then, make the bull put it's head down, in prep for charging
            else if (BullPuttingHeadDown)
            {
                if (HeadDownInterpolant > 1)
                {
                    Vector3 PlayerPos = Target.transform.position;
                    Vector3 MePos = transform.position;

                    BullPuttingHeadDown = false;
                    IsBullCharging = true;
                    BullChargeStart = MePos;
                    BullInitalChargingDistance = Vector3.Distance(PlayerPos, BullChargeStart);
                    BaseBullSpeed = Speed;
                    Speed = 20;
                    HeadDownInterpolant = 0f;
                }
                else
                {
                    BullHead.transform.SetLocalPositionAndRotation(
                        Util.Lerp(HeadUpPos, HeadDownPos, HeadDownInterpolant),
                        Quaternion.Euler(Util.Lerp(HeadUpRot, HeadDownRot, HeadDownInterpolant))
                    );
                    HeadDownInterpolant += 0.001f;
                }
            }
            // Next, check if the bull should be charging
            else if (IsBullCharging)
            {
                if (Vector3.Distance(transform.position, BullChargeStart) > BullInitalChargingDistance * BullChargeThroughMul + BullChargeAddition)
                {
                    IsBullCharging = false;
                    Speed = BaseBullSpeed;
                    BullHead.transform.SetLocalPositionAndRotation(HeadUpPos, Quaternion.Euler(HeadUpRot));
                }
                else
                {
                    // Uses the MoveTowardsTarget math, but subsitutes the auto angle cal with a single calculation
                    // This is because we don't want the direction changing when we attempt to charge through the target
                    Vector3 NewPos = transform.position.Clone();

                    NewPos.x += Mathf.Cos(BullChargeRotation) * Speed * Time.deltaTime;
                    NewPos.z += Mathf.Sin(BullChargeRotation) * Speed * Time.deltaTime;

                    transform.position = NewPos;
                }
            }
            // Finally, regular bull activites
            else
            {
                transform.LookAt(new Vector3(Target.transform.position.x, transform.position.y, Target.transform.position.z));

                if (DistFromTarget < BullAttackDist)
                {
                    Attack();
                }
                else
                {
                    MoveTowardsTarget();
                }
            }
        }

        // Must be in Update and not FixedUpdate, otherwise it cause a burst attack
        Frames++;
    }


    private void MoveTowardsTarget(bool Backwards = false)
    {
        // Step one, record the position of the player and the object
        Vector3 PlayerPos = Target.transform.position;
        Vector3 MePos = transform.position;
        Vector3 NewPos = MePos.Clone();
        
        // Step two, obtain the direction to face the target
        float Rot = Mathf.Atan2(PlayerPos.z - MePos.z, PlayerPos.x - MePos.x);

        // Step three and four, get the direction of the rotation and add the direction * speed * deltaTime (for frame-consistent speed)
        NewPos.x += Mathf.Cos(Rot) * Speed * Time.deltaTime * (Backwards ? -1 : 1);
        NewPos.z += Mathf.Sin(Rot) * Speed * Time.deltaTime * (Backwards ? -1 : 1);

        // Step five, apply the adjusted position
        transform.position = NewPos;
    }

    private void Retreat()
    {
		
    }

    private void CircleTarget()
    {
        float Dist = Vector3.Distance(transform.position, Target.transform.position);
        // The first two give organic (more or less) tracking of the target when maintaining distance, as it doesn't move with the player as it rotates

        float _Shrug_ = Util.IsTheThingInsideOrOutsideTheRangeAndIfNotWhichWayIsItNotInsideTheRange(Dist, CirclingDist);

        if (_Shrug_ == -1)
        {
            // Hijack the basic movement function to also move backwards
            MoveTowardsTarget(true);
        }
        else if (_Shrug_ == 1)
        {
            MoveTowardsTarget();
        }
        else
        {
            Vector2 RotatePos = Util.RotateAroundPoint(Target.transform.position.x, Target.transform.position.z, CirclingDegree, CirclingDist);
            //transform.position.Set(RotatePos.x, transform.position.y, RotatePos.y); // No clue why this doesn't work but the below does
            transform.position = new Vector3(RotatePos.x, transform.position.y, RotatePos.y);
            CirclingDegree += CirclingSpeed * CirclingDirectionMul;

            if (Util.Random(1, ChanceToStopCircling) == 1)
                IsCircling = false;
        }
    }

    private IEnumerator DashAttack()
    {
        IsDashing = true;
        float OriginalSpeed = Speed;
        Speed = (int)DashSpeed;
        yield return new WaitForSeconds(OriginalSpeed / (int)DashSpeed);
        Speed = OriginalSpeed;
        IsDashing = false;
    }

    private void Attack()
    {
        if (WeaponType == WeaponTypes.Sword)
        {
            // i think i might actually be able to make this really simple.

            // if I have the handle as the rotation point, I shouldn't have to worry about one of the axes
            // so, the idea is, first rotate around the hip axes
            // this gives us the x-z position for the swing
            // then rotate around the vertical axes
            // this gives us the height of the swing


            // Switching from out of range to in range
            // Or when going for another swing
            if (
                SwingState == SwingingStates.AtRest ||
                (
                    (SwingDir == SwingDirections.Left && LeftRightSwing == 180) ||
                    (SwingDir == SwingDirections.Right && LeftRightSwing == 0)
                )
            ) {
                SwingState = SwingingStates.Swinging;
                // Pick a starting direction to swing from
                SwingDir = Util.Random(1, 2) == 1 ? SwingDirections.Left : SwingDirections.Right;
                // Set the initial horizontal rotation (Left starts at 180 and right starts at 0)
                LeftRightSwing = SwingDir == SwingDirections.Left ? 180 : 0;
                // Set the initial vertical rotation (starts at 0 because it isn't direction-based)
                UpDownSwing = 0;
                //print("new swing started");
            }



            if (SwingState == SwingingStates.Swinging || SwingState == SwingingStates.Returning)
            {
                // We are required to subtract the enemy's rotation so that the rotation is consistent to the direction the enemy is facing
                Vector2 V2A = Util.RotateAroundPoint(new Vector2(transform.position.x, transform.position.z), LeftRightSwing - transform.rotation.eulerAngles.y, LeftRightSwingDist);
                // We can just bake the calculation directly into the new Vec3
                WeaponHandle.transform.position = new(
                    V2A.x,
                    transform.position.y + Mathf.Sin(Util.DegToRad(UpDownSwing)) * UpDownSwingDist,
                    V2A.y
                );
                // 
                WeaponHandle.transform.LookAt(SwordSwingCenter);
                // This makes it so that we swing the weapon in the right direction depending on which angle
                // It also makes it so that when the sword hits (if it does), the swing direction is reversed before going in for another one
                LeftRightSwing += (SwingDir == SwingDirections.Left ? -SwingSpeed : SwingSpeed) * (SwingState == SwingingStates.Returning ? -1 : 1);

                // If the weapon collides with the player (anything, actually), we make it return so it can swing again
                if (SwingState == SwingingStates.Swinging && WeaponCD.CollisionDetected)
                {
                    SwingState = SwingingStates.Returning;
                    //print("sword collision while swinging");
                }
                // If the swing misses, we make sure that when the sword goes past a certain point, it returns
                if (
                    SwingState == SwingingStates.Swinging &&
                    (
                        (SwingDir == SwingDirections.Right && RightSwingingMax > LeftRightSwing) ||
                        (SwingDir == SwingDirections.Left && LeftSwingingMax > LeftRightSwing)
                    )
                )
                {
                    //print("swing missed. trying again");
                    SwingState = SwingingStates.Returning;
                }

                // This logic is cooked so don't even bother
                /*if (SwingState == SwingingStates.Swinging && DownSwingingMax > UpDownSwing)
                {
                    UpDownSwing += SwingSpeed;
                }
                if (SwingState == SwingingStates.Returning && UpDownSwing > 0)
                {
                    UpDownSwing -= SwingSpeed;
                }*/
            }

            /*

            There are some notes to make about this system:

            The angle used for V2A is how far the swing has gone
            The distance used for V2A is how wide the swing is

            The height of the swing is defined as originY + sin(rot) * dist

            for up and down, 0 and 180 are middle, 90 is top and 270 is bottom

            */
        }
        else if (WeaponType == WeaponTypes.Bow)
        {
            if (Frames % FiringInterval == 0)
            {
                // https://yal.cc/simplest-possible-predictive-aiming/


                Vector3 SpawnPos = WeaponHandle.transform.position;

                Vector3 TStart = Target.transform.position.Clone();
                Vector3 TPose = Target.transform.position.Clone();

                for (int i = 0; i < (int)AimIterations; i++)
                {
                    float Dist = Vector3.Distance(TPose, SpawnPos);
                    float TTR = Dist / ProjSpeed;
                    TPose = TStart + PS.Velocity * TTR;
                }

                float Rot = Mathf.Atan2(TPose.x - SpawnPos.x, TPose.z - SpawnPos.z);

                GameObject Proj = Instantiate(WeaponHandle, SpawnPos, Quaternion.identity);
                Rigidbody RB = Proj.GetComponent<Rigidbody>();
                RB.constraints = RigidbodyConstraints.None;
                RB.velocity = new Vector3(Mathf.Sin(Rot), ProjUpVelocity, Mathf.Cos(Rot)) * ProjSpeed;
                Proj.GetComponent<ArrowAI>().ShouldDestroy = true;
                Proj.GetComponent<MeshCollider>().enabled = true;
                Proj.SetActive(true);
            }
        }
        else
        {
            // This is supposed to be for the bull's attacks but we decided that if it touches you, you take damage
        }
    }

    private bool Block()
    {
        // If we can't block due to having no shield, return false
        // (Not yet sure what I'm going to use that return for yet, but it's there)
        if (Shield == null)
            return false;

        return true;
    }

    private void ReturnWeaponToRest()
    {
        // Gradual return
        if (Frames % 2 == 0 && !Util.AreLocalTransformAndTwoVector3sRepresentingAnotherLocalTransformEqual(WeaponHandle.transform, WeaponRestPos, WeaponRestRot))
        {
            // Position
            if (WeaponHandle.transform.localPosition.x < WeaponRestPos.x)
                WeaponHandle.transform.AddLocalPosition(WeaponToRestSpeed * Time.deltaTime, 0, 0);
            if (WeaponHandle.transform.localPosition.y < WeaponRestPos.y)
                WeaponHandle.transform.AddLocalPosition(0, WeaponToRestSpeed * Time.deltaTime, 0);
            if (WeaponHandle.transform.localPosition.z < WeaponRestPos.z)
                WeaponHandle.transform.AddLocalPosition(0, 0, WeaponToRestSpeed * Time.deltaTime);

            if (WeaponHandle.transform.localPosition.x > WeaponRestPos.x)
                WeaponHandle.transform.AddLocalPosition(-WeaponToRestSpeed * Time.deltaTime, 0, 0);
            if (WeaponHandle.transform.localPosition.y > WeaponRestPos.y)
                WeaponHandle.transform.AddLocalPosition(0, -WeaponToRestSpeed * Time.deltaTime, 0);
            if (WeaponHandle.transform.localPosition.z > WeaponRestPos.z)
                WeaponHandle.transform.AddLocalPosition(0, 0, -WeaponToRestSpeed * Time.deltaTime);

            if (Util.Between(WeaponHandle.transform.localPosition.x, WeaponRestPos.x - WeaponToRestSpeed, WeaponRestPos.x + WeaponToRestSpeed))
                WeaponHandle.transform.localPosition = new Vector3(
                    WeaponRestPos.x,
                    WeaponHandle.transform.localPosition.y,
                    WeaponHandle.transform.localPosition.z
                );
            if (Util.Between(WeaponHandle.transform.localPosition.y, WeaponRestPos.y - WeaponToRestSpeed, WeaponRestPos.y + WeaponToRestSpeed))
                WeaponHandle.transform.localPosition = new Vector3(
                    WeaponHandle.transform.localPosition.x,
                    WeaponRestPos.y,
                    WeaponHandle.transform.localPosition.z
                );
            if (Util.Between(WeaponHandle.transform.localPosition.z, WeaponRestPos.z - WeaponToRestSpeed, WeaponRestPos.z + WeaponToRestSpeed))
                WeaponHandle.transform.localPosition = new Vector3(
                    WeaponHandle.transform.localPosition.x,
                    WeaponHandle.transform.localPosition.y,
                    WeaponRestPos.z
                );

            // Rotation
            if (WeaponHandle.transform.localRotation.eulerAngles.x < WeaponRestRot.x)
                WeaponHandle.transform.AddLocalRotation(WeaponToRestSpeed, 0, 0);
            if (WeaponHandle.transform.localRotation.eulerAngles.y < WeaponRestRot.y)
                WeaponHandle.transform.AddLocalRotation(0, WeaponToRestSpeed, 0);
            if (WeaponHandle.transform.localRotation.eulerAngles.z < WeaponRestRot.z)
                WeaponHandle.transform.AddLocalRotation(0, 0, WeaponToRestSpeed);

            if (WeaponHandle.transform.localRotation.eulerAngles.x > WeaponRestRot.x)
                WeaponHandle.transform.AddLocalRotation(-WeaponToRestSpeed, 0, 0);
            if (WeaponHandle.transform.localRotation.eulerAngles.y > WeaponRestRot.y)
                WeaponHandle.transform.AddLocalRotation(0, -WeaponToRestSpeed, 0);
            if (WeaponHandle.transform.localRotation.eulerAngles.z > WeaponRestRot.z)
                WeaponHandle.transform.AddLocalRotation(0, 0, -WeaponToRestSpeed);

            if (Util.Between(WeaponHandle.transform.localRotation.eulerAngles.x, WeaponRestRot.x - WeaponToRestSpeed, WeaponRestRot.x + WeaponToRestSpeed))
                WeaponHandle.transform.localRotation = Quaternion.Euler(
                    WeaponRestRot.x,
                    WeaponHandle.transform.localRotation.eulerAngles.y,
                    WeaponHandle.transform.localRotation.eulerAngles.z
                );
            if (Util.Between(WeaponHandle.transform.localRotation.eulerAngles.y, WeaponRestRot.y - WeaponToRestSpeed, WeaponRestRot.y + WeaponToRestSpeed))
                WeaponHandle.transform.localRotation = Quaternion.Euler(
                    WeaponHandle.transform.localRotation.eulerAngles.x,
                    WeaponRestRot.y,
                    WeaponHandle.transform.localRotation.eulerAngles.z
                );
            if (Util.Between(WeaponHandle.transform.localRotation.eulerAngles.z, WeaponRestRot.z - WeaponToRestSpeed, WeaponRestRot.z + WeaponToRestSpeed))
                WeaponHandle.transform.localRotation = Quaternion.Euler(
                    WeaponHandle.transform.localRotation.eulerAngles.x,
                    WeaponHandle.transform.localRotation.eulerAngles.y,
                    WeaponRestRot.z
                );
        }
    }

    public void Damage(float DamageToTake)
        => Health -= DamageToTake;

    // An attempt to make some sort of object avoidance system
    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
