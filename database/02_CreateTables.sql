-- ============================================================
-- 02_CreateTables.sql
-- Creates all application tables with PKs, FKs, and constraints.
-- Run this script connected to RealTimeAuctionDb.
-- Safe to re-run: each table is only created if it doesn't exist.
--
-- IMPORTANT FK DELETE RULE:
-- We use ON DELETE NO ACTION everywhere to avoid SQL Server's
-- "multiple cascade paths" error. For example, the Auctions table
-- has three foreign keys pointing to Users (SellerId, HighestBidderId,
-- WinnerId). If we used CASCADE on any of them, SQL Server would
-- complain about ambiguous delete paths.
-- ============================================================

USE RealTimeAuctionDb;
GO

-- ============================================================
-- Users
-- Stores all platform users regardless of role.
-- PasswordHash stores a BCrypt hash — never plain text.
-- Role is stored as an integer matching the UserRole enum:
--   1 = Admin, 2 = Seller, 3 = Bidder
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id            INT             NOT NULL IDENTITY(1,1),
        FullName      NVARCHAR(150)   NOT NULL,
        Email         NVARCHAR(256)   NOT NULL,
        PasswordHash  NVARCHAR(512)   NOT NULL,
        Role          INT             NOT NULL DEFAULT 3,   -- Default: Bidder
        CreatedAtUtc  DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        IsActive      BIT             NOT NULL DEFAULT 1,

        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
    PRINT 'Table Users created.';
END
ELSE
    PRINT 'Table Users already exists. Skipping.';
GO

-- ============================================================
-- Categories
-- Simple lookup table for auction categories (e.g. Electronics).
-- Only Admins can create categories (enforced in application layer).
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id    INT           NOT NULL IDENTITY(1,1),
        Name  NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Categories PRIMARY KEY (Id),
        CONSTRAINT UQ_Categories_Name UNIQUE (Name)
    );
    PRINT 'Table Categories created.';
END
ELSE
    PRINT 'Table Categories already exists. Skipping.';
GO

-- ============================================================
-- Auctions
-- Core table. An auction belongs to a Seller and a Category.
-- HighestBidderId and WinnerId are nullable because:
--   - HighestBidderId is null until the first bid is placed.
--   - WinnerId is null until the auction ends.
-- RowVersion is a SQL Server rowversion (timestamp) column used
-- for optimistic concurrency. EF Core reads this value and
-- includes it in UPDATE WHERE clauses. If another request
-- changed the row first, the update affects 0 rows and EF
-- throws a DbUpdateConcurrencyException.
-- Status matches AuctionStatus enum: 1=Scheduled, 2=Live,
--   3=Ended, 4=Cancelled
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Auctions')
BEGIN
    CREATE TABLE Auctions (
        Id                INT              NOT NULL IDENTITY(1,1),
        SellerId          INT              NOT NULL,
        CategoryId        INT              NOT NULL,
        Title             NVARCHAR(200)    NOT NULL,
        Description       NVARCHAR(MAX)    NOT NULL,
        StartingPrice     DECIMAL(18,2)    NOT NULL,
        MinIncrement      DECIMAL(18,2)    NOT NULL,
        CurrentPrice      DECIMAL(18,2)    NOT NULL,
        HighestBidderId   INT              NULL,
        StartTime         DATETIME2        NOT NULL,
        EndTime           DATETIME2        NOT NULL,
        Status            INT              NOT NULL DEFAULT 1,  -- Default: Scheduled
        WinnerId          INT              NULL,
        RowVersion        ROWVERSION       NOT NULL,            -- Optimistic concurrency token

        CONSTRAINT PK_Auctions PRIMARY KEY (Id),

        -- NO ACTION on all three User FKs to avoid multiple cascade paths
        CONSTRAINT FK_Auctions_Seller
            FOREIGN KEY (SellerId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_Auctions_HighestBidder
            FOREIGN KEY (HighestBidderId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_Auctions_Winner
            FOREIGN KEY (WinnerId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_Auctions_Category
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE NO ACTION,

        -- Business rule: end time must be after start time
        CONSTRAINT CK_Auctions_EndAfterStart
            CHECK (EndTime > StartTime),

        -- Business rule: prices must be positive
        CONSTRAINT CK_Auctions_StartingPrice
            CHECK (StartingPrice > 0),

        CONSTRAINT CK_Auctions_MinIncrement
            CHECK (MinIncrement > 0)
    );
    PRINT 'Table Auctions created.';
END
ELSE
    PRINT 'Table Auctions already exists. Skipping.';
GO

-- ============================================================
-- Bids
-- Each row is one bid placed by a Bidder on an Auction.
-- IsWinningBid is updated when the auction ends — only one bid
-- per auction will have IsWinningBid = 1.
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bids')
BEGIN
    CREATE TABLE Bids (
        Id           INT           NOT NULL IDENTITY(1,1),
        AuctionId    INT           NOT NULL,
        BidderId     INT           NOT NULL,
        Amount       DECIMAL(18,2) NOT NULL,
        BidTimeUtc   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        IsWinningBid BIT           NOT NULL DEFAULT 0,

        CONSTRAINT PK_Bids PRIMARY KEY (Id),

        CONSTRAINT FK_Bids_Auction
            FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_Bids_Bidder
            FOREIGN KEY (BidderId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT CK_Bids_Amount
            CHECK (Amount > 0)
    );
    PRINT 'Table Bids created.';
END
ELSE
    PRINT 'Table Bids already exists. Skipping.';
GO

-- ============================================================
-- AuctionImages
-- An auction can have multiple images stored as URLs.
-- Images are deleted when the auction is deleted (CASCADE).
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuctionImages')
BEGIN
    CREATE TABLE AuctionImages (
        Id         INT            NOT NULL IDENTITY(1,1),
        AuctionId  INT            NOT NULL,
        ImageUrl   NVARCHAR(1000) NOT NULL,

        CONSTRAINT PK_AuctionImages PRIMARY KEY (Id),

        CONSTRAINT FK_AuctionImages_Auction
            FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE CASCADE
    );
    PRINT 'Table AuctionImages created.';
END
ELSE
    PRINT 'Table AuctionImages already exists. Skipping.';
GO

-- ============================================================
-- AutoBids
-- Allows a bidder to set a maximum amount they are willing to
-- pay. The system automatically places bids on their behalf
-- up to this maximum when they are outbid.
-- IsActive = 0 when the max is reached or the auction ends.
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AutoBids')
BEGIN
    CREATE TABLE AutoBids (
        Id             INT           NOT NULL IDENTITY(1,1),
        AuctionId      INT           NOT NULL,
        BidderId       INT           NOT NULL,
        MaximumAmount  DECIMAL(18,2) NOT NULL,
        CreatedAtUtc   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        IsActive       BIT           NOT NULL DEFAULT 1,

        CONSTRAINT PK_AutoBids PRIMARY KEY (Id),

        CONSTRAINT FK_AutoBids_Auction
            FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_AutoBids_Bidder
            FOREIGN KEY (BidderId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT CK_AutoBids_MaximumAmount
            CHECK (MaximumAmount > 0)
    );
    PRINT 'Table AutoBids created.';
END
ELSE
    PRINT 'Table AutoBids already exists. Skipping.';
GO

-- ============================================================
-- WatchLists
-- A user can watch auctions to receive notifications.
-- The unique constraint on (UserId, AuctionId) prevents a user
-- from adding the same auction to their watchlist twice.
-- This is enforced both here in the DB and in the service layer.
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WatchLists')
BEGIN
    CREATE TABLE WatchLists (
        Id           INT       NOT NULL IDENTITY(1,1),
        UserId       INT       NOT NULL,
        AuctionId    INT       NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT PK_WatchLists PRIMARY KEY (Id),

        CONSTRAINT FK_WatchLists_User
            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_WatchLists_Auction
            FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE NO ACTION,

        -- Prevents duplicate watchlist entries at the database level
        CONSTRAINT UQ_WatchLists_UserAuction UNIQUE (UserId, AuctionId)
    );
    PRINT 'Table WatchLists created.';
END
ELSE
    PRINT 'Table WatchLists already exists. Skipping.';
GO

-- ============================================================
-- Notifications
-- In-app notifications sent to users for auction events.
-- AuctionId is nullable because some notifications may not
-- relate to a specific auction (e.g. account-level messages).
-- Type matches NotificationType enum:
--   1=AuctionStarted, 2=AuctionEnded, 3=OutBid,
--   4=AuctionWon, 5=AuctionCancelled
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        Id           INT            NOT NULL IDENTITY(1,1),
        UserId       INT            NOT NULL,
        AuctionId    INT            NULL,
        Message      NVARCHAR(500)  NOT NULL,
        Type         INT            NOT NULL,
        IsRead       BIT            NOT NULL DEFAULT 0,
        CreatedAtUtc DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT PK_Notifications PRIMARY KEY (Id),

        CONSTRAINT FK_Notifications_User
            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,

        CONSTRAINT FK_Notifications_Auction
            FOREIGN KEY (AuctionId) REFERENCES Auctions(Id) ON DELETE NO ACTION
    );
    PRINT 'Table Notifications created.';
END
ELSE
    PRINT 'Table Notifications already exists. Skipping.';
GO
