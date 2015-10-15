namespace TestCoverage.Storage
{
    public interface ICoverageSettingsStore
    {
        CoverageSettings Read();
        string[] GetIgnoredTestProjects();
        void Update(CoverageSettings coverageSettings);
        void Update(TestProjectSettings testProjectSettings);
    }
}