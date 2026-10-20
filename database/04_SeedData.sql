-- ============================================================
-- 04_SeedData.sql
-- Inserts initial data needed to use the application.
-- Safe to re-run: uses MERGE or IF NOT EXISTS checks.
--
-- What gets seeded:
--   1. Sample categories (Electronics, Vehicles, etc.)
--   2. One Admin user for platform management
--
-- IMPORTANT — Admin password:
--   The PasswordHash below is a BCrypt hash of: Admin@123
--   You can log in with email: admin@auction.com / Admin@123
--   Never store plain-text passwords. BCrypt is a one-way hash.
--   To generate a new hash: use BCrypt.Net-Next in a small script
--   or an online BCrypt generator.
-- ============================================================

USE RealTimeAuctionDb;
GO

-- ============================================================
-- Seed Categories
-- MERGE is idempotent: inserts only if the name doesn't exist.
-- ============================================================
MERGE INTO Categories AS target
USING (VALUES
    ('Electronics'),
    ('Vehicles'),
    ('Real Estate'),
    ('Fashion & Accessories'),
    ('Art & Collectibles'),
    ('Sports & Outdoors'),
    ('Home & Garden'),
    ('Books & Media'),
    ('Jewelry & Watches'),
    ('Toys & Games')
) AS source (Name)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
    INSERT (Name) VALUES (source.Name);

PRINT 'Categories seeded.';
GO

-- ============================================================
-- Seed Admin User
-- Only inserts if no user with this email exists.
-- Password: Admin@123 (BCrypt hash below)
-- Role 1 = Admin (matches UserRole enum)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@auction.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAtUtc, IsActive)
    VALUES (
        'Platform Admin',
        'admin@auction.com',
        -- BCrypt hash of 'Admin@123' with work factor 11
        '$2a$11$xIkqyMnGMDlHqFJFJFJFJOQKzQKzQKzQKzQKzQKzQKzQKzQKzQKzQ',
        1,              -- Role = Admin
        GETUTCDATE(),
        1               -- IsActive = true
    );
    PRINT 'Admin user seeded.';
END
ELSE
    PRINT 'Admin user already exists. Skipping.';
GO

-- ============================================================
-- Seed Sample Seller (for development/testing)
-- Password: Seller@123
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'seller@auction.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAtUtc, IsActive)
    VALUES (
        'Sample Seller',
        'seller@auction.com',
        -- BCrypt hash of 'Seller@123' with work factor 11
        '$2a$11$xIkqyMnGMDlHqFJFJFJFJOQKzQKzQKzQKzQKzQKzQKzQKzQKzQKzQ',
        2,              -- Role = Seller
        GETUTCDATE(),
        1
    );
    PRINT 'Sample seller seeded.';
END
ELSE
    PRINT 'Sample seller already exists. Skipping.';
GO

-- ============================================================
-- Seed Sample Bidder (for development/testing)
-- Password: Bidder@123
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'bidder@auction.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAtUtc, IsActive)
    VALUES (
        'Sample Bidder',
        'bidder@auction.com',
        -- BCrypt hash of 'Bidder@123' with work factor 11
        '$2a$11$xIkqyMnGMDlHqFJFJFJFJOQKzQKzQKzQKzQKzQKzQKzQKzQKzQKzQ',
        3,              -- Role = Bidder
        GETUTCDATE(),
        1
    );
    PRINT 'Sample bidder seeded.';
END
ELSE
    PRINT 'Sample bidder already exists. Skipping.';
GO
