using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Core.Tests.TankControllerSteps.LocationSteps
{
    [Binding]
    public sealed class TankMoveNoObstaclesSteps
    {
        private readonly TankControllerContext _tankControllerContext;
        public TankMoveNoObstaclesSteps(TankControllerContext tankControllerContext)
        {
            _tankControllerContext = tankControllerContext;
        } 
      
        [Then(@"the tank position should be updated")]
        public void ThenTheTankPositionShouldBeUpdated()
        {
            var tank = _tankControllerContext.TankController.Tank;
            Assert.AreEqual(_tankControllerContext.FramesCountUpdate * tank.Location.Speed * _tankControllerContext.CurrentDirection,
                tank.Location.Position);
        }
    }
}
