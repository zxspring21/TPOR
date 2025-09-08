using TPOR.Shared.Models;

namespace TPOR.Intranet.Worker.Services;

public interface IFileProcessingService
{
    Task<bool> ProcessFileAsync(FileProcessingMessage message);
}
