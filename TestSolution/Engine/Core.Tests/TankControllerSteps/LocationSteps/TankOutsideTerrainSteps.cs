
using NSubstitute;
using NUnit.Framework;
using TechTalk.SpecFlow;
using UnityEngine;

namespace Core.Tests.TankControllerSteps.LocationSteps
{
    [Binding]
    public class TankOutsideTerrainSteps
    {
        private readonly TankControllerContext _tankControllerContext;

        public TankOutsideTerrainSteps(TankControllerContext tankControllerContext)
        {
            _tankControllerContext = tankControllerContext;
        }

        [When(@"tank is out of terrain")]
        public void WhenTankIsOutOfTerrain()
        {
            _tankControllerContext.PhysicsMock.Contains(Arg.Any<Bounds>(), Arg.Any<Bounds>())
           .Returns(false);
        }

        [Then(@"the tank should not move")]
        public void ThenTheTankShouldNotMove()
        {            
            Assert.AreEqual(Vector3.zero,_tankControllerContext.Tank.Location.Position);
        }
    }
}
