-- ============================================================================
-- Stored Procedure: dbo.POS_RolePermission
-- Description:      Handles all Role-Based Access Control (RBAC), Roles, Forms,
--                   and Permission Management operations.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_RolePermission]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_RolePermission] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_RolePermission]
    @_Operation     NVARCHAR(50)  = NULL,
    @RoleID         INT           = NULL,
    @RoleIds        NVARCHAR(MAX) = NULL,
    @RoleName       NVARCHAR(100) = NULL,
    @FormID         INT           = NULL,
    @FormKey        NVARCHAR(100) = NULL,
    @FormName       NVARCHAR(200) = NULL,
    @Category       NVARCHAR(100) = NULL,
    @CanView        BIT           = 0,
    @CanAdd         BIT           = 0,
    @CanEdit        BIT           = 0,
    @CanDelete      BIT           = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Op NVARCHAR(50) = UPPER(RTRIM(LTRIM(ISNULL(@_Operation, ''))));

    -- ========================================================================
    -- 1. ENSURE SCHEMA (Tables & Columns)
    -- ========================================================================
    IF @Op = 'ENSURESCHEMA'
    BEGIN
        -- Ensure dbo.Forms exists
        IF OBJECT_ID('dbo.Forms', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Forms (
                FormID   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                FormKey  NVARCHAR(100) NULL,
                FormName NVARCHAR(200) NULL,
                Category NVARCHAR(100) NULL,
                IsActive BIT NULL DEFAULT 1
            );
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Forms') AND name = 'FormKey')
                ALTER TABLE dbo.Forms ADD FormKey NVARCHAR(100) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Forms') AND name = 'Category')
                ALTER TABLE dbo.Forms ADD Category NVARCHAR(100) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Forms') AND name = 'IsActive')
                ALTER TABLE dbo.Forms ADD IsActive BIT NULL DEFAULT 1;
        END

        -- Ensure dbo.RolePermissions exists
        IF OBJECT_ID('dbo.RolePermissions', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.RolePermissions (
                PermissionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                RoleID       INT NOT NULL,
                FormID       INT NOT NULL,
                FormKey      NVARCHAR(100) NULL,
                FormName     NVARCHAR(200) NULL,
                CanView      BIT NOT NULL DEFAULT 0,
                CanAdd       BIT NOT NULL DEFAULT 0,
                CanEdit      BIT NOT NULL DEFAULT 0,
                CanDelete    BIT NOT NULL DEFAULT 0
            );
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey')
                ALTER TABLE dbo.RolePermissions ADD FormKey NVARCHAR(100) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName')
                ALTER TABLE dbo.RolePermissions ADD FormName NVARCHAR(200) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'CanView')
                ALTER TABLE dbo.RolePermissions ADD CanView BIT NOT NULL DEFAULT 0;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'CanAdd')
                ALTER TABLE dbo.RolePermissions ADD CanAdd BIT NOT NULL DEFAULT 0;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'CanEdit')
                ALTER TABLE dbo.RolePermissions ADD CanEdit BIT NOT NULL DEFAULT 0;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'CanDelete')
                ALTER TABLE dbo.RolePermissions ADD CanDelete BIT NOT NULL DEFAULT 0;
        END

        SELECT 'Schema verified' AS Result;
        RETURN;
    END

    -- ========================================================================
    -- 2. SEED DEFAULT ROLES
    -- ========================================================================
    IF @Op = 'SEEDROLES'
    BEGIN
        IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            CREATE TABLE #DefaultRoles (RoleName NVARCHAR(100));
            INSERT INTO #DefaultRoles (RoleName) VALUES 
            (N'Admin'),
            (N'Manager'),
            (N'Supervisor'),
            (N'Cashier'),
            (N'Sales Man'),
            (N'Accountant'),
            (N'Inventory Manager'),
            (N'Standard User');

            IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
            BEGIN
                INSERT INTO #DefaultRoles (RoleName)
                SELECT DISTINCT UserLevel FROM dbo.UserLevel WHERE ISNULL(UserLevel, '') <> '';

                INSERT INTO dbo.UserLevel (UserLevel)
                SELECT d.RoleName 
                FROM #DefaultRoles d
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.UserLevel u 
                    WHERE UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(d.RoleName))) 
                       OR (d.RoleName IN (N'Admin', N'Administrator') AND UPPER(RTRIM(LTRIM(u.UserLevel))) IN (N'ADMIN', N'ADMINISTRATOR'))
                );
            END;

            DECLARE @HasIsActive BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'IsActive') THEN 1 ELSE 0 END;
            DECLARE @HasIsDelete BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'IsDelete') THEN 1 ELSE 0 END;
            DECLARE @HasUserLevelID BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'UserLevelID') THEN 1 ELSE 0 END;
            DECLARE @SqlRoles NVARCHAR(MAX);

            SET @SqlRoles = N'
            INSERT INTO dbo.Roles (RoleName' + 
                CASE WHEN @HasIsActive = 1 THEN N', IsActive' ELSE N'' END + 
                CASE WHEN @HasIsDelete = 1 THEN N', IsDelete' ELSE N'' END + N')
            SELECT DISTINCT d.RoleName' + 
                CASE WHEN @HasIsActive = 1 THEN N', 1' ELSE N'' END + 
                CASE WHEN @HasIsDelete = 1 THEN N', 0' ELSE N'' END + N'
            FROM #DefaultRoles d
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.Roles r 
                WHERE UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(d.RoleName))) 
                   OR (d.RoleName IN (N''Admin'', N''Administrator'') AND UPPER(RTRIM(LTRIM(r.RoleName))) IN (N''ADMIN'', N''ADMINISTRATOR''))
            );';
            EXEC sp_executesql @SqlRoles;

            IF @HasUserLevelID = 1 AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
            BEGIN
                EXEC sp_executesql N'
                UPDATE r
                SET r.UserLevelID = u.UserLevelID
                FROM dbo.Roles r
                INNER JOIN dbo.UserLevel u ON UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(u.UserLevel)));';
            END;

            DROP TABLE #DefaultRoles;
        END

        SELECT 'Roles seeded' AS Result;
        RETURN;
    END

    -- ========================================================================
    -- 3. SEED SYSTEM FORMS
    -- ========================================================================
    IF @Op = 'SEEDFORMS'
    BEGIN
        IF OBJECT_ID('dbo.Forms', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Forms (
                FormID   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                FormKey  NVARCHAR(100) NULL,
                FormName NVARCHAR(200) NULL,
                Category NVARCHAR(100) NULL,
                IsActive BIT NULL DEFAULT 1
            );
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Forms') AND name = 'FormKey')
                ALTER TABLE dbo.Forms ADD FormKey NVARCHAR(100) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Forms') AND name = 'Category')
                ALTER TABLE dbo.Forms ADD Category NVARCHAR(100) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Forms') AND name = 'IsActive')
                ALTER TABLE dbo.Forms ADD IsActive BIT NULL DEFAULT 1;
        END

        CREATE TABLE #FormsToEnsure (
            FormKey  NVARCHAR(100),
            FormName NVARCHAR(200),
            Category NVARCHAR(100)
        );

        INSERT INTO #FormsToEnsure (FormKey, FormName, Category) VALUES
        -- Master
        ('company', 'Company Setup', 'Master'),
        ('branch', 'Branch Setup', 'Master'),
        ('state', 'State Master', 'Master'),
        ('country', 'Country Master', 'Master'),
        ('currency', 'Currency Master', 'Master'),
        ('group', 'Group Master', 'Master'),
        ('category', 'Category Master', 'Master'),
        ('brand', 'Brand Master', 'Master'),
        ('ItemMaster', 'Item Master', 'Master'),
        ('itemtype', 'Item Type Master', 'Master'),
        ('unit', 'Unit Master', 'Master'),
        ('line', 'Line Master', 'Master'),
        ('rack', 'Rack Master', 'Master'),
        ('row', 'Row Master', 'Master'),
        ('reason', 'Reason Master', 'Master'),
        ('users', 'Users & Security Setup', 'Master'),
        ('paymode', 'General Paymode Setup', 'Master'),
        ('TaxManagement', 'Tax Management & Rates', 'Master'),
        ('counter', 'Counter Master', 'Master'),
        ('pricesetting', 'Price Level Settings', 'Master'),

        -- Transaction
        ('pos', 'POS Terminal', 'Transaction'),
        ('sales', 'Sales Invoice', 'Transaction'),
        ('salesreturn', 'Sales Return', 'Transaction'),
        ('purchase', 'Purchase Invoice', 'Transaction'),
        ('purchaseorder', 'Purchase Order', 'Transaction'),
        ('purchasereturn', 'Purchase Return', 'Transaction'),
        ('stockadjustment', 'Stock Adjustment', 'Transaction'),
        ('stocktransfer', 'Stock Transfer', 'Transaction'),
        ('grn', 'Goods Received Notes (GRN)', 'Transaction'),
        ('quotation', 'Sales Quotation / Estimate', 'Transaction'),
        ('deliverychallan', 'Delivery Challan', 'Transaction'),

        -- Accounts
        ('customer', 'Customer Management', 'Accounts'),
        ('vendor', 'Vendor Management', 'Accounts'),
        ('ledgers', 'Account Ledgers', 'Accounts'),
        ('accountgroup', 'Account Group Setup', 'Accounts'),
        ('chartofaccount', 'Chart of Accounts', 'Accounts'),
        ('receipt', 'Customer Receipt', 'Accounts'),
        ('payment', 'Vendor Payment', 'Accounts'),
        ('generalpayment', 'General Payment Voucher', 'Accounts'),
        ('generalreceipt', 'General Receipt Voucher', 'Accounts'),
        ('contra', 'Contra Voucher', 'Accounts'),
        ('journal', 'Journal Voucher', 'Accounts'),
        ('debitnote', 'Debit Note Voucher', 'Accounts'),
        ('creditnote', 'Credit Note Voucher', 'Accounts'),
        ('bankreconciliation', 'Bank Reconciliation', 'Accounts'),
        ('manualpartybalance', 'Manual Party Balance Entry', 'Accounts'),
        ('expenseentry', 'Expense Voucher Entry', 'Accounts'),

        -- Reports & Analytics
        ('dashboard', 'Executive Dashboard & KPIs', 'Reports'),
        ('salesanalytics', 'Sales Analytics Dashboard', 'Reports'),
        ('purchaseanalytics', 'Purchase Analytics Dashboard', 'Reports'),
        ('stockanalytics', 'Stock Analytics Dashboard', 'Reports'),
        ('smartreorder', 'Smart Reorder Dashboard', 'Reports'),
        ('itemreport', 'Item Report & Details', 'Reports'),
        ('stockreport', 'Stock Report', 'Reports'),
        ('salesreport', 'Sales Summary Report', 'Reports'),
        ('purchasereport', 'Purchase Summary Report', 'Reports'),
        ('vendorpurchasereport', 'Vendor Purchase Report', 'Reports'),
        ('tradingpl', 'Trading & Profit / Loss Account', 'Reports'),
        ('balancesheet', 'Balance Sheet', 'Reports'),
        ('trialbalance', 'Trial Balance', 'Reports'),
        ('cashbankbook', 'Cash & Bank Book', 'Reports'),
        ('daybook', 'Day Book', 'Reports'),
        ('bankstatement', 'Bank Statement Report', 'Reports'),
        ('shiftreconciliation', 'Shift Reconciliation Report', 'Reports'),
        ('vendoroutstanding', 'Vendor Outstanding Report', 'Reports'),
        ('customerledger', 'Customer Ledger Report', 'Reports'),
        ('customeroutstanding', 'Customer Outstanding Report', 'Reports'),
        ('customerreceiptreport', 'Customer Receipt Report', 'Reports'),
        ('salesprofit', 'Sales Profit Report', 'Reports'),
        ('counterreport', 'Counter Sales Report', 'Reports'),
        ('salesmanincentive', 'Salesman Incentive Report', 'Reports'),
        ('customerwisesalessummary', 'Customerwise Sales Summary Report', 'Reports'),
        ('salesmanwisesalessummary', 'Salesmanwise Sales Summary Report', 'Reports'),
        ('itemwisesalessummary', 'Itemwise Sales Summary Report', 'Reports'),
        ('stockvaluation', 'Stock Valuation Report', 'Reports'),
        ('lowstockalert', 'Low Stock Alert Report', 'Reports'),
        ('stockadjustmentreport', 'Stock Adjustment Report', 'Reports'),
        ('inactiveitemsreport', 'Inactive Items Report', 'Reports'),
        ('manualpartybalancereport', 'Manual Party Balance Report', 'Reports'),
        ('combinedpartybalancereport', 'Combined Party Balance Report', 'Reports'),
        ('stockreportadvanced', 'Advanced Stock Report', 'Reports'),
        ('stockaudit', 'Stock Audit Report', 'Reports'),
        ('salesdetails', 'Sales Details Report', 'Reports'),
        ('salesreturnreport', 'Sales Return Report', 'Reports'),
        ('salesreturndetails', 'Sales Return Details Report', 'Reports'),
        ('purchasedetails', 'Purchase Details Report', 'Reports'),
        ('purchasereturnreport', 'Purchase Return Report', 'Reports'),
        ('purchasereturndetails', 'Purchase Return Details Report', 'Reports'),
        ('grnreport', 'Goods Received (GRN) Report', 'Reports'),
        ('cashinout', 'Cash In / Out Report', 'Reports'),
        ('profitloss', 'Profit and Loss Account', 'Reports'),
        ('businessexpense', 'Business Expense Report', 'Reports'),
        ('inputgst', 'Input GST Report', 'Reports'),
        ('outputgst', 'Output GST Report', 'Reports'),
        ('govtgst', 'Government GST Return Report', 'Reports'),

        -- Settings & Utilities
        ('possettings', 'Sale Settings & Preferences', 'Settings'),
        ('excelimport', 'Excel Import / Export Utility', 'Utilities'),
        ('rolepermissions', 'Role Permissions Management', 'Settings'),
        ('ActivityLog', 'Activity Log (Master)', 'Settings'),
        ('itemhistory', 'Item History Log', 'Settings'),
        ('transactionactivity', 'Transaction Activity Log', 'Settings'),
        ('useractivity', 'User Activity Log', 'Settings'),
        ('itemstockactivity', 'Stock Activity Log', 'Settings'),
        ('yearclosing', 'Financial Year Closing', 'Settings'),
        ('applanguage', 'Application Language Settings', 'Settings'),
        ('nexorisai', 'Nexoris AI Copilot', 'Utilities'),
        ('barcode', 'Barcode Label Designer & Print', 'Utilities'),
        ('plu', 'PLU Weighing Scale Setup', 'Utilities'),
        ('openingstock', 'Opening Stock Entry Utility', 'Utilities'),
        ('counterclosing', 'Counter Shift Closing Utility', 'Utilities'),
        ('changeitemno', 'Change Item Number Utility', 'Utilities'),
        ('databasemaintenance', 'Database Backup & Maintenance', 'Utilities'),
        ('printsetup', 'Receipt & Invoice Print Setup', 'Settings'),
        ('systemsettings', 'System Preferences & Hardware', 'Settings'),
        ('dataaudit', 'Data Integrity Audit Utility', 'Utilities');

        DECLARE @IsFormsIdent BIT = COLUMNPROPERTY(OBJECT_ID('dbo.Forms'), 'FormID', 'IsIdentity');

        IF @IsFormsIdent = 1
        BEGIN
            INSERT INTO dbo.Forms (FormKey, FormName, Category, IsActive)
            SELECT f.FormKey, f.FormName, f.Category, 1
            FROM #FormsToEnsure f
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.Forms t 
                WHERE (ISNULL(t.FormKey, '') <> '' AND UPPER(RTRIM(LTRIM(t.FormKey))) = UPPER(RTRIM(LTRIM(f.FormKey))))
                   OR (ISNULL(t.FormName, '') <> '' AND UPPER(RTRIM(LTRIM(t.FormName))) = UPPER(RTRIM(LTRIM(f.FormName))))
            );
        END
        ELSE
        BEGIN
            DECLARE @MaxFormID INT = ISNULL((SELECT MAX(FormID) FROM dbo.Forms), 0);
            INSERT INTO dbo.Forms (FormID, FormKey, FormName, Category, IsActive)
            SELECT @MaxFormID + ROW_NUMBER() OVER (ORDER BY f.Category, f.FormName), f.FormKey, f.FormName, f.Category, 1
            FROM #FormsToEnsure f
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.Forms t 
                WHERE (ISNULL(t.FormKey, '') <> '' AND UPPER(RTRIM(LTRIM(t.FormKey))) = UPPER(RTRIM(LTRIM(f.FormKey))))
                   OR (ISNULL(t.FormName, '') <> '' AND UPPER(RTRIM(LTRIM(t.FormName))) = UPPER(RTRIM(LTRIM(f.FormName))))
            );
        END

        UPDATE t
        SET t.Category = CASE WHEN ISNULL(t.Category, '') = '' THEN f.Category ELSE t.Category END,
            t.FormKey  = CASE WHEN ISNULL(t.FormKey, '') = '' THEN f.FormKey ELSE t.FormKey END,
            t.IsActive = ISNULL(t.IsActive, 1)
        FROM dbo.Forms t
        INNER JOIN #FormsToEnsure f ON UPPER(RTRIM(LTRIM(t.FormName))) = UPPER(RTRIM(LTRIM(f.FormName)))
                                    OR UPPER(RTRIM(LTRIM(t.FormKey)))  = UPPER(RTRIM(LTRIM(f.FormKey)));

        DROP TABLE #FormsToEnsure;
        SELECT 'Forms seeded' AS Result;
        RETURN;
    END

    -- ========================================================================
    -- 4. GET ALL ROLES
    -- ========================================================================
    IF @Op = 'GETALLROLES'
    BEGIN
        IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL AND OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            SELECT 
                u.UserLevelID AS RoleID,
                u.UserLevel AS RoleName,
                ISNULL(r.RoleID, u.UserLevelID) AS UserLevelID
            FROM dbo.UserLevel u
            LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
            WHERE ISNULL(u.UserLevel, '') <> ''
            ORDER BY u.UserLevelID;
        END
        ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
        BEGIN
            SELECT UserLevelID AS RoleID, UserLevel AS RoleName, UserLevelID 
            FROM dbo.UserLevel 
            WHERE ISNULL(UserLevel, '') <> ''
            ORDER BY UserLevelID;
        END
        ELSE IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            SELECT RoleID, RoleName, RoleID AS UserLevelID 
            FROM dbo.Roles 
            ORDER BY RoleID;
        END
        RETURN;
    END

    -- ========================================================================
    -- 5. GET ACTIVE FORMS
    -- ========================================================================
    IF @Op = 'GETFORMS'
    BEGIN
        IF OBJECT_ID('dbo.Forms', 'U') IS NOT NULL
        BEGIN
            SELECT FormID, 
                   ISNULL(FormKey, '') AS FormKey, 
                   ISNULL(FormName, '') AS FormName, 
                   ISNULL(Category, 'General') AS Category 
            FROM dbo.Forms 
            WHERE ISNULL(IsActive, 1) = 1
            ORDER BY Category, FormName;
        END
        RETURN;
    END

    -- ========================================================================
    -- 6. GET ROLE BY NAME
    -- ========================================================================
    IF @Op = 'GETROLEBYNAME'
    BEGIN
        IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL AND OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            SELECT TOP 1 
                ISNULL(r.RoleID, u.UserLevelID) AS RoleID, 
                u.UserLevel AS RoleName, 
                u.UserLevelID 
            FROM dbo.UserLevel u
            LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
            WHERE UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(ISNULL(@RoleName, ''))))
               OR UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(ISNULL(@RoleName, ''))));
        END
        ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
        BEGIN
            SELECT TOP 1 UserLevelID AS RoleID, UserLevel AS RoleName, UserLevelID 
            FROM dbo.UserLevel 
            WHERE UPPER(RTRIM(LTRIM(UserLevel))) = UPPER(RTRIM(LTRIM(ISNULL(@RoleName, ''))));
        END
        ELSE IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            SELECT TOP 1 RoleID, RoleName, RoleID AS UserLevelID 
            FROM dbo.Roles 
            WHERE UPPER(RTRIM(LTRIM(RoleName))) = UPPER(RTRIM(LTRIM(ISNULL(@RoleName, ''))));
        END
        RETURN;
    END

    -- ========================================================================
    -- 7. GET ROLE BY LEVEL ID
    -- ========================================================================
    IF @Op = 'GETROLEBYLEVELID'
    BEGIN
        IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL AND OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            SELECT TOP 1 
                ISNULL(r.RoleID, u.UserLevelID) AS RoleID, 
                u.UserLevel AS RoleName, 
                u.UserLevelID 
            FROM dbo.UserLevel u
            LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
            WHERE u.UserLevelID = @RoleID OR r.RoleID = @RoleID;
        END
        ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
        BEGIN
            SELECT TOP 1 UserLevelID AS RoleID, UserLevel AS RoleName, UserLevelID 
            FROM dbo.UserLevel 
            WHERE UserLevelID = @RoleID;
        END
        ELSE IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        BEGIN
            SELECT TOP 1 RoleID, RoleName, RoleID AS UserLevelID 
            FROM dbo.Roles 
            WHERE RoleID = @RoleID;
        END
        RETURN;
    END

    -- ========================================================================
    -- 8. GET PERMISSIONS BY ROLE (Single RoleID or comma-separated RoleIds)
    -- ========================================================================
    IF @Op = 'GETBYROLE'
    BEGIN
        IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
        BEGIN
            DECLARE @HasFormKey BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey') THEN 1 ELSE 0 END;
            DECLARE @HasFormName BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName') THEN 1 ELSE 0 END;

            IF ISNULL(@RoleIds, '') <> ''
            BEGIN
                IF @HasFormKey = 1 AND @HasFormName = 1
                BEGIN
                    SELECT 
                        p.RoleID, 
                        p.FormID, 
                        ISNULL(NULLIF(p.FormKey, ''), ISNULL(f.FormKey, '')) AS FormKey, 
                        ISNULL(NULLIF(p.FormName, ''), ISNULL(f.FormName, '')) AS FormName, 
                        p.CanView, 
                        p.CanAdd, 
                        p.CanEdit, 
                        p.CanDelete 
                    FROM dbo.RolePermissions p
                    LEFT JOIN dbo.Forms f ON p.FormID = f.FormID
                    WHERE (',' + RTRIM(LTRIM(@RoleIds)) + ',' LIKE '%,' + CAST(p.RoleID AS NVARCHAR(20)) + ',%');
                END
                ELSE
                BEGIN
                    SELECT 
                        p.RoleID, 
                        p.FormID, 
                        ISNULL(f.FormKey, '') AS FormKey, 
                        ISNULL(f.FormName, '') AS FormName, 
                        p.CanView, 
                        p.CanAdd, 
                        p.CanEdit, 
                        p.CanDelete 
                    FROM dbo.RolePermissions p
                    LEFT JOIN dbo.Forms f ON p.FormID = f.FormID
                    WHERE (',' + RTRIM(LTRIM(@RoleIds)) + ',' LIKE '%,' + CAST(p.RoleID AS NVARCHAR(20)) + ',%');
                END
            END
            ELSE
            BEGIN
                IF @HasFormKey = 1 AND @HasFormName = 1
                BEGIN
                    SELECT 
                        p.RoleID, 
                        p.FormID, 
                        ISNULL(NULLIF(p.FormKey, ''), ISNULL(f.FormKey, '')) AS FormKey, 
                        ISNULL(NULLIF(p.FormName, ''), ISNULL(f.FormName, '')) AS FormName, 
                        p.CanView, 
                        p.CanAdd, 
                        p.CanEdit, 
                        p.CanDelete 
                    FROM dbo.RolePermissions p
                    LEFT JOIN dbo.Forms f ON p.FormID = f.FormID
                    WHERE p.RoleID = @RoleID;
                END
                ELSE
                BEGIN
                    SELECT 
                        p.RoleID, 
                        p.FormID, 
                        ISNULL(f.FormKey, '') AS FormKey, 
                        ISNULL(f.FormName, '') AS FormName, 
                        p.CanView, 
                        p.CanAdd, 
                        p.CanEdit, 
                        p.CanDelete 
                    FROM dbo.RolePermissions p
                    LEFT JOIN dbo.Forms f ON p.FormID = f.FormID
                    WHERE p.RoleID = @RoleID;
                END
            END
        END
        ELSE IF OBJECT_ID('dbo.POS_RolePermissions', 'U') IS NOT NULL
        BEGIN
            SELECT 
                p.RoleID, 
                p.FormID, 
                ISNULL(f.FormKey, '') AS FormKey, 
                ISNULL(f.FormName, '') AS FormName, 
                p.CanView, 
                p.CanAdd, 
                p.CanEdit, 
                p.CanDelete 
            FROM dbo.POS_RolePermissions p
            LEFT JOIN dbo.Forms f ON p.FormID = f.FormID
            WHERE p.RoleID = @RoleID OR (ISNULL(@RoleIds, '') <> '' AND (',' + RTRIM(LTRIM(@RoleIds)) + ',' LIKE '%,' + CAST(p.RoleID AS NVARCHAR(20)) + ',%'));
        END
        RETURN;
    END

    -- ========================================================================
    -- 9. SAVE PERMISSION (Upsert single permission)
    -- ========================================================================
    IF @Op = 'SAVE'
    BEGIN
        DECLARE @RealFormID INT = @FormID;

        IF OBJECT_ID('dbo.Forms', 'U') IS NOT NULL
        BEGIN
            IF @RealFormID <= 0 OR NOT EXISTS (SELECT 1 FROM dbo.Forms WHERE FormID = @RealFormID)
            BEGIN
                SELECT TOP 1 @RealFormID = FormID 
                FROM dbo.Forms 
                WHERE (ISNULL(@FormKey, '') <> '' AND UPPER(RTRIM(LTRIM(FormKey))) = UPPER(RTRIM(LTRIM(@FormKey))))
                   OR (ISNULL(@FormName, '') <> '' AND UPPER(RTRIM(LTRIM(FormName))) = UPPER(RTRIM(LTRIM(@FormName))));

                IF @RealFormID IS NULL OR @RealFormID <= 0 OR NOT EXISTS (SELECT 1 FROM dbo.Forms WHERE FormID = @RealFormID)
                BEGIN
                    DECLARE @IsIdentSave BIT = COLUMNPROPERTY(OBJECT_ID('dbo.Forms'), 'FormID', 'IsIdentity');
                    IF @IsIdentSave = 1
                    BEGIN
                        INSERT INTO dbo.Forms (FormKey, FormName, Category, IsActive)
                        VALUES (@FormKey, @FormName, ISNULL(@Category, 'General'), 1);
                        SET @RealFormID = SCOPE_IDENTITY();
                    END
                    ELSE
                    BEGIN
                        SELECT @RealFormID = ISNULL(MAX(FormID), 0) + 1 FROM dbo.Forms;
                        INSERT INTO dbo.Forms (FormID, FormKey, FormName, Category, IsActive)
                        VALUES (@RealFormID, @FormKey, @FormName, ISNULL(@Category, 'General'), 1);
                    END
                END
            END
        END

        IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
        BEGIN
            DECLARE @HasFK BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey') THEN 1 ELSE 0 END;
            DECLARE @HasFN BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName') THEN 1 ELSE 0 END;

            IF @HasFK = 1 AND @HasFN = 1
            BEGIN
                IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @RoleID AND FormID = @RealFormID)
                BEGIN
                    UPDATE dbo.RolePermissions
                    SET CanView = @CanView, 
                        CanAdd = @CanAdd, 
                        CanEdit = @CanEdit, 
                        CanDelete = @CanDelete,
                        FormKey = ISNULL(@FormKey, FormKey), 
                        FormName = ISNULL(@FormName, FormName)
                    WHERE RoleID = @RoleID AND FormID = @RealFormID;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.RolePermissions (RoleID, FormID, FormKey, FormName, CanView, CanAdd, CanEdit, CanDelete)
                    VALUES (@RoleID, @RealFormID, @FormKey, @FormName, @CanView, @CanAdd, @CanEdit, @CanDelete);
                END
            END
            ELSE
            BEGIN
                IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @RoleID AND FormID = @RealFormID)
                BEGIN
                    UPDATE dbo.RolePermissions
                    SET CanView = @CanView, 
                        CanAdd = @CanAdd, 
                        CanEdit = @CanEdit, 
                        CanDelete = @CanDelete
                    WHERE RoleID = @RoleID AND FormID = @RealFormID;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
                    VALUES (@RoleID, @RealFormID, @CanView, @CanAdd, @CanEdit, @CanDelete);
                END
            END
        END

        IF OBJECT_ID('dbo.POS_RolePermissions', 'U') IS NOT NULL
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.POS_RolePermissions WHERE RoleID = @RoleID AND FormID = @RealFormID)
            BEGIN
                UPDATE dbo.POS_RolePermissions
                SET CanView = @CanView, 
                    CanAdd = @CanAdd, 
                    CanEdit = @CanEdit, 
                    CanDelete = @CanDelete
                WHERE RoleID = @RoleID AND FormID = @RealFormID;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.POS_RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
                VALUES (@RoleID, @RealFormID, @CanView, @CanAdd, @CanEdit, @CanDelete);
            END
        END

        SELECT 'Success' AS Result;
        RETURN;
    END

    -- ========================================================================
    -- 10. REGISTER NEW FORM & GRANT ADMIN RIGHTS
    -- ========================================================================
    IF @Op = 'REGISTERFORM'
    BEGIN
        IF OBJECT_ID('dbo.Forms', 'U') IS NOT NULL
        BEGIN
            DECLARE @RegFormID INT = 0;
            SELECT TOP 1 @RegFormID = FormID 
            FROM dbo.Forms 
            WHERE (ISNULL(@FormKey, '') <> '' AND UPPER(RTRIM(LTRIM(FormKey))) = UPPER(RTRIM(LTRIM(@FormKey))))
               OR (ISNULL(@FormName, '') <> '' AND UPPER(RTRIM(LTRIM(FormName))) = UPPER(RTRIM(LTRIM(@FormName))));

            IF @RegFormID IS NULL OR @RegFormID <= 0
            BEGIN
                DECLARE @IsIdentReg BIT = COLUMNPROPERTY(OBJECT_ID('dbo.Forms'), 'FormID', 'IsIdentity');
                IF @IsIdentReg = 1
                BEGIN
                    INSERT INTO dbo.Forms (FormKey, FormName, Category, IsActive)
                    VALUES (@FormKey, @FormName, ISNULL(@Category, 'General'), 1);
                    SET @RegFormID = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @RegFormID = ISNULL(MAX(FormID), 0) + 1 FROM dbo.Forms;
                    INSERT INTO dbo.Forms (FormID, FormKey, FormName, Category, IsActive)
                    VALUES (@RegFormID, @FormKey, @FormName, ISNULL(@Category, 'General'), 1);
                END
            END
            ELSE
            BEGIN
                UPDATE dbo.Forms
                SET FormKey = CASE WHEN ISNULL(FormKey, '') = '' THEN @FormKey ELSE FormKey END,
                    FormName = CASE WHEN ISNULL(FormName, '') = '' THEN @FormName ELSE FormName END,
                    Category = CASE WHEN ISNULL(Category, '') = '' THEN ISNULL(@Category, 'General') ELSE Category END,
                    IsActive = 1
                WHERE FormID = @RegFormID;
            END

            -- Ensure Admin role gets view/add/edit/delete permissions automatically
            IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
            BEGIN
                DECLARE @AdminRoleID INT = 0;
                SELECT TOP 1 @AdminRoleID = RoleID FROM dbo.Roles WHERE UPPER(RTRIM(LTRIM(RoleName))) IN ('ADMIN', 'ADMINISTRATOR');
                IF @AdminRoleID <= 0 AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
                    SELECT TOP 1 @AdminRoleID = UserLevelID FROM dbo.UserLevel WHERE UPPER(RTRIM(LTRIM(UserLevel))) IN ('ADMIN', 'ADMINISTRATOR');

                IF @AdminRoleID > 0
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @AdminRoleID AND FormID = @RegFormID)
                    BEGIN
                        INSERT INTO dbo.RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
                        VALUES (@AdminRoleID, @RegFormID, 1, 1, 1, 1);
                    END
                END
            END

            SELECT @RegFormID AS FormID;
            RETURN;
        END
    END
END
GO
