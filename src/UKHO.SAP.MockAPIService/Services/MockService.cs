using System.Diagnostics.CodeAnalysis;
using UKHO.SAP.MockAPIService.Enums;

namespace UKHO.SAP.MockAPIService.Services
{
    [ExcludeFromCodeCoverage]
    public class MockService
    {
        private const string CURRENTTESTFILENAME = "CurrentTestCase.txt";
        private readonly string _homeDirectoryPath;

        public MockService()
        {
            _homeDirectoryPath = Environment.CurrentDirectory;
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
            string destPath = Path.Combine(_homeDirectoryPath, CURRENTTESTFILENAME);

            CreateIfNotExists(destPath);

            File.WriteAllText(destPath, testCase.ToString());
        }

        public string GetCurrentTestCase()
        {
            string destPath = Path.Combine(_homeDirectoryPath, CURRENTTESTFILENAME);

            CreateIfNotExists(destPath);

            return File.ReadAllText(destPath);
        }

        public void CleanUp()
        {
            string destPath = Path.Combine(_homeDirectoryPath, CURRENTTESTFILENAME);
            if (Directory.Exists(Path.GetDirectoryName(destPath)))
            {
                File.Delete(destPath);
            }
        }
    }
}