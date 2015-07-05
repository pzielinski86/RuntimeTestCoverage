using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using UnityEngine;

namespace Core.Tests.TankControllerSteps.TurretUpdateSteps
{
    [Binding]
    public sealed class TankControllerTurretUpdateSteps
    {
        private readonly TankControllerContext _context;
        private readonly Vector3 _firePointPosition = new Vector3(1, 2, 3);
        private readonly Vector3 _firePointForward = new Vector3(1, 0, 1f);
        public TankControllerTurretUpdateSteps(TankControllerContext context)
        {
            _context = context;
        }

        [When(@"I call TankController\.Update with the world information")]
        public void WhenICallTankController_UpdateWithTheWorldInformation()
        {
            var worldTankInfo = new WorldTankInfo(Vector3.zero, _firePointPosition, _firePointForward, Vector3.forward,
                new Bounds());

            _context.TankController.Update(_context.Battlefield,worldTankInfo);
        }

        [When(@"I call a fire method on cannon")]
        public void WhenICallAFireMethodOnCannon()
        {
            _context.Tank.Turret.Cannon.Fire();
        }
 
        [Then(@"a new bullet should be added into collection")]
        public void ThenANewBulletShouldBeAddedIntoCollection()
        {
            Assert.AreEqual(1, _context.Battlefield.Bullets.Count());
        }

        [Then(@"the bullet start position should be equal to the cannon fire point")]
        public void ThenTheBulletStartPositionShouldBeEqualToTheCannonFirePoint()
        {
            Assert.AreEqual(_firePointPosition, _context.Battlefield.Bullets.ElementAt(0).Position);            
        }

        [Then(@"the bullet direction should be equal to the cannon direction")]
        public void ThenTheBulletDirectionShouldBeEqualToTheCannonDirection()
        {
            Assert.AreEqual(_firePointForward, _context.Battlefield.Bullets.ElementAt(0).Direction);            
        }
    }
}
