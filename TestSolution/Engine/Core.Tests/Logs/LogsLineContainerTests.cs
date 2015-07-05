using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Logs;
using NUnit.Framework;

namespace Core.Tests.Logs
{
    [TestFixture]
    public class LogsLineContainerTests
    {
        [Test]
        public void ShouldAddNewLine_When_ThereIsCapacity()
        {
            const string newLine = "test";
            var container = new LogsLineContainer(3);

            container.Add(newLine);

            Assert.That(container.Lines.Count(), Is.EqualTo(1));
            Assert.That(container.Lines.ElementAt(0), Is.EqualTo(newLine));
        }

        [Test]
        public void ShouldAddNewLine_And_RemoveFirstOne_When_ThereIsEnoughCapacity()
        {
            const string newLine = "test";
            var container = new LogsLineContainer(2);

            container.Add("A");
            container.Add("B");
            container.Add(newLine);


            Assert.That(container.Lines.ElementAt(0), Is.EqualTo("B"));
            Assert.That(container.Lines.ElementAt(1), Is.EqualTo(newLine));
        }

        [Test]
        public void Text_Should_ReturnJoinedLines()
        {
            var container = new LogsLineContainer();
            
            container.Add("A");
            container.Add("B");
            container.Add("C");

            Assert.That(container.Text,Is.EqualTo("A\r\nB\r\nC"));
        }
    }
}
