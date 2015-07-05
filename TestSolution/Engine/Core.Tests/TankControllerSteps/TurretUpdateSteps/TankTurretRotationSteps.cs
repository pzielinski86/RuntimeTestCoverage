using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Core.Tests.TankControllerSteps.TurretUpdateSteps
{
    [Binding]
    public sealed class TankTurretRotation
    {
        private readonly TankControllerContext _context;
        public TankTurretRotation(TankControllerContext context)
        {
            _context = context;
        }

        [When(@"I call a rotate method on turret")]
        public void WhenICallARotateMethodOnTurret()
        {
            _context.Tank.Turret.MoveTurret(50f);
        }

        [Then(@"the tank turret rotation should be updated")]
        public void ThenTheTankTurretRotationShouldBeUpdated()
        {
            Assert.Greater(_context.Tank.Turret.Rotation.Current, 0f);
        }
    }
}
