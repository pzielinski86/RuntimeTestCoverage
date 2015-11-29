using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using NUnit.Framework;
using System.Threading;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    [Timeout(5000)]
    public class TaskCoverageManagerTests
    {
        private TaskCoverageManager _sut;
        private IVsSolutionTestCoverage _vsSolutionTestCoverageMock;
        private ITextSnapshot _textSnapshotMock;
        private IDocumentFromTextSnapshotExtractor _documentFromTextSnapshotExtractorMock;
        private TimerMock _timerMock;

        [SetUp]
        public void Setup()
        {
            _vsSolutionTestCoverageMock = Substitute.For<IVsSolutionTestCoverage>();
            _timerMock = new TimerMock();
            _textSnapshotMock = Substitute.For<ITextSnapshot>();
            _documentFromTextSnapshotExtractorMock = Substitute.For<IDocumentFromTextSnapshotExtractor>();
            _sut = new TaskCoverageManager(_timerMock, _vsSolutionTestCoverageMock, _documentFromTextSnapshotExtractorMock);

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [Test]
        public void Should_AcceptOnlyCsharpDocuments()
        {
            // arrange

            // act
            _sut.EnqueueMethodTask("TestProject",0,null,"File.txt");

            // assert
             Assert.IsFalse(_sut.IsBusy);
        }

        [Test]
        public void Should_CalculateCoverage_After_SpecifiedTime_When_ThereAreNoOtherPendingTasks()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";
            const int position = 3543;
            SyntaxNode node = CSharpSyntaxTree.ParseText("class Test{}").GetRoot();
            _documentFromTextSnapshotExtractorMock.ExtactDocument(_textSnapshotMock).Returns(node);

            // act
            _sut.EnqueueMethodTask(projectName, position, _textSnapshotMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForSelectedMethodAsync(projectName, position, node);
        }

        [Test]
        public void When_SecondDocumentCoverageTaskIsAdded_And_Document_IsNot_Already_InQueue_Then_They_ShouldBeExecutedOneByOne()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath1 = @"c:\\MathHelperTests1.cs";
            const string documentPath2 = @"c:\\PathHelperTests2.cs";
            const int position = 3543;

            ITextSnapshot snapshot1 = Substitute.For<ITextSnapshot>();
            SyntaxNode node1 = CSharpSyntaxTree.ParseText("class Doc1{}").GetRoot();
            _documentFromTextSnapshotExtractorMock.ExtactDocument(snapshot1).Returns(node1);

            ITextSnapshot snapshot2 = Substitute.For<ITextSnapshot>();
            SyntaxNode node2 = CSharpSyntaxTree.ParseText("class Doc2{}").GetRoot();
            _documentFromTextSnapshotExtractorMock.ExtactDocument(snapshot2).Returns(node2);

            _sut.EnqueueMethodTask(projectName, position, snapshot1, documentPath1);

            // act
            _sut.EnqueueMethodTask(projectName, position, snapshot2, documentPath2);
            _timerMock.ExecuteNow();

            // assert
            Received.InOrder((() =>
            {
                _vsSolutionTestCoverageMock.CalculateForSelectedMethodAsync(projectName, position, node1);
                _vsSolutionTestCoverageMock.CalculateForSelectedMethodAsync(projectName, position, node2);
            }));
        }

        [Test]
        public void When_SecondMethodCoverageTaskIsAdded_And_Document_I_Already_InQueue_Then_FirstTaskShouldBeUpdated_With_New_Content()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";
            const int position = 3543;

            ITextSnapshot snapshot1 = Substitute.For<ITextSnapshot>();
            SyntaxNode node1 = CSharpSyntaxTree.ParseText("class Doc1{}").GetRoot();
            _documentFromTextSnapshotExtractorMock.ExtactDocument(snapshot1).Returns(node1);

            ITextSnapshot snapshot2 = Substitute.For<ITextSnapshot>();
            SyntaxNode node2 = CSharpSyntaxTree.ParseText("class Doc2{}").GetRoot();
            _documentFromTextSnapshotExtractorMock.ExtactDocument(snapshot2).Returns(node2);

            _sut.EnqueueMethodTask(projectName, position, snapshot1, documentPath);

            // act
            _sut.EnqueueMethodTask(projectName, position, snapshot2, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(0).CalculateForSelectedMethodAsync(projectName, position, node1);
            _vsSolutionTestCoverageMock.Received(1).CalculateForSelectedMethodAsync(projectName, position, node2);
        }
    }
}