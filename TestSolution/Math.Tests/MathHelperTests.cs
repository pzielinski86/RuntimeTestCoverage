﻿using System;
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
            ///////ak55
            MathHelper helper = new MathHelper();
            decimal result = helper.Divide(10, 2);

            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void DivideTestZero()
        {////////////////////ph//ghgjj
            MathHelper helper = new MathHelper();
            decimal result = helper.Divide(10, 0);

            //// Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void DoSOmethingTest()
        {
            ////
            MathHelper helper = new MathHelper();
            helper.DoSomething(9);
        }



    }

    [TestFixture]
    public class MathHelperTests1
    {
        public void Test()
        {

        }
    }
}
