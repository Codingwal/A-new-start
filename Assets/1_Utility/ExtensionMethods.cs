using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class ExtensionMethods
    {
        public static Vector2 Abs(this Vector2 v)
        {
            return new(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }
        public static Vector2 Sign(this Vector2 v)
        {
            return new(Mathf.Sign(v.x), Mathf.Sign(v.y));
        }
    }
}