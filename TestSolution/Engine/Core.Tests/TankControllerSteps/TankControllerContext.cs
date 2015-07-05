using Core.Math;
using UnityEngine;

namespace Core.Tests.TankControllerSteps
{
    public sealed class TankControllerContext
    {
        internal IPhysics PhysicsMock { get; set; }
        internal ITerrain TerrainMock { get; set; }
        internal TankController TankController { get; set; }
        internal Vector3 CurrentDirection { get; set; }
        internal int FramesCountUpdate { get; set; }

        internal TankBase Tank { get { return TankController.Tank; } }
        internal Battlefield Battlefield { get; set; }
    }
}