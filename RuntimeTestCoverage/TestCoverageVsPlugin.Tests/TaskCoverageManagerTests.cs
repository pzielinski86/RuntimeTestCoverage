using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using NSubstitute;
using NUnit.Framework;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    [Timeout(5000)]
    public class TaskCoverageManagerTests
    {
        private TaskCoverageManager _sut;
        private IVsSolutionTestCoverage _vsSolutionTestCoverageMock;
        private TimerMock _timerMock;

        [SetUp]
        public void Setup()
        {
            _vsSolutionTestCoverageMock = Substitute.For<IVsSolutionTestCoverage>();
            _timerMock = new TimerMock();
            _sut = new TaskCoverageManager(_timerMock, _vsSolutionTestCoverageMock);

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [Test]
        public void Should_CalculateCoverage_After_SpecifiedTime_When_ThereAreNoOtherPendingTasks()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";
            const string documentContent = "...";

            // act
            _sut.EnqueueDocumentTask(projectName, documentPath, documentContent);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(1)
                .CalculateForDocumentAsync(projectName, documentPath, documentContent);
        }

        [Test]
        public void Should_PreWarmEngine_After_TaskIsFinished()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";
            const string documentContent = "...";

            // act
            _sut.EnqueueDocumentTask(projectName, documentPath, documentContent);
            _timerMock.ExecuteNow();
            while (_sut.IsBusy) { }

            // assert
            _vsSolutionTestCoverageMock.Received(1).InitAsync(false);
        }

        [Test]
        public void When_SecondDocumentCoverageTaskIsAdded_And_Document_IsNot_Already_InQueue_Then_They_ShouldBeExecutedOneByOne()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath1 = @"c:\\MathHelperTests.cs";
            const string documentPath2 = @"c:\\PathHelperTests.cs";
            const string doc1Content = "document 1 content";
            const string doc2Content = "document 2 content";

            _sut.EnqueueDocumentTask(projectName, documentPath1, doc1Content);

            // act
            _sut.EnqueueDocumentTask(projectName, documentPath2, doc2Content);
            _timerMock.ExecuteNow();

            // assert
            Received.InOrder((() =>
            {
                _vsSolutionTestCoverageMock.CalculateForDocumentAsync(projectName, documentPath1, doc1Content);
                _vsSolutionTestCoverageMock.CalculateForDocumentAsync(projectName, documentPath2, doc2Content);
            }));
        }

        [Test]
        public void When_SecondDocumentCoverageTaskIsAdded_And_Document_I_Already_InQueue_Then_FirstTaskShouldBeUpdated_With_New_Content()
        {
            // arrange
            const string projectName = "MathHelper.Tests";
            const string documentPath = @"c:\\MathHelperTests.cs";
            const string oldContent = "old 1 content";
            const string newContent = "new 2 content";

            _sut.EnqueueDocumentTask(projectName, documentPath, oldContent);

            // act
            _sut.EnqueueDocumentTask(projectName, documentPath, newContent);
            _timerMock.ExecuteNow();

            // assert
            _vsSolutionTestCoverageMock.Received(0).CalculateForDocumentAsync(projectName, documentPath, oldContent);
            _vsSolutionTestCoverageMock.Received(1).CalculateForDocumentAsync(projectName, documentPath, newContent);
        }
    }
}