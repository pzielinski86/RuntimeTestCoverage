using Microsoft.VisualStudio.Text;
using NSubstitute;
using NUnit.Framework;
using LiveCoverageVsPlugin.Extensions;

namespace LiveCoverageVsPlugin.Tests.Extensions
{
    [TestFixture]
    public class ITextChangeExtensionsTests
    {
        private ITextChange _textChangeMock;

        [SetUp]
        public void Setup()
        {
            _textChangeMock = Substitute.For<ITextChange>();
        }

        [Test]
        public void AnyCodeChanges_Should_Return_False_When_ThereAreOnlyWhitespaces_In_NewText_And_OldText()
        {
            string changes = "     ";
            _textChangeMock.OldText.Returns(changes);
            _textChangeMock.NewText.Returns(changes);

            bool result = _textChangeMock.AnyCodeChanges();

            Assert.IsFalse(result);
        }

        [Test]
        public void AnyCodeChanges_Should_Return_False_When_ThereAreOnlyNewLines_In_NewText_And_OldText()
        {
            string changes = "\r\n\r\n";
            _textChangeMock.OldText.Returns(changes);
            _textChangeMock.NewText.Returns(changes);

            bool result = _textChangeMock.AnyCodeChanges();

            Assert.IsFalse(result);
        }

        [Test]
        public void AnyCodeChanges_Should_Return_False_When_ThereAreOnlyNewLines_And_Whitespaces_In_NewText_And_OldText()
        {
            string changes = "\r\n    \r\n";
            _textChangeMock.OldText.Returns(changes);
            _textChangeMock.NewText.Returns(changes);

            bool result = _textChangeMock.AnyCodeChanges();

            Assert.IsFalse(result);
        }

        [Test]
        public void AnyCodeChanges_Should_Return_True_When_ThereAreNumbersAndWhitespaces_In_NewText_And_OldText()
        {
            string changes = "343 43 ";
            _textChangeMock.OldText.Returns(changes);
            _textChangeMock.NewText.Returns(changes);

            bool result = _textChangeMock.AnyCodeChanges();

            Assert.IsTrue(result);
        }
    }
}