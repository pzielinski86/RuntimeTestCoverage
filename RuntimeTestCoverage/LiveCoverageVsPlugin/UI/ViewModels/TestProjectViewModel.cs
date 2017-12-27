using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LiveCoverageVsPlugin.Annotations;
using TestCoverage.Storage;

namespace LiveCoverageVsPlugin.UI.ViewModels
{
    public sealed class TestProjectViewModel:INotifyPropertyChanged
    {
        private readonly ICoverageSettingsStore _coverageSettingsStore;

        public TestProjectViewModel(ICoverageSettingsStore coverageSettingsStore)
        {
            _coverageSettingsStore = coverageSettingsStore;
            FlagProjectCoverageSettingsCmd = new DelegateCommand(FlagProjectCoverageSettings);
        }

        public TestProjectSettings TestProjectSettings { get; set; }

        public TestFixtureViewModel[] TestFixtures { get; set; }

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