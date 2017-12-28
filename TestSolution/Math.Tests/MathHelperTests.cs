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

        public void Test()
        {

        }
    }

    [TestFixture]
    public class MathHelperTests
    {
        [TestCase(20,10,2)]
        [TestCase(30, 10, 3)]
        public void DivideTest(int a,int b,int c)
        {

            MathHelper helper = new MathHelper();
            decimal result = helper.Divide(a, b);
            
            Assert.That(result, Is.EqualTo(c));

        }



        [Test]
        public void DivideTestZero()
        {////////////////////ph//ghgjj
            MathHelper helper = new MathHelper();










            decimal result = helper.Divide(10, 5);

              

             Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void DoSOmethingTest()
        {
            ////
            MathHelper helper = new MathHelper();
            helper.DoSomething(4);
            Assert.IsTrue(true);
        }
          
        [Test]
        public void DoSOmethingTes131t()
        {
            ////
            MathHelper helper = new MathHelper();

            Assert.IsTrue(true);


     //      helper.DoSomething(5); 
        }

        [Test]
        public void DoSOmethingTest2()
        {
            ////
            MathHelper helper = new MathHelper();
            helper.DoSomething(451);
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
