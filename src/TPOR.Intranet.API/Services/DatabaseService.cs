using TPOR.Shared.Services;
using TPOR.Shared.Data;
using TPOR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace TPOR.Intranet.API.Services;

public class DatabaseService : IDatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly TporDbContext _context;

    public DatabaseService(ILogger<DatabaseService> logger, TporDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<bool> EnsureCustomerExistsAsync(string customerCode, string customerName)
    {
        try
        {
            var existingCustomer = await _context.RefCustomers
                .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);

            if (existingCustomer == null)
            {
                var newCustomer = new RefCustomer
                {
                    CustomerCode = customerCode,
                    Name = customerName,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                };

                _context.RefCustomers.Add(newCustomer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new customer: {CustomerCode}", customerCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring customer exists: {CustomerCode}", customerCode);
            return false;
        }
    }

    public async Task<bool> EnsureTesterExistsAsync(string testerCode, string testerName)
    {
        try
        {
            var existingTester = await _context.RefTesters
                .FirstOrDefaultAsync(t => t.TesterCode == testerCode);

            if (existingTester == null)
            {
                var newTester = new RefTester
                {
                    TesterCode = testerCode,
                    Name = testerName,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                };

                _context.RefTesters.Add(newTester);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new tester: {TesterCode}", testerCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring tester exists: {TesterCode}", testerCode);
            return false;
        }
    }

    public async Task<bool> EnsureTestProgramExistsAsync(string testProgramCode, string testProgramName)
    {
        try
        {
            var existingTestProgram = await _context.RefTestPrograms
                .FirstOrDefaultAsync(tp => tp.TestProgramCode == testProgramCode);

            if (existingTestProgram == null)
            {
                var newTestProgram = new RefTestProgram
                {
                    TestProgramCode = testProgramCode,
                    Name = testProgramName,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                };

                _context.RefTestPrograms.Add(newTestProgram);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new test program: {TestProgramCode}", testProgramCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring test program exists: {TestProgramCode}", testProgramCode);
            return false;
        }
    }

    public async Task<bool> EnsureFamilyExistsAsync(string familyCode, string familyName)
    {
        try
        {
            var existingFamily = await _context.RefFamilies
                .FirstOrDefaultAsync(f => f.FamilyCode == familyCode);

            if (existingFamily == null)
            {
                var newFamily = new RefFamily
                {
                    FamilyCode = familyCode,
                    Name = familyName,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                };

                _context.RefFamilies.Add(newFamily);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new family: {FamilyCode}", familyCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring family exists: {FamilyCode}", familyCode);
            return false;
        }
    }

    public async Task<bool> EnsureWaferExistsAsync(string waferCode, string waferName)
    {
        try
        {
            var existingWafer = await _context.RefWafers
                .FirstOrDefaultAsync(w => w.WaferCode == waferCode);

            if (existingWafer == null)
            {
                var newWafer = new RefWafer
                {
                    WaferCode = waferCode,
                    Name = waferName,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                };

                _context.RefWafers.Add(newWafer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new wafer: {WaferCode}", waferCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring wafer exists: {WaferCode}", waferCode);
            return false;
        }
    }

    public async Task<bool> EnsureLotExistsAsync(string lotCode, string lotName)
    {
        try
        {
            var existingLot = await _context.RefLots
                .FirstOrDefaultAsync(l => l.LotCode == lotCode);

            if (existingLot == null)
            {
                var newLot = new RefLot
                {
                    LotCode = lotCode,
                    Name = lotName,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                };

                _context.RefLots.Add(newLot);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new lot: {LotCode}", lotCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring lot exists: {LotCode}", lotCode);
            return false;
        }
    }

    public async Task<bool> LogBucketObjectAsync(string objectName, string bucketName, string status, long? fileSize = null, string? errorMessage = null)
    {
        try
        {
            var logEntry = new BucketObjectLog
            {
                ObjectName = objectName,
                BucketName = bucketName,
                Status = status,
                FileSize = fileSize,
                ErrorMessage = errorMessage,
                Name = $"Log_{objectName}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedBy = "System",
                ModifiedBy = "System"
            };

            _context.BucketObjectLogs.Add(logEntry);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Logged bucket object: {ObjectName} with status: {Status}", objectName, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging bucket object: {ObjectName}", objectName);
            return false;
        }
    }

    public async Task<bool> SaveLotAttributeAsync(string lotCode, string attributeName, string attributeValue, string? dataType = null)
    {
        try
        {
            var lotAttribute = new DataLotAttribute
            {
                LotCode = lotCode,
                AttributeName = attributeName,
                AttributeValue = attributeValue,
                DataType = dataType,
                Name = $"Attribute_{lotCode}_{attributeName}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedBy = "System",
                ModifiedBy = "System"
            };

            _context.DataLotAttributes.Add(lotAttribute);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Saved lot attribute: {LotCode}.{AttributeName}", lotCode, attributeName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving lot attribute: {LotCode}.{AttributeName}", lotCode, attributeName);
            return false;
        }
    }

    public async Task<bool> SaveRefreshTokenAsync(string token, string userId, DateTime expiresAt)
    {
        try
        {
            var refreshToken = new RefRefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = expiresAt,
                Name = $"RefreshToken_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedBy = userId,
                ModifiedBy = userId
            };

            _context.RefRefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Saved refresh token for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving refresh token for user: {UserId}", userId);
            return false;
        }
    }
}
