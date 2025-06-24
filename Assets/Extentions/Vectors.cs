using Unity.VisualScripting;
using UnityEngine;

namespace Extentions
{
    public static class Vectors
    {
        public static Vector3 OneOver(this Vector3 v)
        {
            v.x = 1 / v.x;
            v.y = 1 / v.y;
            v.z = 1 / v.z;
            return v;
        }

        /// <returns>true if any axe is a zero</returns>
        public static bool HaveZero(this Vector3 v)
        {
            return v.x == 0 || v.y == 0 || v.z == 0;
        }

        public static float Value(this Vector3 v, Axis axis)
        {
            return axis switch
            {
                Axis.x => v.x,
                Axis.y => v.y,
                Axis.z => v.z,
                _ => throw new System.NotImplementedException(),
            };
        }

        public static float HighestValue(this Vector3 v)
        {
            return Mathf.Max(v.x, v.y, v.z);
        }
    }
}
