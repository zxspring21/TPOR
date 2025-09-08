namespace TPOR.Shared.Models;

public class FileUploadInfo
{
    public string CustomerCode { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string Tester { get; set; } = string.Empty;
    public string Lot { get; set; } = string.Empty;
    public string Wafer { get; set; } = string.Empty;
    public string TestProgram { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ProcessedFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class FileProcessingMessage
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public FileUploadInfo FileInfo { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
