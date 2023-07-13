using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
