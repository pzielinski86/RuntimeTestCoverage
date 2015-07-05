using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Core.Tests.TankControllerSteps.LocationSteps
{
    [Binding]
    public sealed class TankRotationSteps
    {
        private readonly TankController _tankController;

        public TankRotationSteps(TankControllerContext tankControllerContext)
        {
            _tankController = tankControllerContext.TankController;
        }
     
        [Then(@"the tank rotation should be updated")]
        public void ThenTheTankRotationShouldBeUpdated()
        {
            Assert.Greater(_tankController.Tank.Location.RotationY.Current, 0f);
        }  
    }
}
