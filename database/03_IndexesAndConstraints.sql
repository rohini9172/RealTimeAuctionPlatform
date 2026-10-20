-- ============================================================
-- 03_IndexesAndConstraints.sql
-- Adds indexes to speed up the most common queries.
-- Safe to re-run: each index is only created if it doesn't exist.
--
-- WHY INDEXES MATTER:
-- Without indexes, SQL Server scans every row in a table.
-- With indexes, it jumps directly to matching rows.
-- We index columns that appear in WHERE, JOIN, and ORDER BY clauses.
-- ============================================================

USE RealTimeAuctionDb;
GO

-- ============================================================
-- Auctions indexes
-- ============================================================

-- Filter auctions by status (e.g. "show all Live auctions")
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auctions_Status' AND object_id = OBJECT_ID('Auctions'))
BEGIN
    CREATE INDEX IX_Auctions_Status ON Auctions (Status);
    PRINT 'Index IX_Auctions_Status created.';
END

-- Filter auctions by end time (Hangfire job queries "EndTime <= NOW AND Status = Live")
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auctions_EndTime' AND object_id = OBJECT_ID('Auctions'))
BEGIN
    CREATE INDEX IX_Auctions_EndTime ON Auctions (EndTime);
    PRINT 'Index IX_Auctions_EndTime created.';
END

-- Filter auctions by start time (Hangfire job queries "StartTime <= NOW AND Status = Scheduled")
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auctions_StartTime' AND object_id = OBJECT_ID('Auctions'))
BEGIN
    CREATE INDEX IX_Auctions_StartTime ON Auctions (StartTime);
    PRINT 'Index IX_Auctions_StartTime created.';
END

-- Filter auctions by category (e.g. "show all Electronics auctions")
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auctions_CategoryId' AND object_id = OBJECT_ID('Auctions'))
BEGIN
    CREATE INDEX IX_Auctions_CategoryId ON Auctions (CategoryId);
    PRINT 'Index IX_Auctions_CategoryId created.';
END

-- Filter auctions by seller (e.g. "show my auctions" page)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auctions_SellerId' AND object_id = OBJECT_ID('Auctions'))
BEGIN
    CREATE INDEX IX_Auctions_SellerId ON Auctions (SellerId);
    PRINT 'Index IX_Auctions_SellerId created.';
END

-- Composite index: status + end time together — used by the Hangfire lifecycle job
-- which queries: WHERE Status = 2 AND EndTime <= GETUTCDATE()
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auctions_Status_EndTime' AND object_id = OBJECT_ID('Auctions'))
BEGIN
    CREATE INDEX IX_Auctions_Status_EndTime ON Auctions (Status, EndTime);
    PRINT 'Index IX_Auctions_Status_EndTime created.';
END

-- ============================================================
-- Bids indexes
-- ============================================================

-- Look up all bids for a specific auction (bid history page)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bids_AuctionId' AND object_id = OBJECT_ID('Bids'))
BEGIN
    CREATE INDEX IX_Bids_AuctionId ON Bids (AuctionId);
    PRINT 'Index IX_Bids_AuctionId created.';
END

-- Look up all bids placed by a specific user ("my bids" page)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bids_BidderId' AND object_id = OBJECT_ID('Bids'))
BEGIN
    CREATE INDEX IX_Bids_BidderId ON Bids (BidderId);
    PRINT 'Index IX_Bids_BidderId created.';
END

-- Composite: find the highest bid for an auction quickly
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bids_AuctionId_Amount' AND object_id = OBJECT_ID('Bids'))
BEGIN
    CREATE INDEX IX_Bids_AuctionId_Amount ON Bids (AuctionId, Amount DESC);
    PRINT 'Index IX_Bids_AuctionId_Amount created.';
END

-- ============================================================
-- Notifications indexes
-- ============================================================

-- Look up all notifications for a user (notifications page)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId' AND object_id = OBJECT_ID('Notifications'))
BEGIN
    CREATE INDEX IX_Notifications_UserId ON Notifications (UserId);
    PRINT 'Index IX_Notifications_UserId created.';
END

-- Filter unread notifications for a user
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId_IsRead' AND object_id = OBJECT_ID('Notifications'))
BEGIN
    CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications (UserId, IsRead);
    PRINT 'Index IX_Notifications_UserId_IsRead created.';
END

-- ============================================================
-- WatchLists indexes
-- ============================================================

-- Look up all watchlist entries for a user
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WatchLists_UserId' AND object_id = OBJECT_ID('WatchLists'))
BEGIN
    CREATE INDEX IX_WatchLists_UserId ON WatchLists (UserId);
    PRINT 'Index IX_WatchLists_UserId created.';
END

-- ============================================================
-- AutoBids indexes
-- ============================================================

-- Find active auto-bids for an auction (used during bid processing)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AutoBids_AuctionId_IsActive' AND object_id = OBJECT_ID('AutoBids'))
BEGIN
    CREATE INDEX IX_AutoBids_AuctionId_IsActive ON AutoBids (AuctionId, IsActive);
    PRINT 'Index IX_AutoBids_AuctionId_IsActive created.';
END
GO
