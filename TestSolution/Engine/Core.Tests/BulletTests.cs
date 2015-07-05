
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Core.Tests
{
    [TestFixture]
    public sealed class BulletTests
    {
        private TankBase _tankMock ;

        [SetUp]
        public void Setup()
        {
            _tankMock = Substitute.For<TankBase>();
        }

        [Test]
        public void PositionShouldBeUpdated()
        {
            var startPosition = new Vector3(5, 5, 5);
            var direction = new Vector3(5, 10, 15);
            var bullet = new Bullet(_tankMock.Turret.Cannon,startPosition, direction);

            const int framesCount = 5;
            for (int i = 0; i < framesCount; i++)
                bullet.Update();

            Assert.AreEqual(startPosition + framesCount * direction * bullet.Speed, bullet.Position);
        }

        [Test]
        public void BulletShouldDisappearWhenRangeIsExceeded()
        {
            var bullet = new Bullet(_tankMock.Turret.Cannon, Vector3.zero, Vector3.forward);
           
            var framesCount =(int) System.Math.Ceiling(bullet.Range/bullet.Speed);

            bool isDestroyed = false;

            bullet.BulletDestroyed += (sender,e) => { isDestroyed = true; };

            for (int i = 0; i < framesCount; i++)
                bullet.Update();

            Assert.IsTrue(isDestroyed);
            
        }
    }
}
