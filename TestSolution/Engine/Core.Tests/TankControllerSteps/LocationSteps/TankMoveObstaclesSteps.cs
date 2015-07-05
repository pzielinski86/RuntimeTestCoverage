using Core.Math;
using NSubstitute;
using NUnit.Framework;
using TechTalk.SpecFlow;
using UnityEngine;

namespace Core.Tests.TankControllerSteps.LocationSteps
{
    [Binding]
    public sealed class TankMoveObstaclesSteps
    {
        private readonly TankControllerContext _tankControllerContext;
        public TankMoveObstaclesSteps(TankControllerContext tankControllerContext)
        {
            _tankControllerContext = tankControllerContext;
        }

        [Given(@"path with obstacles")]
        public void GivenPathWithObstacles()
        {
            _tankControllerContext.PhysicsMock.Raycast(Arg.Any<Vector3>(), Arg.Any<Vector3>()).Returns(new RaycastResult(null, 0));
        }

        [Then(@"the tank position should not be updated")]
        public void ThenTheTankPositionShouldNotBeUpdated()
        {
            Assert.AreEqual(Vector3.zero, _tankControllerContext.TankController.Tank.Location.Position);
        }
    }
}
