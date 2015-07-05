using System;
using Core.Collections;
using UnityEngine;

namespace Core
{
    public sealed class TankCannon
    {
        public event EventHandler<BulletFireEventArgs> BulletFired;

        public TankCannon(TankBase tank, float cannonRotationSpeed)
        {
            Rotation=new TransformableAngle(cannonRotationSpeed,0);
            Tank = tank;
        }

        public Vector3 FirePointPosition { get; internal set; }        
        public TransformableAngle Rotation { get; private set; }
        public Vector3 FirePointForward { get; internal set; }
        public TankBase Tank { get; private set; }

        public void Fire()
        {
            var bullet = new Bullet(this, FirePointPosition, FirePointForward);
            OnBulletFired(bullet);
        }

        public void Tilt(float angle)
        {
            const int minValue = -30;
            const int maxValue = 10;

            float destinationAngle = angle;

            if (destinationAngle < minValue)
                destinationAngle = minValue;
            if (destinationAngle > maxValue)
                destinationAngle = maxValue;

           Rotation.Destination = destinationAngle;
        }

        private void OnBulletFired(Bullet bullet)
        {
            if (BulletFired != null)
                BulletFired(this,new BulletFireEventArgs(bullet));
        }

    }
}
