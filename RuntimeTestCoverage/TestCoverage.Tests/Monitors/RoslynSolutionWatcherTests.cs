using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using EnvDTE;
using TestCoverage.Monitors;
using TestCoverage.Storage;
using TestCoverage.Tasks;
using TestCoverage.Tests.Utilities;
using Document = EnvDTE.Document;

namespace TestCoverage.Tests.Monitors
{
    [TestFixture]
    public class RoslynSolutionWatcherTests
    {
        private RoslynSolutionWatcher _sut;
        private ICoverageStore _coverageStoreMock;
        private ITaskCoverageManager _testCoverageManagerMock;
        private IRewrittenDocumentsStorage _rewrittenDocumentsStorageMock;
        private AdhocWorkspace _workspace;
        private DTE _dteMock;

        [SetUp]
        public void Setup()
        {
            _workspace = new AdhocWorkspace();
            _dteMock = Substitute.For<DTE>();
            _coverageStoreMock = Substitute.For<ICoverageStore>();
            _rewrittenDocumentsStorageMock = Substitute.For<IRewrittenDocumentsStorage>();
            _testCoverageManagerMock = Substitute.For<ITaskCoverageManager>();

            _sut = new RoslynSolutionWatcher(_dteMock, _workspace, _coverageStoreMock, _rewrittenDocumentsStorageMock, _testCoverageManagerMock);
        }

        [Test]
        public void ChangingDocument_ShouldNotEnqueueResyncAllTask_WhenTheChangedDocumentIsActive()
        {
            // arrange
            var documentMock = Substitute.For<Document>();
            documentMock.FullName.Returns("Code.cs");

            _dteMock.ActiveDocument.Returns(documentMock);
            var testsProject = _workspace.AddProject("Tests", LanguageNames.CSharp);
            
            var doc1 = _workspace.AddDocument(CreateDocumentInfo(testsProject.Id, "Code.cs"));

            _sut.Start();
            var eventWaiter = VerifyWorkspaceChangedEvent(_workspace);

            // act
            var updatedDocument = doc1.WithText(SourceText.From("test"));
            _workspace.TryApplyChanges(updatedDocument.Project.Solution);
            eventWaiter.WaitForEventToFire();

            // assert
            _testCoverageManagerMock.Received(0).ResyncAll();
        }

        [Test]
        public void ChangingDocument_ShouldEnqueueResyncAllTask_When_TheChangedDocumentIsActive()
        {
            // arrange
            var documentMock = Substitute.For<Document>();
            documentMock.FullName.Returns("Code.cs");

            var testsProject = _workspace.AddProject("Tests", LanguageNames.CSharp);            
            var doc1 = _workspace.AddDocument(CreateDocumentInfo(testsProject.Id, "Code.cs"));

            _sut.Start();
            var eventWaiter = VerifyWorkspaceChangedEvent(_workspace);

            // act
            var updatedDocument = doc1.WithText(SourceText.From("test"));
            _workspace.TryApplyChanges(updatedDocument.Project.Solution);
            eventWaiter.WaitForEventToFire();
            
            // assert
            _testCoverageManagerMock.Received(1).ResyncAll();
        }

        [Test]
        public void RemovingDocument_ShouldRemoveCoverage_ForThatFile()
        {
            // arrange
            var testsProject = _workspace.AddProject("Tests", LanguageNames.CSharp);
            const string fileToRemovePath = "MathHelperTests.cs";
            var doc1 = _workspace.AddDocument(CreateDocumentInfo(testsProject.Id, fileToRemovePath));

            _sut.Start();
            var eventWaiter = VerifyWorkspaceChangedEvent(_workspace);

            // act
            var newProject = _workspace.CurrentSolution.Projects.First().RemoveDocument(doc1.Id);
            _workspace.TryApplyChanges(newProject.Solution);
            eventWaiter.WaitForEventToFire();

            // assert
            _coverageStoreMock.Received(1).RemoveByFile(fileToRemovePath);
        }

        [Test]
        public void RemovingDocument_ShouldRemoveCoverage_Of_TheTest_CoveringThatFile()
        {
            // arrange
            var testsProject = _workspace.AddProject("Tests", LanguageNames.CSharp);
            var doc1 = _workspace.AddDocument(CreateDocumentInfo(testsProject.Id, "MathHelper.cs"));

            _sut.Start();
            var eventWaiter = VerifyWorkspaceChangedEvent(_workspace);

            // act
            var newProject = _workspace.CurrentSolution.Projects.First().RemoveDocument(doc1.Id);
            _workspace.TryApplyChanges(newProject.Solution);
            eventWaiter.WaitForEventToFire();

            // assert
            _coverageStoreMock.Received(1).RemoveByDocumentTestNodePath("MathHelper.cs");
        }

        [Test]
        public void RemovingDocument_ShouldRaiseEvent()
        {
            // arrange
            var testsProject = _workspace.AddProject("Tests", LanguageNames.CSharp);
            var doc1 = _workspace.AddDocument(CreateDocumentInfo(testsProject.Id, "MathHelper.cs"));

            _sut.Start();
            var documentRemovedEvent = new EventWaiter();
            _sut.DocumentRemoved += documentRemovedEvent.Wrap<DocumentRemovedEventArgs>(
                (sender, args) => Assert.That(args.DocumentPath == "MathHelper.cs"));

            // act
            var newProject = _workspace.CurrentSolution.Projects.First().RemoveDocument(doc1.Id);
            _workspace.TryApplyChanges(newProject.Solution);

            // assert
            var eventRaised = documentRemovedEvent.WaitForEventToFire(TimeSpan.FromMilliseconds(500));
            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void RemovingDocument_Should_Remove_RewrittenFileCache()
        {
            // arrange
            _workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Default, "Project.sln"));
            var testsProject = _workspace.AddProject("Tests", LanguageNames.CSharp);
            var doc1 = _workspace.AddDocument(CreateDocumentInfo(testsProject.Id, "MathHelper.cs"));

            _sut.Start();
            var eventWaiter = VerifyWorkspaceChangedEvent(_workspace);

            // act
            var newProject = _workspace.CurrentSolution.Projects.First().RemoveDocument(doc1.Id);
            _workspace.TryApplyChanges(newProject.Solution);
            eventWaiter.WaitForEventToFire();

            // assert
            _rewrittenDocumentsStorageMock.Received(1).RemoveByDocument("MathHelper.cs","Tests", "Project.sln");
        }

        private DocumentInfo CreateDocumentInfo(ProjectId projectId, string filePath)
        {
            var docId = DocumentId.CreateNewId(projectId);
            var docInfo = DocumentInfo.Create(docId, filePath, filePath: filePath);

            return docInfo;
        }
        private EventWaiter VerifyWorkspaceChangedEvent(Workspace workspace)
        {
            var wew = new EventWaiter();
            workspace.WorkspaceChanged += wew.Wrap<WorkspaceChangeEventArgs>((sender, args) => { });
            return wew;
        }
    }
}