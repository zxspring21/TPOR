using TPOR.Shared.Services;
using TPOR.Shared.Models;
using Google.Cloud.Storage.V1;
using System.Text.RegularExpressions;

namespace TPOR.Intranet.Worker.Services;

public class WorkerFileService : IFileService
{
    private readonly ILogger<WorkerFileService> _logger;
    private readonly string _basePath;
    private readonly StorageClient? _storageClient;
    private readonly string _bucketName;
    private readonly bool _isProduction;

    public WorkerFileService(ILogger<WorkerFileService> logger)
    {
        _logger = logger;
        _basePath = Environment.GetEnvironmentVariable("LOCAL_STORAGE_PATH") ?? "uploads";
        _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME") ?? "tpor-intranet-storage";
        _isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        
        if (_isProduction)
        {
            try
            {
                _storageClient = StorageClient.Create();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Google Cloud Storage client");
                throw;
            }
        }
        
        // Ensure local directory exists for development
        if (!_isProduction && !Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string? folderPath = null)
    {
        try
        {
            if (_isProduction)
            {
                return await SaveToCloudStorageAsync(fileStream, fileName, folderPath);
            }
            else
            {
                return await SaveToLocalStorageAsync(fileStream, fileName, folderPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            throw;
        }
    }

    private async Task<string> SaveToCloudStorageAsync(Stream fileStream, string fileName, string? folderPath = null)
    {
        var objectName = string.IsNullOrEmpty(folderPath) ? fileName : $"{folderPath}/{fileName}";
        
        await _storageClient!.UploadObjectAsync(_bucketName, objectName, null, fileStream);
        
        _logger.LogInformation("File saved to Cloud Storage: {ObjectName}", objectName);
        return objectName;
    }

    private async Task<string> SaveToLocalStorageAsync(Stream fileStream, string fileName, string? folderPath = null)
    {
        var fullPath = Path.Combine(_basePath, folderPath ?? "", fileName);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStreamOutput = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput);

        _logger.LogInformation("File saved to local storage: {FilePath}", fullPath);
        return fullPath;
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        try
        {
            if (_isProduction)
            {
                return await GetFromCloudStorageAsync(filePath);
            }
            else
            {
                return await GetFromLocalStorageAsync(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file: {FilePath}", filePath);
            throw;
        }
    }

    private async Task<Stream> GetFromCloudStorageAsync(string objectName)
    {
        var stream = new MemoryStream();
        await _storageClient!.DownloadObjectAsync(_bucketName, objectName, stream);
        stream.Position = 0;
        return stream;
    }

    private Task<Stream> GetFromLocalStorageAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (_isProduction)
            {
                await _storageClient!.DeleteObjectAsync(_bucketName, filePath);
                _logger.LogInformation("File deleted from Cloud Storage: {FilePath}", filePath);
                return true;
            }
            else
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted from local storage: {FilePath}", filePath);
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> RenameFileAsync(string oldPath, string newPath)
    {
        try
        {
            if (_isProduction)
            {
                // Copy object with new name
                await _storageClient!.CopyObjectAsync(_bucketName, oldPath, _bucketName, newPath);
                await _storageClient.DeleteObjectAsync(_bucketName, oldPath);
                
                _logger.LogInformation("File renamed in Cloud Storage from {OldPath} to {NewPath}", oldPath, newPath);
                return true;
            }
            else
            {
                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, newPath);
                    _logger.LogInformation("File renamed in local storage from {OldPath} to {NewPath}", oldPath, newPath);
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming file from {OldPath} to {NewPath}", oldPath, newPath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            if (_isProduction)
            {
                try
                {
                    await _storageClient!.GetObjectAsync(_bucketName, filePath);
                    return true;
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
            }
            else
            {
                return File.Exists(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if file exists: {FilePath}", filePath);
            return false;
        }
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
