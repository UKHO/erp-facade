using System.IO;

namespace UKHO.ERPFacade.API.UnitTests.Common
{
    public static class TestHelper
    {
        public static string ReadFileData(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }
}
