using System.Diagnostics.CodeAnalysis;
using UKHO.SAP.MockAPIService.Enums;

namespace UKHO.SAP.MockAPIService.Services
{
    [ExcludeFromCodeCoverage]
    public class MockService
    {
        private readonly string _homeDirectoryPath;

        public MockService(IConfiguration configuration)
        {
            _homeDirectoryPath = Path.Combine(Environment.CurrentDirectory, configuration["FolderName"]);
        }

        private readonly string currentTestFileName = "CurrentTestCase.txt";

        public void CreateIfNotExists(string destPath)
        {
            if (!File.Exists(destPath))
            {
                File.Create(destPath).Close();
            }
        }

        public void UpdateTestCase(TestCase testCase)
        {
            string destPath = Path.Combine(_homeDirectoryPath, currentTestFileName);

            CreateIfNotExists(destPath);

            File.WriteAllText(destPath, testCase.ToString());
        }

        public TestCase GetCurrentTestCase()
        {
            var testCase = new TestCase();
            string destPath = Path.Combine(_homeDirectoryPath, currentTestFileName);

            CreateIfNotExists(destPath);

            string readText = File.ReadAllText(destPath);

            if (!String.IsNullOrEmpty(readText))
            {
                Enum.TryParse(readText, true, out testCase);
            }

            return testCase;
        }

        public void CleanUp()
        {
            string destPath = Path.Combine(_homeDirectoryPath, currentTestFileName);
            if (Directory.Exists(Path.GetDirectoryName(destPath)))
            {
                File.Delete(destPath);
            }
        }
    }
}