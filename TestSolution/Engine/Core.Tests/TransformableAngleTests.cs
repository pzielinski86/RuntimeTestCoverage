using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public static class TransformableAngleTests
    {
        [TestCase(0, 80, 80, 80)]
        [TestCase(0, 80, 79, 79)]
        [TestCase(0, 179, 179, 179)]
        [TestCase(0, 350, 10, -10)]
        [TestCase(10, 340, 30, -20)]
        [TestCase(160, 190, 30, 190)]
        [TestCase(340, 10, 30, 370)]
        [TestCase(349, 339, 10, 339)]
        [TestCase(-150, 250, 40, -110)]
        public static  void UpdateTest(int currentAngle, int destinationAngle, int framesCount, int expectedCurrentAngle)
        {
            var transformable = new TransformableAngle(1f, currentAngle);
            transformable.Destination = destinationAngle;
            Assert.IsTrue(transformable.IsRotationRequired());

            for (int frameIndex = 0; frameIndex < framesCount; frameIndex++)
                transformable.Update();
            
            Assert.IsFalse(transformable.IsRotationRequired());
            Assert.AreEqual(expectedCurrentAngle, (int)transformable.Current, delta:2f);
            Assert.AreEqual(1f, transformable.CurrentOffset,delta:0.05f);
        }
    }
}

