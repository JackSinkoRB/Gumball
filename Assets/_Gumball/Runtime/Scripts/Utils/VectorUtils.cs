using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class VectorUtils
    {

        public static Vector3 Flatten(this Vector3 vector3)
        {
            return new Vector3(vector3.x, 0, vector3.z);
        }

        public static Vector2 FlattenAsVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }
        
        public static Vector3 Unflatten(this Vector2 vector2)
        {
            return new Vector3(vector2.x, 0, vector2.y);
        }

    }
}
