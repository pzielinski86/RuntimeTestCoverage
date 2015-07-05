using Core.Math;
using System;
using UnityEngine;

namespace Core
{
    public sealed class TankController
    {
        public event EventHandler PositionChanged;

        private readonly IPhysics _physics;
        private readonly TankBase _tank;

        public TankController(IPhysics physics, TankBase tank, Vector3 startPosition)
        {
            if (tank == null)
                throw new ArgumentNullException("tank");

            _physics = physics;
            _tank = tank;
            _tank.Location.Position = startPosition;
        }

        public TankBase Tank { get { return _tank; } }

        public void Update(Battlefield battlefield, WorldTankInfo worldTankInfo)
        {
            if (battlefield == null || worldTankInfo == null)
                throw new ArgumentNullException("All parameters must be different than null.", (Exception)null);

            UpdateTankWithWorldData(worldTankInfo);

            _tank.Update(battlefield);

            _tank.Turret.Rotation.Update();
            _tank.Turret.Cannon.Rotation.Update();

            if (!_physics.Contains(battlefield.Terrain.Bounds, _tank.Location.Bounds))
                return;

            if (_tank.Location.RotationY.IsRotationRequired())
                _tank.Location.RotationY.Update();
            else if (_tank.Location.IsMoving)
                MoveTank();
        }

        private void UpdateTankWithWorldData(WorldTankInfo worldTankInfo)
        {
            _tank.Location.Bounds = worldTankInfo.Bounds;
            _tank.Location.Position = worldTankInfo.TankPosition;
            _tank.Location.CurrentDirection = worldTankInfo.Forward;
            _tank.Turret.Cannon.FirePointPosition = worldTankInfo.FirePointPosition;
            _tank.Turret.Cannon.FirePointForward = worldTankInfo.FirePointForward;
        }

        private void MoveTank()
        {
            const string terrainName = "Terrain";

            TankLocation tankLocation = _tank.Location;

            RaycastResult raycastHit = _physics.Raycast(tankLocation.Bounds.center, tankLocation.CurrentDirection);            

            if (raycastHit.ObjectName == terrainName || raycastHit.Distance > tankLocation.Bounds.size.z)
            {
                tankLocation.Position = tankLocation.Position + tankLocation.Speed * tankLocation.CurrentDirection;

                if (PositionChanged != null)
                    PositionChanged(this, EventArgs.Empty);
            }
        }
    }
}

