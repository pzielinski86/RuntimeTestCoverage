using UnityEngine;

namespace Core
{
    public sealed class TransformableAngle
    {
        public TransformableAngle(float speed, float currentAngle)
        {
            Speed = speed;
            Destination = Base = Current = currentAngle;
        }

        public float Speed { get; private set; }
        public float Current { get; private set; }
        public float Destination { get; set; }
        public float Base { get; private set; }
        public float CurrentOffset { get; private set; }


        public bool IsRotationRequired()
        {
            const float deltaTolerance = 2f;

            return GetRealAnglesDistance(Current, Destination) > deltaTolerance;
        }
        public void Update()
        {
            if (!IsRotationRequired())
            {
                Complete();
            }
            else
            {
                CurrentOffset += GetSpeedScaledByDistance(Base, Destination);
                Current = Mathf.LerpAngle(Base, Destination, CurrentOffset);
            }
        }

        public float GetSpeedScaledByDistance(float from, float to)
        {
            return Speed / GetRealAnglesDistance(from, to);
        }

        private static float GetRealAnglesDistance(float to, float from)
        {
            const float fullAngle = 360;

            float dist = System.Math.Abs(@from - to);

            if (dist <= fullAngle / 2f)
                return dist;

            return System.Math.Abs(fullAngle - dist);
        }
        private void Complete()
        {
            Base = Destination = Current;
            CurrentOffset = 1;
        }
    }
}