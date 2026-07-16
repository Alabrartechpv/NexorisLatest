USE [RambaiTest]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[_POS_ItemMasterStatusRules]', N'P') IS NOT NULL
    DROP PROC [dbo].[_POS_ItemMasterStatusRules]
GO

CREATE PROC [dbo].[_POS_ItemMasterStatusRules]
(
    @CompanyId int = null,
    @BranchId int = null,
    @ItemId int = null,
    @StatusName nvarchar(50) = null,
    @StatusReason nvarchar(500) = null,
    @StatusDate datetime = null,
    @BlockSale bit = 0,
    @BlockPurchase bit = 0,
    @_Operation varchar(50) = null
)
AS
BEGIN
    SET NOCOUNT ON;

    IF(@_Operation = 'ENSURESTATUSSTORAGE')
    BEGIN
        IF OBJECT_ID(N'dbo.POS_ItemMasterStatusRules', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.POS_ItemMasterStatusRules
            (
                Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                ItemId int NOT NULL,
                CompanyId int NULL,
                BranchId int NULL,
                StatusName nvarchar(50) NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_StatusName DEFAULT(N'Active'),
                StatusReason nvarchar(500) NULL,
                StatusDate datetime NULL,
                BlockSale bit NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_BlockSale DEFAULT(0),
                BlockPurchase bit NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_BlockPurchase DEFAULT(0),
                CreatedOn datetime NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_CreatedOn DEFAULT(GETDATE()),
                ModifiedOn datetime NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_ModifiedOn DEFAULT(GETDATE())
            )
        END

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'CompanyId') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD CompanyId int NULL;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'BranchId') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD BranchId int NULL;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'StatusName') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD StatusName nvarchar(50) NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_StatusName_Alt DEFAULT(N'Active') WITH VALUES;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'StatusReason') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD StatusReason nvarchar(500) NULL;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'StatusDate') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD StatusDate datetime NULL;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'BlockSale') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD BlockSale bit NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_BlockSale_Alt DEFAULT(0) WITH VALUES;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'BlockPurchase') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD BlockPurchase bit NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_BlockPurchase_Alt DEFAULT(0) WITH VALUES;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'CreatedOn') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD CreatedOn datetime NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_CreatedOn_Alt DEFAULT(GETDATE()) WITH VALUES;

        IF COL_LENGTH(N'dbo.POS_ItemMasterStatusRules', N'ModifiedOn') IS NULL
            ALTER TABLE dbo.POS_ItemMasterStatusRules ADD ModifiedOn datetime NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_ModifiedOn_Alt DEFAULT(GETDATE()) WITH VALUES;

        SELECT 'SUCCESS' AS Result;
    END
    ELSE IF(@_Operation = 'GETSTATUS')
    BEGIN
        IF OBJECT_ID(N'dbo.POS_ItemMasterStatusRules', N'U') IS NULL
        BEGIN
            SELECT
                @ItemId AS ItemId,
                N'Active' AS StatusName,
                CAST(NULL AS nvarchar(500)) AS StatusReason,
                CAST(GETDATE() AS datetime) AS StatusDate,
                CAST(0 AS bit) AS BlockSale,
                CAST(0 AS bit) AS BlockPurchase;
        END
        ELSE IF EXISTS (SELECT 1 FROM dbo.POS_ItemMasterStatusRules WHERE ItemId = @ItemId)
        BEGIN
            SELECT TOP 1 ItemId, StatusName, StatusReason, StatusDate, BlockSale, BlockPurchase
            FROM dbo.POS_ItemMasterStatusRules
            WHERE ItemId = @ItemId;
        END
        ELSE
        BEGIN
            SELECT
                @ItemId AS ItemId,
                N'Active' AS StatusName,
                CAST(NULL AS nvarchar(500)) AS StatusReason,
                CAST(GETDATE() AS datetime) AS StatusDate,
                CAST(0 AS bit) AS BlockSale,
                CAST(0 AS bit) AS BlockPurchase;
        END
    END
    ELSE IF(@_Operation = 'SAVESTATUS')
    BEGIN
        EXEC dbo._POS_ItemMasterStatusRules @_Operation = 'ENSURESTATUSSTORAGE';

        IF EXISTS (SELECT 1 FROM dbo.POS_ItemMasterStatusRules WHERE ItemId = @ItemId)
        BEGIN
            UPDATE dbo.POS_ItemMasterStatusRules
            SET CompanyId = @CompanyId,
                BranchId = @BranchId,
                StatusName = ISNULL(@StatusName, N'Active'),
                StatusReason = @StatusReason,
                StatusDate = ISNULL(@StatusDate, GETDATE()),
                BlockSale = ISNULL(@BlockSale, 0),
                BlockPurchase = ISNULL(@BlockPurchase, 0),
                ModifiedOn = GETDATE()
            WHERE ItemId = @ItemId;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.POS_ItemMasterStatusRules
            (
                ItemId, CompanyId, BranchId, StatusName, StatusReason, StatusDate,
                BlockSale, BlockPurchase, CreatedOn, ModifiedOn
            )
            VALUES
            (
                @ItemId, @CompanyId, @BranchId, ISNULL(@StatusName, N'Active'), @StatusReason,
                ISNULL(@StatusDate, GETDATE()), ISNULL(@BlockSale, 0), ISNULL(@BlockPurchase, 0),
                GETDATE(), GETDATE()
            );
        END

        SELECT 'SUCCESS' AS Result;
    END
END
GO
