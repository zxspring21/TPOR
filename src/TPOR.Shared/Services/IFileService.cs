using TPOR.Shared.Models;

namespace TPOR.Shared.Services;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string? folderPath = null);
    Task<Stream> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> RenameFileAsync(string oldPath, string newPath);
    Task<bool> FileExistsAsync(string filePath);
    Task<FileUploadInfo> ParseFileNameAsync(string fileName);
}
