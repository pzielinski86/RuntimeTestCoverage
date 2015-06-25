using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Math.Tests
{
    [TestFixture]
    public class MathHelperTests2
    {
        [Test]
        public void Test()
        {

        }
    }

    [TestFixture]
    public class MathHelperTests
    {
        [Test]
        public void DivideTest()
        {
            //
            MathHelper helper = new MathHelper();
            decimal result = helper.Divide(10, 2);

            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void DivideTestZero()
        {//
            MathHelper helper = new MathHelper();
            decimal result = helper.Divide(10, 0);

            Assert.That(result, Is.EqualTo(5));
        }
    }

    [TestFixture]
    public class MathHelperTests1
    {
        [Test]
        public void Test()
        {
            
        }
    }
}
