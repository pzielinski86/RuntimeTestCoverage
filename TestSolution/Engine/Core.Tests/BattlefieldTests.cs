using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Math;
using NSubstitute;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class BattlefieldTests
    {
        private Battlefield _battlefield;

        [SetUp]
        public void Setup()
        {
            _battlefield=new Battlefield(null);
        }

        [Test]
        public void GetEnemyTanks_Should_ReturnNull_When_ThereAreNoEnemies()
        {
            TankBase myTank = Substitute.For<TankBase>();
            _battlefield.AllTanks.Add(myTank);

            IEnumerable<TankBase> enemyTanks = _battlefield.GetEnemyTanks(myTank);
            Assert.That(enemyTanks.Count(),Is.EqualTo(0));
        }

        [Test]
        public void GetEnemyTanks_Should_ReturnOneTank_When_ThereIsOneEnemy()
        {
            TankBase myTank = Substitute.For<TankBase>();
            TankBase enemy1 = Substitute.For<TankBase>();

            _battlefield.AllTanks.Add(myTank);
            _battlefield.AllTanks.Add(enemy1);

            IEnumerable<TankBase> enemyTanks = _battlefield.GetEnemyTanks(myTank);
            
            Assert.That(enemyTanks.Count(), Is.EqualTo(1));
            Assert.That(enemyTanks.ElementAt(0),Is.EqualTo(enemy1));
        }
    }
}
