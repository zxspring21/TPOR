using Microsoft.EntityFrameworkCore;
using TPOR.Shared.Models;

namespace TPOR.Shared.Data;

public class TporDbContext : DbContext
{
    public TporDbContext(DbContextOptions<TporDbContext> options) : base(options)
    {
    }

    public DbSet<RefCustomer> RefCustomers { get; set; }
    public DbSet<RefTester> RefTesters { get; set; }
    public DbSet<RefTestProgram> RefTestPrograms { get; set; }
    public DbSet<RefFamily> RefFamilies { get; set; }
    public DbSet<RefWafer> RefWafers { get; set; }
    public DbSet<RefLot> RefLots { get; set; }
    public DbSet<RefRefreshToken> RefRefreshTokens { get; set; }
    public DbSet<BucketObjectLog> BucketObjectLogs { get; set; }
    public DbSet<DataLotAttribute> DataLotAttributes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes for better performance
        modelBuilder.Entity<RefCustomer>()
            .HasIndex(e => e.CustomerCode)
            .IsUnique();

        modelBuilder.Entity<RefTester>()
            .HasIndex(e => e.TesterCode)
            .IsUnique();

        modelBuilder.Entity<RefTestProgram>()
            .HasIndex(e => e.TestProgramCode)
            .IsUnique();

        modelBuilder.Entity<RefFamily>()
            .HasIndex(e => e.FamilyCode)
            .IsUnique();

        modelBuilder.Entity<RefWafer>()
            .HasIndex(e => e.WaferCode)
            .IsUnique();

        modelBuilder.Entity<RefLot>()
            .HasIndex(e => e.LotCode)
            .IsUnique();

        modelBuilder.Entity<RefRefreshToken>()
            .HasIndex(e => e.Token)
            .IsUnique();

        modelBuilder.Entity<BucketObjectLog>()
            .HasIndex(e => e.ObjectName);

        modelBuilder.Entity<DataLotAttribute>()
            .HasIndex(e => new { e.LotCode, e.AttributeName });
    }
}
