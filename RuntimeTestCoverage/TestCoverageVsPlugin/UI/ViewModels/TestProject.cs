using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using TestCoverage.Storage;

namespace TestCoverageVsPlugin.UI.ViewModels
{
    public class TestProject
    {
        private readonly ICoverageSettingsStore _coverageSettingsStore;

        public TestProject(ICoverageSettingsStore coverageSettingsStore)
        {
            _coverageSettingsStore = coverageSettingsStore;
            FlagProjectCoverageSettingsCmd = new DelegateCommand(FlagProjectCoverageSettings);
        }

        public TestProjectSettings TestProjectSettings { get; set; }

        public TestFixture[] TestFixtures { get; set; }

        public ICommand FlagProjectCoverageSettingsCmd { get; set; }

        public string FlagProjectCoverageSettingsCmdText => TestProjectSettings.IsCoverageEnabled ? "Unignore" : "Ignore";

        private void FlagProjectCoverageSettings(object obj)
        {
            TestProjectSettings.IsCoverageEnabled = !TestProjectSettings.IsCoverageEnabled;
            _coverageSettingsStore.Update(TestProjectSettings);
        }
    }
}