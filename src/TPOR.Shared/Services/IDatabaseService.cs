using TPOR.Shared.Models;

namespace TPOR.Shared.Services;

public interface IDatabaseService
{
    Task<bool> EnsureCustomerExistsAsync(string customerCode, string customerName);
    Task<bool> EnsureTesterExistsAsync(string testerCode, string testerName);
    Task<bool> EnsureTestProgramExistsAsync(string testProgramCode, string testProgramName);
    Task<bool> EnsureFamilyExistsAsync(string familyCode, string familyName);
    Task<bool> EnsureWaferExistsAsync(string waferCode, string waferName);
    Task<bool> EnsureLotExistsAsync(string lotCode, string lotName);
    Task<bool> LogBucketObjectAsync(string objectName, string bucketName, string status, long? fileSize = null, string? errorMessage = null);
    Task<bool> SaveLotAttributeAsync(string lotCode, string attributeName, string attributeValue, string? dataType = null);
    Task<bool> SaveRefreshTokenAsync(string token, string userId, DateTime expiresAt);
}
