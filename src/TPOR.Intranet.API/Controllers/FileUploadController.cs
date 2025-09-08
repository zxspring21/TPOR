using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TPOR.Shared.Services;
using TPOR.Shared.Models;

namespace TPOR.Intranet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IFileService _fileService;
    // private readonly IMessageQueueService _messageQueueService;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IFileService fileService,
        // IMessageQueueService messageQueueService,
        ILogger<FileUploadController> logger)
    {
        _fileService = fileService;
        // _messageQueueService = messageQueueService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(100_000_000)] // 100MB limit
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Validate file extension
            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only ZIP files are allowed.");
            }

            // Parse file name to extract information
            var fileInfo = await _fileService.ParseFileNameAsync(file.FileName);
            if (string.IsNullOrEmpty(fileInfo.CustomerCode))
            {
                return BadRequest("Invalid file name format. Expected: {customerCode}_{projectCode}_{tester}_{lot}_{wafer}_{testprogram}_timestamp.zip");
            }

            // Save file
            var savedFilePath = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName);
            fileInfo.OriginalFileName = file.FileName;
            fileInfo.ProcessedFileName = savedFilePath;
            fileInfo.FileSize = file.Length;

            // Publish message to queue
            var message = new FileProcessingMessage
            {
                FileName = file.FileName,
                FilePath = savedFilePath,
                FileInfo = fileInfo
            };

            // await _messageQueueService.PublishMessageAsync(message);

            _logger.LogInformation("File uploaded successfully: {FileName}", file.FileName);

            return Ok(new
            {
                message = "File uploaded successfully",
                fileName = file.FileName,
                fileInfo = new
                {
                    fileInfo.CustomerCode,
                    fileInfo.ProjectCode,
                    fileInfo.Tester,
                    fileInfo.Lot,
                    fileInfo.Wafer,
                    fileInfo.TestProgram,
                    fileInfo.Timestamp
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            return StatusCode(500, "Internal server error occurred while uploading file.");
        }
    }

    [HttpGet("status/{fileName}")]
    [Authorize]
    public async Task<IActionResult> GetFileStatus(string fileName)
    {
        try
        {
            var filePath = Path.Combine(
                Environment.GetEnvironmentVariable("LOCAL_STORAGE_PATH") ?? "uploads",
                fileName
            );

            var exists = await _fileService.FileExistsAsync(filePath);
            if (!exists)
            {
                return NotFound("File not found.");
            }

            return Ok(new
            {
                fileName,
                exists,
                message = "File found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file status: {FileName}", fileName);
            return StatusCode(500, "Internal server error occurred while checking file status.");
        }
    }
}
