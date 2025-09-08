using TPOR.Shared.Services;
using TPOR.Shared.Models;
using System.Text.RegularExpressions;

namespace TPOR.Intranet.API.Services;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly string _basePath;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
        _basePath = Environment.GetEnvironmentVariable("LOCAL_STORAGE_PATH") ?? "uploads";
        
        // Ensure directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string? folderPath = null)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, folderPath ?? "", fileName);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStreamOutput = new FileStream(fullPath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOutput);

            _logger.LogInformation("File saved successfully: {FilePath}", fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            throw;
        }
    }

    public Task<Stream> GetFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file: {FilePath}", filePath);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> RenameFileAsync(string oldPath, string newPath)
    {
        try
        {
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
                _logger.LogInformation("File renamed successfully from {OldPath} to {NewPath}", oldPath, newPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming file from {OldPath} to {NewPath}", oldPath, newPath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<FileUploadInfo> ParseFileNameAsync(string fileName)
    {
        try
        {
            // Remove .zip extension
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
            // Expected format: {customerCode}_{projectCode}_{tester}_{lot}_{wafer}_{testprogram}_timestamp
            var parts = nameWithoutExtension.Split('_');
            
            if (parts.Length < 7)
            {
                throw new ArgumentException("Invalid file name format. Expected: {customerCode}_{projectCode}_{tester}_{lot}_{wafer}_{testprogram}_timestamp.zip");
            }

            var timestamp = parts[^1]; // Last part is timestamp
            var testProgram = parts[^2]; // Second to last is test program
            var wafer = parts[^3];
            var lot = parts[^4];
            var tester = parts[^5];
            var projectCode = parts[^6];
            var customerCode = string.Join("_", parts.Take(parts.Length - 6)); // Everything before the last 6 parts

            var result = new FileUploadInfo
            {
                CustomerCode = customerCode,
                ProjectCode = projectCode,
                Tester = tester,
                Lot = lot,
                Wafer = wafer,
                TestProgram = testProgram,
                Timestamp = timestamp,
                OriginalFileName = fileName
            };
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing file name: {FileName}", fileName);
            throw;
        }
    }
}
