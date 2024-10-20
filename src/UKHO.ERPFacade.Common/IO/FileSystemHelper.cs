using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class FileSystemHelper : IFileSystemHelper
    {
        private readonly IFileSystem _fileSystem;

        public FileSystemHelper(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public bool IsFileExists(string filePath)
        {
            return _fileSystem.File.Exists(filePath);
        }
    }
}
