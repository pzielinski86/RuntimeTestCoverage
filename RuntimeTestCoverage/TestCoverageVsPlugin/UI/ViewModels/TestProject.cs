using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using TestCoverage.Storage;
using TestCoverageVsPlugin.Annotations;

namespace TestCoverageVsPlugin.UI.ViewModels
{
    public sealed class TestProject:INotifyPropertyChanged
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

        public string FlagProjectCoverageSettingsCmdText => TestProjectSettings.IsCoverageEnabled ? "Ignore" : "Unignore";

        private void FlagProjectCoverageSettings(object obj)
        {
            TestProjectSettings.IsCoverageEnabled = !TestProjectSettings.IsCoverageEnabled;
            _coverageSettingsStore.Update(TestProjectSettings);

            OnPropertyChanged(nameof(FlagProjectCoverageSettingsCmdText));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}