using System;
using UnityEngine;

namespace Assets.T_Unit_Assets.Scripts
{
    public static class Util
    {
        #region Properties

        // Auto-seeded
        public static System.Random Rand = new(new System.Random().Next());

        public static GameObject Player = GameObject.FindGameObjectWithTag("Player");

        #endregion

        #region Methods
        public static float Lerp(float A, float B, float N)
            => A + (B - A) * N;

        /// <summary>
        /// Compares the position and rotation of two Transforms
        /// </summary>
        /// <param name="A">Transform A</param>
        /// <param name="B">Transform B</param>
        /// <returns>True if the position and rotations of both transforms are exactly equal; false otherwise.</returns>
        public static bool AreTransformsEqual(Transform A, Transform B)
            => A.position.x == B.position.x &&
               A.position.y == B.position.y &&
               A.position.z == B.position.z &&
               A.rotation.x == B.rotation.x &&
               A.rotation.y == B.rotation.y &&
               A.rotation.z == B.rotation.z;
        public static bool Between(float V, float Min, float Max)
            => V > Min && Max > V;
        public static bool BetweenEqual(float V, float Min, float Max)
            => V >= Min && Max >= V;
        public static Vector2 RotateAroundPoint(float CX, float CY, float Deg, float Dist)
            => new(CX + Mathf.Cos(DegToRad(Deg)) * Dist, CY + Mathf.Sin(DegToRad(Deg)) * Dist);
        // Yay, overloads that I'm never going to use
        public static Vector2 RotateAroundPoint(Vector2 CenterPoint, float Deg, float Dist)
            => RotateAroundPoint(CenterPoint.x, CenterPoint.y, Deg, Dist);
        public static float DegToRad(float Deg)
            => Deg * Mathf.PI / 180;
        public static Vector3 Lerp(Vector3 A, Vector3 B, float N)
            => new(
                Lerp(A.x, B.x, N),
                Lerp(A.y, B.y, N),
                Lerp(A.z, B.z, N)
            );
        public static Vector3 DivideVec3(Vector3 A, Vector3 B)
            => new(
                A.x / B.x,
                A.y / B.y,
                A.z / B.z
            );
        /// <summary>
        /// To let this make sense, imagine a point you want to rotate around.<br/>
        /// Imagine that you are behind it, facing its back<br/>
        /// Note that in this circumstance, X is forwards and backwards, Y is up and down and Z is left and right
        /// In 3d space, there are 3 different rotations you can have:<br/>
        /// Roll: the rotation that controls if the point is falling to the left or right<br/>
        /// Pitch: the rotation that controls if the point is looking up or down<br/>
        /// Yaw: the rotation that controls if the point is LOOKING left or right<br/>
        /// Because of this, there are 3 rotations/rotated positions we need to decide on<br/>
        /// <br/>
        /// First is roll.<br/>
        /// For this, the z (left and right) acts as the 2d x axis and y retains its use (up and down)<br/>
        /// <br/>
        /// Second is pitch.<br/>
        /// For this, both x and y retain their normal uses (left and right, up and down)<br/>
        /// <br/>
        /// Last is yaw.<br/>
        /// For this, the z (left and right) acts as the 2d x axis and x (forwards and backwards) acts as the y axis<br/>
        /// <br/>
        /// Now we face the next challenge, which is we have 6 positions (2 values * 3 operations). How do we know which ones we need?<br/>
        /// Well, testing the method so far reveals some interesting coincidents<br/>
        /// The rolled y and pitched y are similar and so are pitched x and yawed y<br/>
        /// </summary>
        /// <param name="Point">The point to rotate from</param>
        /// <param name="Deg">A vector describing the rotations for each operation. Order is Roll, Pitch, Yaw</param>
        /// <param name="Dist">A vector describing the distances for each operation. Order is Roll, Pitch, Yaw</param>
        /// <returns>A final rotated position</returns>
        public static Vector3 ThreeDRotateAroundPoint(Vector3 Point, Vector3 Deg, Vector3 Dist)
        {
            Vector2 RollPos = new(Point.z, Point.y);
            Vector2 RolledPos = RotateAroundPoint(RollPos, Deg.x, Dist.x);

            Vector2 PitchPos = new(Point.x, Point.y);
            Vector2 PitchedPos = RotateAroundPoint(PitchPos, Deg.y, Dist.y);

            Vector2 YawPos = new(Point.z, Point.x);
            Vector2 YawedPos = RotateAroundPoint(YawPos, Dist.z, Dist.z);

            // We aren't using YawedPos.x
            return new(
                (PitchedPos.x + YawedPos.y) / 2,
                (RolledPos.y + PitchedPos.y) / 2,
                RolledPos.x
            );
        }
        public static float Constrain(float N, float Min, float Max)
            => Mathf.Min(Mathf.Max(N, Min), Max);

        #endregion

        #region Extension methods

        public static void AddPosition(this Transform Trans, float X, float Y, float Z)
        {
            Vector3 V3 = Trans.position;
            V3.x += X;
            V3.y += Y;
            V3.z += Z;
            Trans.position = V3;
        }
        public static void AddPosition(this Transform Trans, Vector3 XYZ)
        {
            Vector3 V3 = Trans.position;
            V3 += XYZ;
            Trans.position = V3;
        }
        public static void AddRotation(this Transform Trans, float X, float Y, float Z)
        {
            Vector3 V3 = Trans.rotation.eulerAngles;
            V3.x += X;
            V3.y += Y;
            V3.z += Z;
            Trans.rotation = Quaternion.Euler(V3);
        }
        public static void AddRotation(this Transform Trans, Vector3 XYZ)
        {
            Vector3 V3 = Trans.rotation.eulerAngles;
            V3 += XYZ;
            Trans.rotation = Quaternion.Euler(V3);
        }
        public static void AddLocalPosition(this Transform Trans, float X, float Y, float Z)
        {
            Vector3 V3 = Trans.localPosition;
            V3.x += X;
            V3.y += Y;
            V3.z += Z;
            Trans.localPosition = V3;
        }
        public static void AddLocalPosition(this Transform Trans, Vector3 XYZ)
        {
            Vector3 V3 = Trans.localPosition;
            V3 += XYZ;
            Trans.localPosition = V3;
        }
        public static void AddLocalRotation(this Transform Trans, float X, float Y, float Z)
        {
            Vector3 V3 = Trans.localRotation.eulerAngles;
            V3.x += X;
            V3.y += Y;
            V3.z += Z;
            Trans.localRotation = Quaternion.Euler(V3);
        }
        public static void AddLocalRotation(this Transform Trans, Vector3 XYZ)
        {
            Vector3 V3 = Trans.localRotation.eulerAngles;
            V3 += XYZ;
            Trans.localRotation = Quaternion.Euler(V3);
        }
        public static Vector3 Clone(this Vector3 Target)
            => new(Target.x, Target.y, Target.z);

        // Exp
        public static float X(this Transform Trans)
            => Trans.position.x;
        public static float Y(this Transform Trans)
            => Trans.position.y;
        public static float Z(this Transform Trans)
            => Trans.position.z;

        #endregion
    }

    public enum WeaponType
    {
        Sword,
        Bow
    }

    public enum DashSpeed
    {
        None = 0,
        Slow = 10,
        Medium = 20,
        Fast = 30,
        Ludicruous = 50,
        InfernumDoGDash = 100
    }

    public enum AimPredictionFactor
    {
        /// <summary>
        /// 0 aim predictions
        /// </summary>
        None,
        /// <summary>
        /// 1 aim prediction
        /// </summary>
        VeryLow,
        /// <summary>
        /// 2 aim predictions
        /// </summary>
        Low,
        /// <summary>
        /// 3 aim predictions
        /// </summary>
        Medium,
        /// <summary>
        /// 4 aim predictions
        /// </summary>
        High,
        /// <summary>
        /// 16 aim predictions
        /// </summary>
        AbsolutePerfection = 16
    }
}
