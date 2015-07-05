using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Core.Math
{
    public static class MathHelper
    {
        public static float GetAngleBetweenForwardVectorAndDestination(Vector3 sourcePosition, Vector3 destinationPosition)
        {
            Vector3 newDirection = destinationPosition - sourcePosition;
            newDirection.Normalize();

            float angle = Vector3.Angle(Vector3.forward, newDirection);
            float sign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(Vector3.forward, newDirection)));

            float signedAngle = angle * sign;
            

            return signedAngle;
        }
    }
}
