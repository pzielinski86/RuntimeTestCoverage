using Core.Math;
using NSubstitute;
using TechTalk.SpecFlow;
using UnityEngine;

namespace Core.Tests.TankControllerSteps
{
    [Binding]
    public sealed class GlobalTankSteps
    {
        private readonly TankControllerContext _tankControllerContext;

        public GlobalTankSteps(TankControllerContext tankControllerContext)
        {
            _tankControllerContext = tankControllerContext;
        }

        [Given(@"a tank bot")]
        public void GivenATankBot()
        {
            _tankControllerContext.PhysicsMock = Substitute.For<IPhysics>();

            var tankMock = Substitute.For<TankBase>();
            
            InitTerrain();
            InitBattlefield(tankMock);

            var tankController = new TankController(_tankControllerContext.PhysicsMock, tankMock, Vector3.zero);
            _tankControllerContext.TankController = tankController;
            InitPhysics();
        }

        private void InitBattlefield(TankBase tankMock)
        {
            _tankControllerContext.Battlefield = new Battlefield(_tankControllerContext.TerrainMock);
            _tankControllerContext.Battlefield.AllTanks.Add(tankMock);
        }

        private void InitTerrain()
        {
            _tankControllerContext.TerrainMock = Substitute.For<ITerrain>();
            _tankControllerContext.TerrainMock.Bounds
                .Returns(new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)));
        }

        private void InitPhysics()
        {
            _tankControllerContext.PhysicsMock.Raycast(Arg.Any<Vector3>(), Arg.Any<Vector3>())
                .Returns(new RaycastResult(null, float.MaxValue));
            _tankControllerContext.PhysicsMock.Contains(Arg.Any<Bounds>(), Arg.Any<Bounds>())
                .Returns(true);
        }

        [Given(@"path without obstacles")]
        public void GivenPathWithoutObstacles()
        {
            _tankControllerContext.PhysicsMock.Raycast(Arg.Any<Vector3>(), Arg.Any<Vector3>()).Returns(new RaycastResult(null, float.MaxValue));
        }

        [When(@"I call a TankController\.Update (.*) times with direction (.*)")]
        public void WhenICallATankController_UpdateTimesWithDirection(int framesCount, Vector3 currentDirection)
        {
            TankController tankController = _tankControllerContext.TankController;

            for (int i = 0; i < framesCount; i++)
            {
                Vector3 position = tankController.Tank.Location.Position;
                const float tankSize = 5f;

                var worldTankInfo = new WorldTankInfo(position, Vector3.zero, Vector3.zero, currentDirection,
                    new Bounds(position, new Vector3(tankSize, tankSize)));

                tankController.Update(_tankControllerContext.Battlefield, worldTankInfo);
            }
            _tankControllerContext.CurrentDirection = currentDirection;
            _tankControllerContext.FramesCountUpdate = framesCount;
        }

        [When(@"I call a move method")]
        public void WhenICallAMoveMethod()
        {
            _tankControllerContext.TankController.Tank.Location.Move();
        }

        [When(@"I call a rotate method")]
        public void WhenICallARotateMethod()
        {
            _tankControllerContext.TankController.Tank.Location.RotateTank(10f);
        }

        [StepArgumentTransformation(@"\((\d*\.?\d*),(\d*\.?\d*),(\d*\.?\d*)\)")]
        public static Vector3 ToVector3(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }
}