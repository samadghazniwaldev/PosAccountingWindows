using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using PosAccountingApp.Models;

namespace PosAccountingApp.Data;

public static class DatabaseInitializer
{
    [ThreadStatic]
    private static string? _testDbPath;

    private static string GetDbPath()
    {
        if (_testDbPath != null) return _testDbPath;
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PosAccountingApp", "pos_data.db");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return path;
    }

    public static AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={GetDbPath()}");
        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Create a fresh test database. Each thread gets its own isolated DB.
    /// </summary>
    public static AppDbContext CreateTestDbContext()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"pos_test_{Guid.NewGuid():N}.db");
        _testDbPath = dbPath;
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        var db = new AppDbContext(optionsBuilder.Options);
        db.Database.EnsureCreated();
        return db;
    }

    public static void CleanupTestDb()
    {
        if (_testDbPath != null)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (File.Exists(_testDbPath))
                    File.Delete(_testDbPath);
            }
            catch { }
            _testDbPath = null;
        }
    }

    public static void Initialize()
    {
        using var db = CreateDbContext();
        db.Database.EnsureCreated();

        const string adminName = "admin";
        const string renamedAdminName = "مدیر ارشد";
        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("admin123")))
            .ToLowerInvariant();

        using var transaction = db.Database.BeginTransaction();
        db.Database.ExecuteSqlInterpolated($"""
            UPDATE Users
            SET Name = {adminName}
            WHERE Name = {renamedAdminName}
                AND Role = {(int)UserRole.Admin}
                AND PinCodeHash = {passwordHash}
                AND NOT EXISTS (SELECT 1 FROM Users WHERE Name = {adminName});
            """);
        db.Database.ExecuteSqlInterpolated($"""
            UPDATE Users
            SET Role = {(int)UserRole.SuperAdmin}
            WHERE Name = {adminName}
                AND Role = {(int)UserRole.Admin}
                AND PinCodeHash = {passwordHash};
            """);
        db.Database.ExecuteSqlInterpolated($"""
            INSERT INTO Users (Name, PinCodeHash, Role, IsActive)
            SELECT {adminName}, {passwordHash}, {(int)UserRole.SuperAdmin}, 1
            WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Name = {adminName});
            """);
        transaction.Commit();
    }
}
