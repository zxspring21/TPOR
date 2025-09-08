using TPOR.Shared.Services;
using TPOR.Shared.Models;

namespace TPOR.Intranet.Worker.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IFileService _fileService;
    private readonly IDatabaseService _databaseService;

    public FileProcessingService(
        ILogger<FileProcessingService> logger,
        IFileService fileService,
        IDatabaseService databaseService)
    {
        _logger = logger;
        _fileService = fileService;
        _databaseService = databaseService;
    }

    public async Task<bool> ProcessFileAsync(FileProcessingMessage message)
    {
        try
        {
            _logger.LogInformation("Starting to process file: {FileName}", message.FileName);

            var fileInfo = message.FileInfo;

            // Ensure all required entities exist in database
            var customerSuccess = await _databaseService.EnsureCustomerExistsAsync(
                fileInfo.CustomerCode, 
                $"Customer_{fileInfo.CustomerCode}");

            var testerSuccess = await _databaseService.EnsureTesterExistsAsync(
                fileInfo.Tester, 
                $"Tester_{fileInfo.Tester}");

            var testProgramSuccess = await _databaseService.EnsureTestProgramExistsAsync(
                fileInfo.TestProgram, 
                $"TestProgram_{fileInfo.TestProgram}");

            var familySuccess = await _databaseService.EnsureFamilyExistsAsync(
                fileInfo.ProjectCode, 
                $"Family_{fileInfo.ProjectCode}");

            var waferSuccess = await _databaseService.EnsureWaferExistsAsync(
                fileInfo.Wafer, 
                $"Wafer_{fileInfo.Wafer}");

            var lotSuccess = await _databaseService.EnsureLotExistsAsync(
                fileInfo.Lot, 
                $"Lot_{fileInfo.Lot}");

            // Log bucket object processing
            var logSuccess = await _databaseService.LogBucketObjectAsync(
                message.FileName,
                "local-storage", // or bucket name in production
                "Processing",
                fileInfo.FileSize);

            // Save lot attributes
            var attributeSuccess = await _databaseService.SaveLotAttributeAsync(
                fileInfo.Lot,
                "Timestamp",
                fileInfo.Timestamp,
                "DateTime");

            var attributeSuccess2 = await _databaseService.SaveLotAttributeAsync(
                fileInfo.Lot,
                "CustomerCode",
                fileInfo.CustomerCode,
                "String");

            var attributeSuccess3 = await _databaseService.SaveLotAttributeAsync(
                fileInfo.Lot,
                "ProjectCode",
                fileInfo.ProjectCode,
                "String");

            // Check if all operations were successful
            var allSuccess = customerSuccess && testerSuccess && testProgramSuccess && 
                           familySuccess && waferSuccess && lotSuccess && 
                           logSuccess && attributeSuccess && attributeSuccess2 && attributeSuccess3;

            if (allSuccess)
            {
                // Rename file to add underscore prefix
                var processedFileName = $"_{message.FileName}";
                var newPath = Path.Combine(Path.GetDirectoryName(message.FilePath) ?? "", processedFileName);
                
                var renameSuccess = await _fileService.RenameFileAsync(message.FilePath, newPath);
                
                if (renameSuccess)
                {
                    _logger.LogInformation("Successfully processed and renamed file: {FileName} -> {ProcessedFileName}", 
                        message.FileName, processedFileName);
                    
                    // Update log status to completed
                    await _databaseService.LogBucketObjectAsync(
                        message.FileName,
                        "local-storage",
                        "Completed",
                        fileInfo.FileSize);
                }
                else
                {
                    _logger.LogError("Failed to rename processed file: {FileName}", message.FileName);
                    return false;
                }
            }
            else
            {
                _logger.LogError("One or more database operations failed for file: {FileName}", message.FileName);
                
                // Log error status
                await _databaseService.LogBucketObjectAsync(
                    message.FileName,
                    "local-storage",
                    "Failed",
                    fileInfo.FileSize,
                    "One or more database operations failed");
                
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {FileName}", message.FileName);
            
            // Log error status
            await _databaseService.LogBucketObjectAsync(
                message.FileName,
                "local-storage",
                "Error",
                message.FileInfo.FileSize,
                ex.Message);
            
            return false;
        }
    }
}
