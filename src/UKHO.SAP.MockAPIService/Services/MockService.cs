using System.Diagnostics.CodeAnalysis;
using UKHO.SAP.MockAPIService.Enums;

namespace UKHO.SAP.MockAPIService.Services
{
    [ExcludeFromCodeCoverage]
    public class MockService
    {
        private const string CurrentTestFileName = "CurrentTestCase.txt";
        private readonly string _homeDirectoryPath;

        public MockService(IConfiguration configuration)
        {
            _homeDirectoryPath = Path.Combine(configuration["HOME"]);
        }

        public void CreateIfNotExists(string destPath)
        {
            if (!File.Exists(destPath))
            {
                File.Create(destPath).Close();
            }
        }

        public void UpdateTestCase(TestCase testCase)
        {
            string destPath = Path.Combine(_homeDirectoryPath, CurrentTestFileName);

            CreateIfNotExists(destPath);

            File.WriteAllText(destPath, testCase.ToString());
        }

        public string GetCurrentTestCase()
        {
            string destPath = Path.Combine(_homeDirectoryPath, CurrentTestFileName);

            CreateIfNotExists(destPath);

            return File.ReadAllText(destPath);
        }

        public void CleanUp()
        {
            string destPath = Path.Combine(_homeDirectoryPath, CurrentTestFileName);
            if (Directory.Exists(Path.GetDirectoryName(destPath)))
            {
                File.Delete(destPath);
            }
        }
    }
}