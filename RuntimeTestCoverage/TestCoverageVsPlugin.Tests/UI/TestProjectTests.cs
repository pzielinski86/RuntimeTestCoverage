using NSubstitute;
using NUnit.Framework;
using TestCoverage.Storage;
using TestCoverageVsPlugin.UI.ViewModels;

namespace TestCoverageVsPlugin.Tests.UI
{
    [TestFixture]
    public class TestProjectTests
    {
        private TestProjectViewModel _sut;
        private ICoverageSettingsStore _coverageSettingsStore;

        [SetUp]
        public void Setup()
        {
            _coverageSettingsStore = Substitute.For<ICoverageSettingsStore>();

            _sut =new TestProjectViewModel(_coverageSettingsStore);
            _sut.TestProjectSettings=new TestProjectSettings();
        }

        [Test]
        public void ShouldIgnoreProject_When_ProjectIsUnignored()
        {
            _sut.TestProjectSettings.IsCoverageEnabled = true;

            _sut.FlagProjectCoverageSettingsCmd.Execute(null);

            Assert.IsFalse(_sut.TestProjectSettings.IsCoverageEnabled);
        }

        [Test]
        public void ShouldUnignoreProject_When_ProjectIsIgnored()
        {
            _sut.TestProjectSettings.IsCoverageEnabled = false;

            _sut.FlagProjectCoverageSettingsCmd.Execute(null);

            Assert.IsTrue(_sut.TestProjectSettings.IsCoverageEnabled);
        }
    }
}