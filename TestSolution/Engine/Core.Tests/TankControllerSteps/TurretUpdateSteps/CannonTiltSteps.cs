using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Core.Tests.TankControllerSteps.TurretUpdateSteps
{
    [Binding]
    public sealed class CannonTiltSteps
    {
        private readonly TankControllerContext _context;
        public CannonTiltSteps(TankControllerContext context)
        {
            _context = context;
        }

        [When(@"I call a tilt method on cannon")]
        public void WhenICallATiltMethodOnCannon()
        {
            _context.Tank.Turret.Cannon.Tilt(5f);
        }

        [Then(@"the tank cannon position should be updated\.")]
        public void ThenTheTankCannonPositionShouldBeUpdated_()
        {
            Assert.Greater(_context.Tank.Turret.Cannon.Rotation.Current, 0f);
        }
    }
}
