using System;
using UnityEngine;

namespace Core
{
    public sealed class Bullet
    {
        public event EventHandler BulletDestroyed;

        private readonly TankCannon _cannon;
        private readonly Vector3 _startPosition;
        private bool _isDestroyed;

        public Bullet(TankCannon cannon, Vector3 firePointPosition, Vector3 firePointDirection)
        {
            Speed = 3f;
            Range = 1000f;
            HitPower = 5f;
            _cannon = cannon;
            _startPosition = firePointPosition;
            Position = firePointPosition;
            Direction = firePointDirection;
        }

        public Vector3 Direction { get; private set; }
        public Vector3 Position { get; private set; }
        public float HitPower { get; private set; }
        public float Range { get; private set; }
        public float Speed { get; private set; }

        public TankBase ParentTank
        {
            get { return _cannon.Tank; }
        }

        public void Update()
        {
            if (_isDestroyed)
                return;

            Position = Position + Speed * Direction;

            if (Vector3.Distance(_startPosition, Position) > Range)
                Destroy();
        }

        public void Destroy()
        {
            _isDestroyed = true;
            OnBulletDestroyed();
        }

        private void OnBulletDestroyed()
        {
            if (BulletDestroyed != null)
                BulletDestroyed(this, EventArgs.Empty);
        }
    }
}
