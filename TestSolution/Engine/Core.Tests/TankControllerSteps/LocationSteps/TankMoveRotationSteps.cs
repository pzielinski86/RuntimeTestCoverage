using NUnit.Framework;
using TechTalk.SpecFlow;
using UnityEngine;

namespace Core.Tests.TankControllerSteps.LocationSteps
{
    [Binding]
    public sealed class TankMoveRotationSteps
    {
        private readonly TankControllerContext _tankControllerContext;
        public TankMoveRotationSteps(TankControllerContext tankControllerContext)
        {
            _tankControllerContext = tankControllerContext;
        }

        [When(@"I call a TankController\.Update until the tank is fully rotated")]
        public void WhenICallATankController_UpdateUntilTheTankIsFullyRotated()
        {
            TankBase tank = _tankControllerContext.TankController.Tank;

            while (tank.Location.RotationY.IsRotationRequired())
            {
                WorldTankInfo worldTankInfo = new WorldTankInfo(tank.Location.Position, Vector3.zero, Vector3.zero, Vector3.forward, new Bounds());
                _tankControllerContext.TankController.Update(_tankControllerContext.Battlefield, worldTankInfo);
            }
        }

        [Then(@"position should not be changed at this time")]
        public void ThenPositionShouldNotBeChangedAtThisTime()
        {
            TankBase tank = _tankControllerContext.TankController.Tank;

            Assert.AreEqual(Vector3.zero, tank.Location.Position);
        }

        [When(@"I call a TankController\.Update again")]
        public void WhenICallATankController_UpdateAgain()
        {
            TankBase tank = _tankControllerContext.TankController.Tank;

            WorldTankInfo worldTankInfo = new WorldTankInfo(tank.Location.Position, Vector3.zero, Vector3.zero, Vector3.forward, new Bounds());
            _tankControllerContext.TankController.Update(_tankControllerContext.Battlefield, worldTankInfo);
        }


        [Then(@"the tank should be first rotated")]
        public void ThenTheTankShouldBeFirstRotated()
        {
            TankBase tank = _tankControllerContext.TankController.Tank;
            Assert.Greater(tank.Location.RotationY.Current, 0);
        }

        [Then(@"the tank should be moved into the requested position")]
        public void ThenTheTankShouldBeMovedIntoTheRequestedPosition()
        {
            Assert.AreNotEqual(Vector3.zero, _tankControllerContext.TankController.Tank.Location.Position);
        }
    }
}
