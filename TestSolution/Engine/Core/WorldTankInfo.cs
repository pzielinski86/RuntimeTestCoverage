using UnityEngine;

namespace Core
{
    public sealed class WorldTankInfo
    {
        private readonly Vector3 _tankPosition;
        private readonly Vector3 _firePointPosition;
        private readonly Vector3 _firePointForward;
        private readonly Vector3 _forward;
        private readonly Bounds _bounds;

        public WorldTankInfo(Vector3 tankPosition, Vector3 firePointPosition,Vector3 firePointForward, Vector3 forward, Bounds bounds)
        {
            _tankPosition = tankPosition;
            _firePointPosition = firePointPosition;
            _firePointForward = firePointForward;
            _forward = forward;
            _bounds = bounds;
        }

        public Vector3 TankPosition
        {
            get { return _tankPosition; }
        }


        public Vector3 Forward
        {
            get { return _forward; }
        }

        public Bounds Bounds
        {
            get { return _bounds; }
        }

        public Vector3 FirePointPosition
        {
            get { return _firePointPosition; }
        }

        public Vector3 FirePointForward
        {
            get { return _firePointForward; }
        }
    }
}
