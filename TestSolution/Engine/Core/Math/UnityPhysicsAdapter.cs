using UnityEngine;

namespace Core.Math
{
    public sealed class UnityPhysicsAdapter : IPhysics
    {
        public RaycastResult Raycast(Vector3 position, Vector3 currentDirection)
        {
            RaycastHit unityRaycastHit;
            if (Physics.Raycast(position, currentDirection, out unityRaycastHit))
            {
                string objectName = null;

                if (unityRaycastHit.transform != null)
                    objectName = unityRaycastHit.transform.name;

                return new RaycastResult(objectName, unityRaycastHit.distance);
            }

            return new RaycastResult(null, float.MaxValue);
        }

        public bool Contains(Bounds largerBox, Bounds smallerBox)
        {
            return largerBox.Contains(smallerBox.min) && largerBox.Contains(smallerBox.max);
        }
    }
}