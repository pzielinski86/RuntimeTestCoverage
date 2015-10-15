using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace TestCoverage.Storage
{
    public class XmlCoverageSettingsStore : ICoverageSettingsStore
    {
        private readonly string _filePath;

        public XmlCoverageSettingsStore(string solutionPath)
        {
            string solutionDir = Path.GetDirectoryName(solutionPath);
            string solutionName = Path.GetFileNameWithoutExtension(solutionPath);

            _filePath = Path.Combine(solutionDir, $"{solutionName}_CoverageSettings.xml");
        }
        public CoverageSettings Read()
        {
            if (!File.Exists(_filePath))
                return new CoverageSettings();

            using (var fileStream = File.Open(_filePath, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CoverageSettings));
                var settings = (CoverageSettings)xmlSerializer.Deserialize(fileStream);

                return settings;
            }
        }

        public string[] GetIgnoredTestProjects()
        {
            var settings = Read();

            return settings.Projects.Where(x => !x.IsCoverageEnabled).Select(x => x.Name).ToArray();
        }

        public void Update(CoverageSettings coverageSettings)
        {
            using (var fileStream = File.Open(_filePath, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CoverageSettings));
                xmlSerializer.Serialize(fileStream, coverageSettings);
            }
        }

        public void Update(TestProjectSettings testProjectSettings)
        {
            CoverageSettings settings = Read();

            var currentProjectSettings = settings.Projects.FirstOrDefault(x => x.Name == testProjectSettings.Name);
            settings.Projects.Remove(currentProjectSettings);
            settings.Projects.Add(testProjectSettings);

            Update(settings);
        }
    }
}