namespace TestCoverage.Storage
{
    public interface ICoverageSettingsStore
    {
        CoverageSettings Read();
        void Update(CoverageSettings coverageSettings);
        void Update(TestProjectSettings testProjectSettings);
    }
}