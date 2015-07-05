using UnityEngine;

namespace Core
{
    public sealed class TankLocation
    {
        public TankLocation(float speed, float tankRotationSpeed)
        {
            Speed = speed;
            CurrentDirection = Vector3.forward;
            RotationY = new TransformableAngle(tankRotationSpeed, 0);
        }

        private bool _isMoveOperationRequested;

        public float Speed { get; private set; }
        public Vector3 Position { get; internal set; }
        public Vector3 CurrentDirection { get; set; }
        public Bounds Bounds { get; set; }                 
        public TransformableAngle RotationY { get; private set; }
        public bool IsMoving
        {
            get { return _isMoveOperationRequested && !RotationY.IsRotationRequired(); }
        }    
  
        public void RotateTank(float angle)
        {
            RotationY.Destination = angle;
        }

        public void Move()
        {
            _isMoveOperationRequested = true;
        }

        public void Stop()
        {
            _isMoveOperationRequested = false;
        }
    }
}
