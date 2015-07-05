namespace Core
{
    public struct TankTurret
    {
        private readonly TransformableAngle _rotation;
        private readonly TankCannon _cannon;

        public TankTurret(TankBase tank, float turretRotationSpeed, float cannonRotationSpeed)
        {
            _rotation=new TransformableAngle(turretRotationSpeed,0);
            _cannon = new TankCannon(tank, cannonRotationSpeed);

        }

        public TransformableAngle Rotation
        {
            get { return _rotation; }
        }

        public TankCannon Cannon
        {
            get { return _cannon; }
        }

        public void MoveTurret(float destinationAngle)
        {
            Rotation.Destination = destinationAngle;
        }  
    }
}
