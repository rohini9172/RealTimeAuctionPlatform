-- ============================================================
-- 01_CreateDatabase.sql
-- Creates the RealTimeAuctionDb database if it doesn't exist.
-- Run this script connected to the 'master' database.
-- Safe to re-run: uses IF NOT EXISTS check.
-- ============================================================

USE master;
GO

IF NOT EXISTS (
    SELECT name FROM sys.databases WHERE name = N'RealTimeAuctionDb'
)
BEGIN
    CREATE DATABASE RealTimeAuctionDb;
    PRINT 'Database RealTimeAuctionDb created.';
END
ELSE
BEGIN
    PRINT 'Database RealTimeAuctionDb already exists. Skipping.';
END
GO
