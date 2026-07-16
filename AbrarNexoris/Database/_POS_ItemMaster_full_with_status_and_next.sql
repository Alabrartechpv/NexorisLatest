USE [RambaiTest]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[_POS_ItemMaster]
(
    @CompanyId int,
    @BranchId int,
    @FinYearId int,
    @ItemId int = null,
    @ItemNo varchar(50) = null,
    @Description varchar(50) = null,
    @Barcode varchar(50) = null,
    @AliasBarcode varchar(100) = null,
    @ItemTypeId int = null,
    @VendorId int = null,
    @BrandId int = null,
    @GroupId int = null,
    @CategoryId int = null,
    @BaseUnitId int = null,
    @ForCustomerType varchar(50) = null,
    @NameInLocalLanguage varchar(50) = null,
    @HSNCode varchar(50) = null,
    @Order_Cycle_Days int = 7,
    @Box_Quantity int = 1,
    @Is_Perishable bit = 0,
    @NavigationType varchar(20) = null,
    @CurrentItemNo int = null,
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

    IF(@_Operation = 'CREATE')
    BEGIN
        IF EXISTS (select * from ItemMaster where [Description] = @Description)
        BEGIN
            select 'Already Exist'
        END
        ELSE
        BEGIN
            set @ItemId = (SELECT ISNULL(MAX(ItemId) +1,1) from ItemMaster)
            INSERT INTO ItemMaster (CompanyId, BranchId, FinYearId, ItemId, ItemNo, [Description], BarCode, ItemTypeId,
                                    VendorId, BrandId, GroupId, CategoryId, BaseUnitId,
                                    ForCustomerType, NameInLocalLanguage, HSNCode, Active,
                                    Order_Cycle_Days, Box_Quantity, Is_Perishable)
            VALUES (@CompanyId, @BranchId, @FinYearId, @ItemId, CAST(@ItemId AS varchar(50)),
                    @Description, @BarCode, @ItemTypeId,
                    @VendorId, @BrandId, @GroupId, @CategoryId, @BaseUnitId,
                    @ForCustomerType, @NameInLocalLanguage, @HSNCode, 0,
                    @Order_Cycle_Days, @Box_Quantity, @Is_Perishable)
            SELECT @ItemId as ItemId
        END
    END
    ELSE IF(@_Operation = 'GETNEXTITEMNO')
    BEGIN
        SELECT ISNULL(MAX(ItemId) + 1, 1) AS ItemNo
        FROM ItemMaster
    END
    ELSE IF(@_Operation = 'NAVIGATE')
    BEGIN
        IF(@NavigationType = 'FIRST')
        BEGIN
            SELECT TOP 1 ItemId
            FROM ItemMaster
            WHERE Active = 0
            ORDER BY TRY_CONVERT(int, ItemNo), ItemId
        END
        ELSE IF(@NavigationType = 'LAST')
        BEGIN
            SELECT TOP 1 ItemId
            FROM ItemMaster
            WHERE Active = 0
            ORDER BY TRY_CONVERT(int, ItemNo) DESC, ItemId DESC
        END
        ELSE IF(@NavigationType = 'NEXT')
        BEGIN
            SELECT TOP 1 ItemId
            FROM ItemMaster
            WHERE Active = 0
              AND TRY_CONVERT(int, ItemNo) > ISNULL(@CurrentItemNo, 0)
            ORDER BY TRY_CONVERT(int, ItemNo), ItemId
        END
        ELSE IF(@NavigationType = 'PREVIOUS')
        BEGIN
            SELECT TOP 1 ItemId
            FROM ItemMaster
            WHERE Active = 0
              AND TRY_CONVERT(int, ItemNo) < ISNULL(@CurrentItemNo, 0)
            ORDER BY TRY_CONVERT(int, ItemNo) DESC, ItemId DESC
        END
    END
    ELSE IF(@_Operation = 'GETALL')
    BEGIN
        SELECT CompanyId, BranchId, FinYearId, ItemId,
               TRY_CONVERT(int, ItemNo) AS ItemNo,
               [Description], Barcode, ItemTypeId, VendorId, BrandId, GroupId,
               CategoryId, BaseUnitId, ForCustomerType, NameInLocalLanguage,
               HSNCode, Order_Cycle_Days, Box_Quantity, Is_Perishable
        FROM ItemMaster
        WHERE Active = 0
        ORDER BY TRY_CONVERT(int, ItemNo), ItemId
    END
    ELSE IF(@_Operation = 'ENSURESTATUSSTORAGE')
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

        SELECT 'SUCCESS' AS Result
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
                CAST(0 AS bit) AS BlockPurchase
        END
        ELSE
        BEGIN
            SELECT TOP 1 ItemId, StatusName, StatusReason, StatusDate, BlockSale, BlockPurchase
            FROM dbo.POS_ItemMasterStatusRules
            WHERE ItemId = @ItemId
            ORDER BY Id DESC
        END
    END
    ELSE IF(@_Operation = 'SAVESTATUS')
    BEGIN
        IF OBJECT_ID(N'dbo.POS_ItemMasterStatusRules', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.POS_ItemMasterStatusRules
            (
                Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                ItemId int NOT NULL,
                CompanyId int NULL,
                BranchId int NULL,
                StatusName nvarchar(50) NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_StatusName_Save DEFAULT(N'Active'),
                StatusReason nvarchar(500) NULL,
                StatusDate datetime NULL,
                BlockSale bit NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_BlockSale_Save DEFAULT(0),
                BlockPurchase bit NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_BlockPurchase_Save DEFAULT(0),
                CreatedOn datetime NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_CreatedOn_Save DEFAULT(GETDATE()),
                ModifiedOn datetime NOT NULL CONSTRAINT DF_POS_ItemMasterStatusRules_ModifiedOn_Save DEFAULT(GETDATE())
            )
        END

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
            WHERE ItemId = @ItemId
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
            )
        END

        SELECT 'SUCCESS' AS Result
    END
    ELSE IF(@_Operation = 'GETBYID')
    BEGIN
        SELECT Description, NameInLocalLanguage, ItemTypeId, ForCustomerType, BaseUnitId, IM.GroupId, CategoryId, BrandId, ItemId
        FROM ItemMaster AS IM
        WHERE ItemId = @ItemId
    END
    ELSE IF(@_Operation = 'GETITEM')
    BEGIN
        SELECT IM.ItemId, ItemNo, [Description], NameInLocalLanguage, ItemTypeId, IMT.ItemType, BR.BrandName, UM.UnitName,
               PS.PriceLevel, GP.GroupName, CG.CategoryName, ForCustomerType, BaseUnitId, IM.GroupId, CategoryId, BrandId,
                ISNULL(SalH.HoldQty, 0) as HoldQty,IM.Barcode,IM.HSNCode,IM.Order_Cycle_Days,IM.Box_Quantity,IM.Is_Perishable

        FROM ItemMaster AS IM
        LEFT JOIN Brands AS BR ON IM.BrandId = BR.Id
        LEFT JOIN ItemTypes AS IMT ON IM.ItemTypeId = IMT.Id
        LEFT JOIN UnitMaster AS UM ON IM.BaseUnitId = UM.UnitID
        LEFT JOIN [Group] AS GP ON IM.GroupId = GP.Id
        LEFT JOIN Category AS CG ON IM.CategoryId = CG.Id
        LEFT JOIN PriceLevels AS PS ON IM.ForCustomerType = PS.PriceLevel
        LEFT JOIN (
            SELECT Sd.ItemId as ItemId, SUM(qty) as HoldQty
            FROM SMaster as Sm
            LEFT JOIN SDetails as Sd ON Sm.BillNo = Sd.BillNo
            WHERE Sm.Status = 'Hold' AND Sd.ItemId = @ItemId
            GROUP BY Sd.ItemId
        ) as SalH ON SalH.ItemId = IM.ItemId
        WHERE IM.ItemId = @ItemId

        SELECT CompanyId, BranchId, BranchName, FinYearId, ItemId, UnitId, Unit,
               Packing, Cost, MarginPer, MarginAmt, RetailPrice, WholeSalePrice, CreditPrice, CardPrice, MRP,
               StaffPrice, MinPrice,
               Stock, OrderedStock, StockValue, ReOrder, BarCode, AliasBarcode, OpnStk,
               TaxType, TaxPer, TaxAmt, Photo,
               MDWalkinPrice, MDRetailPrice, MDCreditPrice, MDMrpPrice, MDCardPrice, MDStaffPrice, MDMinPrice
        FROM PriceSettings
        WHERE ItemId = @ItemId
        ORDER BY Packing ASC

        SELECT Pm.LedgerID, Pm.VendorName, pd.Cost, pd.Unit, Pm.InvoiceDate, Pm.PurchaseNo, Pm.InvoiceNo
        FROM PMaster as Pm
        LEFT JOIN PDetails as Pd ON pm.PurchaseNo = Pd.PurchaseNo
        WHERE pd.ItemID = @ItemId

        SELECT Id, ItemId, Barcode FROM ItemAlternativeBarcode WHERE ItemId = @ItemId
    END
    ELSE IF(@_Operation = 'UPDATE')
    BEGIN
        IF EXISTS (SELECT 1 FROM ItemMaster WHERE [Description] = @Description AND ItemId <> @ItemId)
        BEGIN
            SELECT 'NAME EXISTS1'
        END
        ELSE
        BEGIN
            IF EXISTS(SELECT * FROM ItemMaster WHERE [Description] = @Description AND Active = 0 AND ItemId <> @ItemId)
            BEGIN
                SELECT 'NAME EXISTS2'
            END
            ELSE IF EXISTS(SELECT * FROM ItemMaster WHERE Active = 1 AND ItemId = @ItemId)
            BEGIN
                SELECT 'CANNOT UPDATE DELETED ITEM.'
            END
            ELSE
            BEGIN
                UPDATE ItemMaster SET
                    [Description] = @Description,
                    BarCode = @BarCode,
                    ItemTypeId = @ItemTypeId,
                    VendorId = @VendorId,
                    BrandId = @BrandId,
                    GroupId = @GroupId,
                    CategoryId = @CategoryId,
                    BaseUnitId = @BaseUnitId,
                    ForCustomerType = @ForCustomerType,
                    NameInLocalLanguage = @NameInLocalLanguage,
                    HSNCode = @HSNCode,
                    Order_Cycle_Days = @Order_Cycle_Days,
                    Box_Quantity = @Box_Quantity,
                    Is_Perishable = @Is_Perishable
                WHERE ItemId = @ItemId
                SELECT 'SUCCESS'
            END
        END
    END
    ELSE IF(@_Operation = 'CHECKBARCODE')
    BEGIN
        SELECT ItemId FROM ItemMaster WHERE Barcode = @Barcode
        UNION
        SELECT ItemId FROM PriceSettings WHERE BarCode = @Barcode
        UNION
        SELECT ItemId FROM PriceSettings WHERE AliasBarcode = @Barcode AND AliasBarcode IS NOT NULL AND AliasBarcode != ''
        UNION
        SELECT ItemId FROM ItemAlternativeBarcode WHERE Barcode = @Barcode
    END
    ELSE IF(@_Operation = 'GETITEMUOMS')
    BEGIN
        SELECT DISTINCT
            PS.UnitId AS UnitID,
            PS.Unit AS UnitName,
            PS.Packing AS Packing
        FROM PriceSettings PS
        WHERE PS.ItemId = @ItemId
        AND PS.UnitId IS NOT NULL
        AND PS.CompanyId = @CompanyId
        AND PS.BranchId = @BranchId
        ORDER BY PS.Packing
    END
    ELSE IF(@_Operation = 'HoldItem')
    BEGIN
        SELECT Sm.BillNo, Sm.LedgerID, Sm.CustomerName, Sd.Qty as HoldQty, Sd.Unit
        FROM SMaster as Sm
        LEFT JOIN SDetails as Sd ON Sm.BillNo = Sd.BillNo
        WHERE Sm.Status = 'Hold' AND Sd.ItemId = @ItemId
    END
END
GO
