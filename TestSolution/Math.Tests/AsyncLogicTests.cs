using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Math.Tests
{
    [TestFixture]
    class AsyncLogicTests
    {
        [Test]
        public async Task Add()
        {

            
            var logic = new AsyncLogic();

            var r = await logic.DoSomethingAsync(5, 5);

            Assert.That(r, Is.EqualTo(10));

        }



    }
}
