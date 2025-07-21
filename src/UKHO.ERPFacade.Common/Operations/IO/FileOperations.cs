using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace UKHO.ERPFacade.Common.Operations.IO
{
    [ExcludeFromCodeCoverage]
    public class FileOperations : IFileOperations
    {
        private readonly IFileSystem _fileSystem;

        public FileOperations(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public bool IsFileExists(string filePath)
        {
            return _fileSystem.File.Exists(filePath);
        }
    }
}
