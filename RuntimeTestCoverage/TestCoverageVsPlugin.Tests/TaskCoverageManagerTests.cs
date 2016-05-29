using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverageVsPlugin.Tasks;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    public class TaskCoverageManagerTests
    {
        private TaskCoverageManager _sut;
        private IVsSolutionTestCoverage _vsSolutionTestCoverageMock;
        private IDocumentProvider _documentProviderMock;
        private ITextBuffer _textBufferMock;
        private TimerMock _timerMock;

        [SetUp]
        public void Setup()
        {
            _vsSolutionTestCoverageMock = Substitute.For<IVsSolutionTestCoverage>();
            _timerMock = new TimerMock();
            _documentProviderMock = Substitute.For<IDocumentProvider>();

            var textSnapshotMock = Substitute.For<ITextSnapshot>();
            _textBufferMock = Substitute.For<ITextBuffer>();
            _textBufferMock.CurrentSnapshot.Returns(textSnapshotMock);

            _sut = new TaskCoverageManager(_timerMock, _documentProviderMock, _vsSolutionTestCoverageMock);

            var taskSchedulerMock = Substitute.For<ITaskSchedulerManager>();
            SynchronizationContext.SetSynchronizationContext(new TestSyncContext());
            var synchronizationTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskSchedulerMock.FromSynchronizationContext().Returns(synchronizationTaskScheduler);

            TaskSchedulerManager.Current = taskSchedulerMock;
        }

        [Test]
        public void EnqueueMethodTask_Should_AcceptOnlyCsharpDocuments()
        {
            // arrange

            // act
            _sut.EnqueueMethodTask("TestProject", 0, null, "File.txt");

            // assert
            Assert.IsFalse(_sut.IsBusy);
        }

        [Test]
        public void Should_CalculateDocumentCoverage_After_SpecifiedTime_When_ThereAreNoOtherPendingTasks()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";
            const string documentContent = "class Tests{ [Test]public void Test1(){}}";

            _textBufferMock.CurrentSnapshot.GetText().Returns(documentContent);

            // act
            _sut.EnqueueDocumentTask(projectName, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForDocumentAsync(projectName, documentPath, documentContent);
        }

        [Test]
        public void CalculateDocumentCoverage_After_SuccessfulCompletion_Should_Invalidate_MethodExecutionTasks_Which_BelongTo_TheSameDocument()
        {
            // arrange 
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";

            var code = "class Tests{ [Test]public void Test1(){}}";
            int position = code.IndexOf("Test1");
            _textBufferMock.CurrentSnapshot.GetText().Returns(code);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            _documentProviderMock.GetSyntaxNodeFromTextSnapshot(_textBufferMock.CurrentSnapshot).Returns(syntaxTree.GetRoot());
            _vsSolutionTestCoverageMock.CalculateForDocumentAsync(projectName, documentPath, code)
                .Returns(Task.FromResult(true));

            // act
            _sut.EnqueueDocumentTask(projectName, _textBufferMock, documentPath);
            _sut.EnqueueMethodTask(projectName, position, _textBufferMock, documentPath);            
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(0)
                .CalculateForSelectedMethodAsync(Arg.Any<string>(), Arg.Any<MethodDeclarationSyntax>());

            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForDocumentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void CalculateDocumentCoverage_After_UnsuccessfulCompletion_ShouldNot_Invalidate_MethodExecutionTasks_Which_BelongTo_TheSameDocument()
        {
            // arrange 
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";

            var code = "class Tests{ [Test]public void Test1(){}}";
            int position = code.IndexOf("Test1");
            _textBufferMock.CurrentSnapshot.GetText().Returns(code);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            _documentProviderMock.GetSyntaxNodeFromTextSnapshot(_textBufferMock.CurrentSnapshot).Returns(syntaxTree.GetRoot());
            _vsSolutionTestCoverageMock.CalculateForDocumentAsync(projectName, documentPath, code)
                .Returns(Task.FromResult(false));
            // act
            _sut.EnqueueDocumentTask(projectName, _textBufferMock, documentPath);
            _sut.EnqueueMethodTask(projectName, position, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForSelectedMethodAsync(Arg.Any<string>(), Arg.Any<MethodDeclarationSyntax>());

            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForDocumentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Should_CalculateCoverageDocument_Only_OneTime_When_TwoTheSameDocuments_Were_Requested()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";

            // act
            _sut.EnqueueDocumentTask(projectName, _textBufferMock, documentPath);
            _sut.EnqueueDocumentTask(projectName, _textBufferMock, documentPath);

            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForDocumentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Should_CalculateMethodCoverage_After_SpecifiedTime_When_ThereAreNoOtherPendingTasks()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";

            var code = "class Tests{ [Test]public void Test1(){}}";
            int position = code.IndexOf("Test1");
            _textBufferMock.CurrentSnapshot.GetText().Returns(code);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            _documentProviderMock.GetSyntaxNodeFromTextSnapshot(_textBufferMock.CurrentSnapshot).Returns(syntaxTree.GetRoot());

            // act
            _sut.EnqueueMethodTask(projectName, position, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForSelectedMethodAsync(projectName, Arg.Is<MethodDeclarationSyntax>(x => x.Identifier.ToString() == "Test1"));
        }

        [Test]
        public void When_SecondMethodCoverageTaskIsAdded_And_Method_IsNot_Already_InQueue_Then_They_ShouldBeExecutedOneByOne()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests1.cs";

            var code = "class Tests{ " +
                       "[Test]public void Test1()" +
                       "{}" +
                       "[Test]public void Test2()" +
                       "{}" +
                       "}";

            int method1Position = code.IndexOf("Test1");
            int method2Position = code.IndexOf("Test2");

            _textBufferMock.CurrentSnapshot.GetText().Returns(code);

            _sut.EnqueueMethodTask(projectName, method1Position, _textBufferMock, documentPath);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            _documentProviderMock.GetSyntaxNodeFromTextSnapshot(_textBufferMock.CurrentSnapshot).Returns(syntaxTree.GetRoot());

            // act
            _sut.EnqueueMethodTask(projectName, method2Position, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1).CalculateForSelectedMethodAsync(projectName,
                  Arg.Is<MethodDeclarationSyntax>(x => x.Identifier.ValueText == "Test1"));
            _vsSolutionTestCoverageMock.Received(1).CalculateForSelectedMethodAsync(projectName,
                Arg.Is<MethodDeclarationSyntax>(x => x.Identifier.ValueText == "Test2"));
        }

        [Test]
        public void When_SecondMethodCoverageTaskIsAdded_And_Method_Is_Already_InQueue_Then_Method_Should_Be_ExecutedOnlyOneTime()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests1.cs";

            var code = "class Tests{ " +
                       "[Test]public void Test1()" +
                       "{}" +
                       "[Test]public void Test2()" +
                       "{}" +
                       "}";

            int method1Position = code.IndexOf("Test1");

            _textBufferMock.CurrentSnapshot.GetText().Returns(code);

            _sut.EnqueueMethodTask(projectName, method1Position, _textBufferMock, documentPath);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            _documentProviderMock.GetSyntaxNodeFromTextSnapshot(_textBufferMock.CurrentSnapshot).Returns(syntaxTree.GetRoot());

            // act
            _sut.EnqueueMethodTask(projectName, method1Position, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1).CalculateForSelectedMethodAsync(projectName,
                Arg.Is<MethodDeclarationSyntax>(x => x.Identifier.ValueText == "Test1"));
        }

        [Test]
        public void UnsuccessfulDocumentTask_ShouldBe_Retried_After_AnySucessfulOperationInThatDocument()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests1.cs";

            var code = "class Tests{ " +
                       "[Test]public void Test1()" +
                       "{}" +
                       "}";

            int method1Position = code.IndexOf("Test1");
            _textBufferMock.CurrentSnapshot.GetText().Returns(code);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            _documentProviderMock.GetSyntaxNodeFromTextSnapshot(_textBufferMock.CurrentSnapshot).Returns(syntaxTree.GetRoot());
            _vsSolutionTestCoverageMock.CalculateForDocumentAsync(projectName, documentPath, code).Returns(false);
            _vsSolutionTestCoverageMock.CalculateForSelectedMethodAsync(Arg.Any<string>(), Arg.Any<MethodDeclarationSyntax>()).
                Returns(true);

            // act
            _sut.EnqueueDocumentTask(projectName, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            _sut.EnqueueMethodTask(projectName, method1Position, _textBufferMock, documentPath);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(2).CalculateForDocumentAsync(projectName,documentPath, code);
        }
    }
}