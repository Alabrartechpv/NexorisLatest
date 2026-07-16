using Dapper;
using ModelClass;
using System;
using System.Data;
using System.Linq;

namespace Repository.ReportRepository
{
    public class FinalAnalysisRepository : BaseRepostitory
    {
        public FinalAnalysisModel GetFinalAnalysis()
        {
            return GetFinalAnalysis(DateTime.Today, DateTime.Today, 0, 0);
        }

        public FinalAnalysisModel GetFinalAnalysis(DateTime fromDate, DateTime toDate, int userId, int counterId)
        {
            var model = new FinalAnalysisModel();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                DateTime rangeFrom = fromDate.Date;
                DateTime rangeTo = toDate.Date;
                if (rangeTo < rangeFrom)
                {
                    DateTime swap = rangeFrom;
                    rangeFrom = rangeTo;
                    rangeTo = swap;
                }

                var p = BuildBaseParams(rangeFrom, rangeTo.AddDays(1), userId, counterId);
                model.CompanyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
                model.BranchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
                model.FinYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);
                model.UserId = userId;
                model.CounterId = counterId;
                model.UserName = userId > 0 ? GetUserName(userId) : "All Users";
                model.CounterName = counterId > 0 ? GetCounterName(counterId) : "All Counters";
                model.BranchName = !string.IsNullOrWhiteSpace(SessionContext.BranchName) ? SessionContext.BranchName : DataBase.Branch;
                model.FromDate = rangeFrom;
                model.ToDate = rangeTo;
                model.GeneratedAt = DateTime.Now;

                string purchaseMasterScope = BuildScopeFilter("PMaster", "BranchID", null, "PurchaseDate", userId, counterId);
                string salesScope = BuildScopeFilter("SMaster", "BranchId", null, "BillDate", userId, counterId);
                string salesAliasScope = BuildScopeFilter("SMaster", "BranchId", "sm", "BillDate", userId, counterId);
                string purchaseReturnScope = BuildScopeFilter("PReturnMaster", "BranchID", null, "PReturnDate", userId, counterId);
                string salesReturnScope = BuildScopeFilter("SReturnMaster", "BranchId", null, "SReturnDate", userId, counterId);

                // Total Purchase
                model.TotalPurchase = Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(GrandTotal,0)),0)
FROM PMaster
WHERE " + purchaseMasterScope + @"
  AND ISNULL(CancelFlag,0)=0;", p);
                model.TotalPurchaseCount = Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster
WHERE " + purchaseMasterScope + @"
  AND ISNULL(CancelFlag,0)=0;", p);
                FillPurchaseModeTotals(model, purchaseMasterScope, p);

                // Total Payment - Cash
                FillVendorPaymentTotals(model, p, userId);

                // Include direct purchase bill payments too, for older entries saved from FrmPurchase.
                model.TotalPaymentCash += Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(PayedAmount,0)),0)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND " + BuildModePredicate("Paymode", "cash") + @";", p);
                model.TotalPaymentCashCount += Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND ISNULL(PayedAmount,0)>0 AND " + BuildModePredicate("Paymode", "cash") + @";", p);
                model.TotalPaymentBank += Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(PayedAmount,0)),0)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND " + BuildModePredicate("Paymode", "bank") + @";", p);
                model.TotalPaymentBankCount += Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND ISNULL(PayedAmount,0)>0 AND " + BuildModePredicate("Paymode", "bank") + @";", p);
                model.TotalPaymentUpi += Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(PayedAmount,0)),0)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND " + BuildModePredicate("Paymode", "upi") + @";", p);
                model.TotalPaymentUpiCount += Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND ISNULL(PayedAmount,0)>0 AND " + BuildModePredicate("Paymode", "upi") + @";", p);
                model.TotalPaymentCard += Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(PayedAmount,0)),0)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND " + BuildModePredicate("Paymode", "card") + @";", p);
                model.TotalPaymentCardCount += Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND ISNULL(PayedAmount,0)>0 AND " + BuildModePredicate("Paymode", "card") + @";", p);
                model.TotalPaymentCheque += Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(PayedAmount,0)),0)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND " + BuildModePredicate("Paymode", "cheque") + @";", p);
                model.TotalPaymentChequeCount += Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster WHERE " + purchaseMasterScope + @" AND ISNULL(CancelFlag,0)=0 AND ISNULL(PayedAmount,0)>0 AND " + BuildModePredicate("Paymode", "cheque") + @";", p);

                // Total Outstanding (Vendor) = GrandTotal - PayedAmount
                model.TotalOutstandingVendor = Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(GrandTotal,0) - ISNULL(PayedAmount,0)),0)
FROM PMaster
WHERE " + purchaseMasterScope + @"
  AND ISNULL(GrandTotal,0) > ISNULL(PayedAmount,0) AND ISNULL(CancelFlag,0)=0;", p);
                model.TotalOutstandingVendorCount = Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster
WHERE " + purchaseMasterScope + @"
  AND ISNULL(GrandTotal,0) > ISNULL(PayedAmount,0) AND ISNULL(CancelFlag,0)=0;", p);

                // Total Cost (sold item cost for the current analysis scope)
                model.TotalCost = Safe<decimal>(@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE IF COL_LENGTH('SMaster', 'BillCost') IS NOT NULL
    SELECT ISNULL(SUM(ISNULL(BillCost,0)),0)
    FROM SMaster
    WHERE " + salesScope + @" AND ISNULL(CancelFlag,0)=0
ELSE IF OBJECT_ID('SDetails','U') IS NOT NULL AND COL_LENGTH('SDetails', 'Cost') IS NOT NULL AND COL_LENGTH('SDetails', 'Qty') IS NOT NULL
    SELECT ISNULL(SUM(ISNULL(sd.Cost,0) * ISNULL(sd.Qty,0)),0)
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo=sd.BillNo AND sm.BranchId=sd.BranchId AND sm.CompanyId=sd.CompanyId AND sm.FinYearId=sd.FinYearId
    WHERE " + salesAliasScope + @" AND ISNULL(sm.CancelFlag,0)=0
ELSE SELECT CAST(0 AS decimal(18,2));", p);
                model.TotalCostCount = Safe<int>(@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM SMaster
WHERE " + salesScope + @" AND ISNULL(CancelFlag,0)=0;", p);

                // Total Sale
                model.TotalSale = Safe<decimal>(@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(NetAmount,0)),0)
FROM SMaster
WHERE " + salesScope + @"
  AND ISNULL(CancelFlag,0)=0;", p);
                model.TotalSaleCount = Safe<int>(@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM SMaster
WHERE " + salesScope + @"
  AND ISNULL(CancelFlag,0)=0;", p);
                FillSalesModeTotals(model, salesScope, salesAliasScope, p);
                FillPartialPaymentTotals(model, salesAliasScope, p);

                // Total Profit = Sale - Cost (approximate via sale minus purchase cost)
                model.TotalProfit = model.TotalSale - model.TotalCost;
                model.TotalProfitCount = model.TotalSaleCount;

                // Purchase Return
                model.PurchaseReturn = Safe<decimal>(@"
IF OBJECT_ID('PReturnMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(GrandTotal,0)),0)
FROM PReturnMaster
WHERE " + purchaseReturnScope + @";", p);
                model.PurchaseReturnCount = Safe<int>(@"
IF OBJECT_ID('PReturnMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PReturnMaster
WHERE " + purchaseReturnScope + @";", p);

                // Sales Return
                model.SalesReturn = Safe<decimal>(@"
IF OBJECT_ID('SReturnMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(GrandTotal,0)),0)
FROM SReturnMaster
WHERE " + salesReturnScope + @";", p);
                model.SalesReturnCount = Safe<int>(@"
IF OBJECT_ID('SReturnMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM SReturnMaster
WHERE " + salesReturnScope + @";", p);

                // Non-Profit Items (items whose profit/margin <= 0)
                // Calculated as items where sale cost >= sale price
                model.NonProfitItems = ReadNonProfitSalesItems(salesAliasScope, p);
                model.NonProfitAmount = ReadNonProfitSalesAmount(salesAliasScope, p);

                // Excess Stock Items — stock > reorder level * 2
                // We try common reorder column names and pick the first that exists
                string excessSql = @"
IF OBJECT_ID('PriceSettings','U') IS NULL SELECT 0
ELSE IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('PriceSettings','U') AND name='ReOrder')
  SELECT COUNT(1) FROM PriceSettings WHERE BranchId=@BranchId AND ISNULL(Stock,0)>ISNULL(ReOrder,0)*2 AND ISNULL(ReOrder,0)>0
ELSE IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('PriceSettings','U') AND name='MinStock')
  SELECT COUNT(1) FROM PriceSettings WHERE BranchId=@BranchId AND ISNULL(Stock,0)>ISNULL(MinStock,0)*2 AND ISNULL(MinStock,0)>0
ELSE SELECT 0;";
                model.ExcessStockItems = Safe<int>(excessSql, p);

                // Out of Stock Items (stock <= 0)
                model.OutOfStockItems = Safe<int>(@"
IF OBJECT_ID('PriceSettings','U') IS NULL SELECT 0
ELSE SELECT COUNT(1) FROM PriceSettings WHERE BranchId=@BranchId AND ISNULL(Stock,0)<=0;", p);

                // Discontinued Items (items in ItemMaster with IsActive=0 or Discontinued flag)
                model.DiscontinuedItems = Safe<int>(@"
IF OBJECT_ID('ItemMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1) FROM ItemMaster
WHERE CompanyId=@CompanyId
  AND (ISNULL(IsActive,1)=0 OR ISNULL(Discontinued,0)=1);", p);

                model.DetailTable = BuildDetailTable(model);
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return model;
        }

        public DataTable GetUsers()
        {
            return ReadLookupFromTable("Users", new[] { "UserId", "UserID" }, new[] { "UserName", "Name" }, new[] { "BranchId", "BranchID" });
        }

        public DataTable GetUserScopes()
        {
            DataTable table = new DataTable();
            table.Columns.Add("UserId", typeof(int));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("CounterId", typeof(int));
            table.Columns.Add("CounterName", typeof(string));
            table.Columns.Add("DisplayName", typeof(string));

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                if (!TableExists("CounterSessions"))
                {
                    foreach (DataRow user in GetUsers().Rows)
                    {
                        int userId = Convert.ToInt32(user["Id"]);
                        string userName = Convert.ToString(user["Name"]);
                        table.Rows.Add(userId, userName, 0, "All Counters", userName + " - All Counters");
                    }

                    return table;
                }

                string usersTable = TableExists("Users") ? "Users" : null;
                string userIdColumn = usersTable == null ? null : GetFirstExistingColumn(usersTable, "UserId", "UserID");
                string userNameColumn = usersTable == null ? null : GetFirstExistingColumn(usersTable, "UserName", "Name");

                string sql = usersTable != null && !string.IsNullOrWhiteSpace(userIdColumn) && !string.IsNullOrWhiteSpace(userNameColumn)
                    ? $@"
SELECT DISTINCT cs.UserId, CAST(u.{userNameColumn} AS nvarchar(150)) AS UserName,
       cs.CounterId, ISNULL(NULLIF(cs.CounterName, ''), 'Counter ' + CAST(cs.CounterId AS varchar(20))) AS CounterName
FROM CounterSessions cs
LEFT JOIN {usersTable} u ON u.{userIdColumn} = cs.UserId
WHERE cs.BranchId=@BranchId AND cs.CompanyId=@CompanyId
ORDER BY UserName, CounterName;"
                    : @"
SELECT DISTINCT cs.UserId, 'User ' + CAST(cs.UserId AS varchar(20)) AS UserName,
       cs.CounterId, ISNULL(NULLIF(cs.CounterName, ''), 'Counter ' + CAST(cs.CounterId AS varchar(20))) AS CounterName
FROM CounterSessions cs
WHERE cs.BranchId=@BranchId AND cs.CompanyId=@CompanyId
ORDER BY UserName, CounterName;";

                var rows = DataConnection.Query(sql, BuildBaseParams()).ToList();
                foreach (var row in rows)
                {
                    int rowUserId = Convert.ToInt32(row.UserId);
                    int rowCounterId = Convert.ToInt32(row.CounterId);
                    string userName = Convert.ToString(row.UserName);
                    string counterName = Convert.ToString(row.CounterName);
                    table.Rows.Add(rowUserId, userName, rowCounterId, counterName, userName + " - " + counterName);
                }
            }
            catch
            {
            }

            return table;
        }

        public DataTable GetCounters()
        {
            return ReadLookupFromTable("CounterMaster", new[] { "CounterID", "CounterId" }, new[] { "CounterName", "Name" }, new[] { "BranchId", "BranchID" });
        }

        public DataTable GetPartialPaymentSaleDetails(DateTime fromDate, DateTime toDate, int userId, int counterId)
        {
            DataTable table = new DataTable();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                if (!TableExists("SMaster"))
                    return table;

                string paymentTable = GetSalesPaymentTable();
                if (string.IsNullOrWhiteSpace(paymentTable))
                    return table;

                string amountColumn = GetFirstExistingColumn(paymentTable, "Amount", "PaymentAmount", "PaidAmount", "ReceivedAmount");
                if (string.IsNullOrWhiteSpace(amountColumn))
                    return table;

                DateTime rangeFrom = fromDate.Date;
                DateTime rangeTo = toDate.Date;
                if (rangeTo < rangeFrom)
                {
                    DateTime swap = rangeFrom;
                    rangeFrom = rangeTo;
                    rangeTo = swap;
                }

                var p = BuildBaseParams(rangeFrom, rangeTo.AddDays(1), userId, counterId);
                string salesAliasScope = BuildScopeFilter("SMaster", "BranchId", "sm", "BillDate", userId, counterId);
                bool hasSalesDetails = TableExists("SDetails");
                string detailBillNoColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "BillNo", "BillNumber") : null;
                string detailBranchColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "BranchId", "BranchID") : null;
                string detailCompanyColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "CompanyId", "CompanyID") : null;
                string detailFinYearColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "FinYearId", "FinyearId", "FinYearID") : null;
                string itemNameColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "ItemName", "Description", "Item") : null;
                string qtyColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "Qty", "Quantity") : null;
                string sellingPriceColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "UnitPrice", "Rate", "SellingPrice") : null;
                string lineAmountColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "TotalAmount", "Amount") : null;
                string slNoColumn = hasSalesDetails ? GetFirstExistingColumn("SDetails", "SlNO", "SlNo", "Slno") : null;
                string paymodeIdColumn = GetFirstExistingColumn(paymentTable, "PaymodeId", "PayModeId", "PayModeID");
                string paymodeNameColumn = GetFirstExistingColumn(paymentTable, "PaymodeName", "PayModeName", "PayMode");
                string payModeTable = TableExists("PayMode") ? "PayMode" : TableExists("Paymode") ? "Paymode" : null;
                string payModeTableIdColumn = string.IsNullOrWhiteSpace(payModeTable) ? null : GetFirstExistingColumn(payModeTable, "PayModeID", "PaymodeID", "PayModeId");
                string payModeTableNameColumn = string.IsNullOrWhiteSpace(payModeTable) ? null : GetFirstExistingColumn(payModeTable, "PayModeName", "PaymodeName", "Name");

                bool canJoinDetails = hasSalesDetails &&
                    !string.IsNullOrWhiteSpace(detailBillNoColumn) &&
                    !string.IsNullOrWhiteSpace(detailBranchColumn) &&
                    !string.IsNullOrWhiteSpace(detailCompanyColumn) &&
                    !string.IsNullOrWhiteSpace(detailFinYearColumn);

                string detailJoin = canJoinDetails
                    ? $@"LEFT JOIN SDetails sd ON sd.{detailBillNoColumn}=sm.BillNo AND sd.{detailBranchColumn}=sm.BranchId AND sd.{detailCompanyColumn}=sm.CompanyId AND sd.{detailFinYearColumn}=sm.FinYearId"
                    : string.Empty;
                string itemNameExpression = canJoinDetails && !string.IsNullOrWhiteSpace(itemNameColumn)
                    ? $"ISNULL(NULLIF(sd.{itemNameColumn}, ''), '(Bill total)')"
                    : "'(Bill total)'";
                string qtyExpression = canJoinDetails && !string.IsNullOrWhiteSpace(qtyColumn) ? $"ISNULL(sd.{qtyColumn},0)" : "CAST(0 AS decimal(18,2))";
                string sellingPriceExpression = canJoinDetails && !string.IsNullOrWhiteSpace(sellingPriceColumn) ? $"ISNULL(sd.{sellingPriceColumn},0)" : "CAST(0 AS decimal(18,2))";
                string lineAmountExpression = canJoinDetails && !string.IsNullOrWhiteSpace(lineAmountColumn) ? $"ISNULL(sd.{lineAmountColumn},0)" : "ISNULL(sm.NetAmount,0)";
                string paymentNameExpression = BuildSalesPaymentNameExpression("sp", paymodeNameColumn, paymodeIdColumn, payModeTable, payModeTableIdColumn, payModeTableNameColumn);
                string detailOrder = canJoinDetails && !string.IsNullOrWhiteSpace(slNoColumn) ? $", sd.{slNoColumn}" : string.Empty;

                string sql = $@"
SELECT
    sm.BillNo,
    sm.BillDate,
    {itemNameExpression} AS ItemName,
    {qtyExpression} AS Qty,
    {sellingPriceExpression} AS SellingPrice,
    {lineAmountExpression} AS LineTotal,
    ISNULL(sm.NetAmount,0) AS BillTotal,
    ISNULL(pay.TotalPaid,0) AS PartiallyPaid,
    ISNULL(sm.NetAmount,0) - ISNULL(pay.TotalPaid,0) AS Balance,
    split.PaymentSplit
FROM SMaster sm
{detailJoin}
INNER JOIN (
    SELECT sp.BillNo, sp.BranchId, sp.CompanyId, sp.FinYearId,
           ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0) AS TotalPaid,
           COUNT(1) AS PaymentRows
    FROM {paymentTable} sp
    GROUP BY sp.BillNo, sp.BranchId, sp.CompanyId, sp.FinYearId
) pay ON pay.BillNo=sm.BillNo AND pay.BranchId=sm.BranchId AND pay.CompanyId=sm.CompanyId AND pay.FinYearId=sm.FinYearId
CROSS APPLY (
    SELECT STUFF((
        SELECT '; ' + x.PaymentMode + ' - ' + CONVERT(varchar(40), CAST(x.Amount AS decimal(18,2)))
        FROM (
            SELECT {paymentNameExpression} AS PaymentMode, ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0) AS Amount
            FROM {paymentTable} sp
            WHERE sp.BillNo=sm.BillNo AND sp.BranchId=sm.BranchId AND sp.CompanyId=sm.CompanyId AND sp.FinYearId=sm.FinYearId
            GROUP BY {paymentNameExpression}
        ) x
        ORDER BY x.PaymentMode
        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS PaymentSplit
) split
WHERE {salesAliasScope}
  AND ISNULL(sm.CancelFlag,0)=0
  AND (pay.PaymentRows>1 OR (pay.TotalPaid>0 AND pay.TotalPaid<ISNULL(sm.NetAmount,0)))
ORDER BY sm.BillDate DESC, sm.BillNo DESC{detailOrder};";

                using (var reader = DataConnection.ExecuteReader(sql, p))
                {
                    table.Load(reader);
                }
            }
            catch
            {
            }

            return table;
        }

        private T Safe<T>(string sql, DynamicParameters p)
        {
            try
            {
                return DataConnection.QueryFirstOrDefault<T>(sql, p);
            }
            catch
            {
                return default(T);
            }
        }

        private void FillPurchaseModeTotals(FinalAnalysisModel model, string purchaseScope, DynamicParameters p)
        {
            model.TotalPurchaseCash = ReadPurchaseModeTotal(purchaseScope, p, "cash");
            model.TotalPurchaseCashCount = ReadPurchaseModeCount(purchaseScope, p, "cash");
            model.TotalPurchaseCredit = ReadPurchaseModeTotal(purchaseScope, p, "credit");
            model.TotalPurchaseCreditCount = ReadPurchaseModeCount(purchaseScope, p, "credit");
            model.TotalPurchaseUpi = ReadPurchaseModeTotal(purchaseScope, p, "upi");
            model.TotalPurchaseUpiCount = ReadPurchaseModeCount(purchaseScope, p, "upi");
            model.TotalPurchaseBank = ReadPurchaseModeTotal(purchaseScope, p, "bank");
            model.TotalPurchaseBankCount = ReadPurchaseModeCount(purchaseScope, p, "bank");
            model.TotalPurchaseCheque = ReadPurchaseModeTotal(purchaseScope, p, "cheque");
            model.TotalPurchaseChequeCount = ReadPurchaseModeCount(purchaseScope, p, "cheque");
        }

        private decimal ReadPurchaseModeTotal(string purchaseScope, DynamicParameters p, string mode)
        {
            return Safe<decimal>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(GrandTotal,0)),0)
FROM PMaster
WHERE " + purchaseScope + @"
  AND ISNULL(CancelFlag,0)=0
  AND " + BuildModePredicate("Paymode", mode) + @";", p);
        }

        private int ReadPurchaseModeCount(string purchaseScope, DynamicParameters p, string mode)
        {
            return Safe<int>(@"
IF OBJECT_ID('PMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM PMaster
WHERE " + purchaseScope + @"
  AND ISNULL(CancelFlag,0)=0
  AND " + BuildModePredicate("Paymode", mode) + @";", p);
        }

        private void FillSalesModeTotals(FinalAnalysisModel model, string salesScope, string salesAliasScope, DynamicParameters p)
        {
            model.TotalSaleCash = ReadSalesModeTotal(salesScope, salesAliasScope, p, "cash");
            model.TotalSaleCashCount = ReadSalesModeCount(salesScope, salesAliasScope, p, "cash");
            model.TotalSaleCredit = ReadSalesModeTotal(salesScope, salesAliasScope, p, "credit");
            model.TotalSaleCreditCount = ReadSalesModeCount(salesScope, salesAliasScope, p, "credit");
            model.TotalSaleUpi = ReadSalesModeTotal(salesScope, salesAliasScope, p, "upi");
            model.TotalSaleUpiCount = ReadSalesModeCount(salesScope, salesAliasScope, p, "upi");
            model.TotalSaleBank = ReadSalesModeTotal(salesScope, salesAliasScope, p, "bank");
            model.TotalSaleBankCount = ReadSalesModeCount(salesScope, salesAliasScope, p, "bank");
            model.TotalSaleCheque = ReadSalesModeTotal(salesScope, salesAliasScope, p, "cheque");
            model.TotalSaleChequeCount = ReadSalesModeCount(salesScope, salesAliasScope, p, "cheque");
            model.TotalSaleCard = ReadSalesModeTotal(salesScope, salesAliasScope, p, "card");
            model.TotalSaleCardCount = ReadSalesModeCount(salesScope, salesAliasScope, p, "card");
        }

        private decimal ReadSalesModeTotal(string salesScope, string salesAliasScope, DynamicParameters p, string mode)
        {
            string paymentTable = GetSalesPaymentTable();
            string amountColumn = string.IsNullOrWhiteSpace(paymentTable) ? null : GetFirstExistingColumn(paymentTable, "Amount", "PaymentAmount", "PaidAmount", "ReceivedAmount");
            string detailNameColumn = string.IsNullOrWhiteSpace(paymentTable) ? null : GetFirstExistingColumn(paymentTable, "PaymodeName", "PayModeName", "PayMode");
            string detailIdColumn = string.IsNullOrWhiteSpace(paymentTable) ? null : GetFirstExistingColumn(paymentTable, "PaymodeId", "PayModeId", "PayModeID");
            string masterNameColumn = GetFirstExistingColumn("SMaster", "PaymodeName", "PayModeName", "PayMode");
            string masterIdColumn = GetFirstExistingColumn("SMaster", "PaymodeId", "PayModeId", "PayModeID");
            string masterPredicate = BuildPaymentModePredicate(masterNameColumn, masterIdColumn, mode);

            if (string.IsNullOrWhiteSpace(paymentTable) || string.IsNullOrWhiteSpace(amountColumn))
            {
                return Safe<decimal>(@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(NetAmount,0)),0)
FROM SMaster
WHERE " + salesScope + @"
  AND ISNULL(CancelFlag,0)=0
  AND " + masterPredicate + @";", p);
            }

            string detailPredicate = BuildPaymentModePredicate(QualifyColumn("sp", detailNameColumn), QualifyColumn("sp", detailIdColumn), mode);
            return Safe<decimal>($@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT
    ISNULL((
        SELECT SUM(ISNULL(sp.{amountColumn},0))
        FROM {paymentTable} sp
        INNER JOIN SMaster sm ON sm.BillNo=sp.BillNo AND sm.BranchId=sp.BranchId AND sm.CompanyId=sp.CompanyId AND sm.FinYearId=sp.FinYearId
        WHERE {salesAliasScope}
          AND ISNULL(sm.CancelFlag,0)=0
          AND {detailPredicate}
    ),0)
    +
    ISNULL((
        SELECT SUM(ISNULL(NetAmount,0))
        FROM SMaster
        WHERE {salesScope}
          AND ISNULL(CancelFlag,0)=0
          AND {masterPredicate}
          AND NOT EXISTS (
              SELECT 1
              FROM {paymentTable} sp
              WHERE sp.BillNo=SMaster.BillNo AND sp.BranchId=SMaster.BranchId AND sp.CompanyId=SMaster.CompanyId AND sp.FinYearId=SMaster.FinYearId
          )
    ),0);", p);
        }

        private int ReadSalesModeCount(string salesScope, string salesAliasScope, DynamicParameters p, string mode)
        {
            string paymentTable = GetSalesPaymentTable();
            string amountColumn = string.IsNullOrWhiteSpace(paymentTable) ? null : GetFirstExistingColumn(paymentTable, "Amount", "PaymentAmount", "PaidAmount", "ReceivedAmount");
            string detailNameColumn = string.IsNullOrWhiteSpace(paymentTable) ? null : GetFirstExistingColumn(paymentTable, "PaymodeName", "PayModeName", "PayMode");
            string detailIdColumn = string.IsNullOrWhiteSpace(paymentTable) ? null : GetFirstExistingColumn(paymentTable, "PaymodeId", "PayModeId", "PayModeID");
            string masterNameColumn = GetFirstExistingColumn("SMaster", "PaymodeName", "PayModeName", "PayMode");
            string masterIdColumn = GetFirstExistingColumn("SMaster", "PaymodeId", "PayModeId", "PayModeID");
            string masterPredicate = BuildPaymentModePredicate(masterNameColumn, masterIdColumn, mode);

            if (string.IsNullOrWhiteSpace(paymentTable) || string.IsNullOrWhiteSpace(amountColumn))
            {
                return Safe<int>(@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM SMaster
WHERE " + salesScope + @"
  AND ISNULL(CancelFlag,0)=0
  AND " + masterPredicate + @";", p);
            }

            string detailPredicate = BuildPaymentModePredicate(QualifyColumn("sp", detailNameColumn), QualifyColumn("sp", detailIdColumn), mode);
            return Safe<int>($@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT 0
ELSE SELECT
    ISNULL((
        SELECT COUNT(DISTINCT sp.BillNo)
        FROM {paymentTable} sp
        INNER JOIN SMaster sm ON sm.BillNo=sp.BillNo AND sm.BranchId=sp.BranchId AND sm.CompanyId=sp.CompanyId AND sm.FinYearId=sp.FinYearId
        WHERE {salesAliasScope}
          AND ISNULL(sm.CancelFlag,0)=0
          AND ISNULL(sp.{amountColumn},0)>0
          AND {detailPredicate}
    ),0)
    +
    ISNULL((
        SELECT COUNT(1)
        FROM SMaster
        WHERE {salesScope}
          AND ISNULL(CancelFlag,0)=0
          AND {masterPredicate}
          AND NOT EXISTS (
              SELECT 1
              FROM {paymentTable} sp
              WHERE sp.BillNo=SMaster.BillNo AND sp.BranchId=SMaster.BranchId AND sp.CompanyId=SMaster.CompanyId AND sp.FinYearId=SMaster.FinYearId
          )
    ),0);", p);
        }

        private void FillVendorPaymentTotals(FinalAnalysisModel model, DynamicParameters p, int userId)
        {
            string masterTable = TableExists("_VendorPaymentMaster") ? "_VendorPaymentMaster" : TableExists("VendorPaymentMaster") ? "VendorPaymentMaster" : null;
            if (string.IsNullOrWhiteSpace(masterTable))
                return;

            string amountColumn = GetFirstExistingColumn(masterTable, "PaymentAmount", "TotalPaymentAmount", "PayableAmount");
            string dateColumn = GetFirstExistingColumn(masterTable, "VoucherDate", "PaymentDate", "CreatedDate");
            string methodColumn = GetFirstExistingColumn(masterTable, "PaymentMethod", "PaymentMode");
            string methodIdColumn = GetFirstExistingColumn(masterTable, "PaymentMethodLedgerId", "PaymentMethodId", "ModeID");
            string userColumn = GetFirstExistingColumn(masterTable, "UserId", "UserID", "CreatedBy");
            string branchColumn = GetFirstExistingColumn(masterTable, "BranchId", "BranchID");
            string companyColumn = GetFirstExistingColumn(masterTable, "CompanyId", "CompanyID");

            if (string.IsNullOrWhiteSpace(amountColumn) || string.IsNullOrWhiteSpace(dateColumn))
                return;

            string scope = "1=1";
            if (!string.IsNullOrWhiteSpace(branchColumn)) scope += $" AND {branchColumn}=@BranchId";
            if (!string.IsNullOrWhiteSpace(companyColumn)) scope += $" AND {companyColumn}=@CompanyId";
            if (!string.IsNullOrWhiteSpace(userColumn) && userId > 0) scope += $" AND {userColumn}=@UserId";
            scope += $" AND {dateColumn}>=@FromDate AND {dateColumn}<@ToDate";

            AddVendorPaymentMode(model, masterTable, amountColumn, methodColumn, methodIdColumn, scope, p, "cash");
            AddVendorPaymentMode(model, masterTable, amountColumn, methodColumn, methodIdColumn, scope, p, "bank");
            AddVendorPaymentMode(model, masterTable, amountColumn, methodColumn, methodIdColumn, scope, p, "upi");
            AddVendorPaymentMode(model, masterTable, amountColumn, methodColumn, methodIdColumn, scope, p, "card");
            AddVendorPaymentMode(model, masterTable, amountColumn, methodColumn, methodIdColumn, scope, p, "cheque");
        }

        private void AddVendorPaymentMode(FinalAnalysisModel model, string tableName, string amountColumn, string methodColumn, string methodIdColumn, string scope, DynamicParameters p, string mode)
        {
            string predicate = BuildPaymentModePredicate(methodColumn, methodIdColumn, mode);
            decimal amount = Safe<decimal>($@"
SELECT ISNULL(SUM(ISNULL({amountColumn},0)),0)
FROM {tableName}
WHERE {scope} AND {predicate};", p);
            int count = Safe<int>($@"
SELECT COUNT(1)
FROM {tableName}
WHERE {scope} AND {predicate};", p);

            switch (mode)
            {
                case "cash":
                    model.TotalPaymentCash += amount;
                    model.TotalPaymentCashCount += count;
                    break;
                case "bank":
                    model.TotalPaymentBank += amount;
                    model.TotalPaymentBankCount += count;
                    break;
                case "upi":
                    model.TotalPaymentUpi += amount;
                    model.TotalPaymentUpiCount += count;
                    break;
                case "card":
                    model.TotalPaymentCard += amount;
                    model.TotalPaymentCardCount += count;
                    break;
                case "cheque":
                    model.TotalPaymentCheque += amount;
                    model.TotalPaymentChequeCount += count;
                    break;
            }
        }

        private void FillPartialPaymentTotals(FinalAnalysisModel model, string salesScope, DynamicParameters p)
        {
            if (!TableExists("SPaymentDetails") && !TableExists("_POS_SPaymentDetails"))
                return;

            string paymentTable = TableExists("SPaymentDetails") ? "SPaymentDetails" : "_POS_SPaymentDetails";
            string amountColumn = GetFirstExistingColumn(paymentTable, "Amount", "PaymentAmount");
            if (string.IsNullOrWhiteSpace(amountColumn))
                return;

            model.PartialPayment = Safe<decimal>($@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(x.PaidAmount),0)
FROM (
    SELECT sp.BillNo, ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0) AS PaidAmount, COUNT(1) AS ModeCount
    FROM {paymentTable} sp
    INNER JOIN SMaster sm ON sm.BillNo=sp.BillNo AND sm.BranchId=sp.BranchId AND sm.CompanyId=sp.CompanyId AND sm.FinYearId=sp.FinYearId
    WHERE {salesScope} AND ISNULL(sm.CancelFlag,0)=0
    GROUP BY sp.BillNo, sm.NetAmount
    HAVING COUNT(1)>1 OR (ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0)>0 AND ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0)<ISNULL(sm.NetAmount,0))
) x;", p);

            model.PartialPaymentCount = Safe<int>($@"
IF OBJECT_ID('SMaster','U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM (
    SELECT sp.BillNo
    FROM {paymentTable} sp
    INNER JOIN SMaster sm ON sm.BillNo=sp.BillNo AND sm.BranchId=sp.BranchId AND sm.CompanyId=sp.CompanyId AND sm.FinYearId=sp.FinYearId
    WHERE {salesScope} AND ISNULL(sm.CancelFlag,0)=0
    GROUP BY sp.BillNo, sm.NetAmount
    HAVING COUNT(1)>1 OR (ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0)>0 AND ISNULL(SUM(ISNULL(sp.{amountColumn},0)),0)<ISNULL(sm.NetAmount,0))
) x;", p);
        }

        private int ReadNonProfitSalesItems(string salesAliasScope, DynamicParameters p)
        {
            string costColumn = GetFirstExistingColumn("SDetails", "Cost", "CostPrice");
            string salePriceColumn = GetFirstExistingColumn("SDetails", "UnitPrice", "Rate", "SellingPrice");
            string itemColumn = GetFirstExistingColumn("SDetails", "ItemId", "ItemCode", "ItemName");

            if (string.IsNullOrWhiteSpace(costColumn) || string.IsNullOrWhiteSpace(salePriceColumn))
                return 0;

            string countExpression = string.IsNullOrWhiteSpace(itemColumn) ? "1" : $"DISTINCT sd.{itemColumn}";
            string condition = BuildNonProfitSaleCondition(costColumn, salePriceColumn);

            return Safe<int>(@"
IF OBJECT_ID('SDetails','U') IS NULL SELECT 0
ELSE SELECT COUNT(" + countExpression + @")
FROM SDetails sd
INNER JOIN SMaster sm ON sm.BillNo=sd.BillNo AND sm.BranchId=sd.BranchId AND sm.CompanyId=sd.CompanyId AND sm.FinYearId=sd.FinYearId
WHERE " + salesAliasScope + @"
  AND ISNULL(sm.CancelFlag,0)=0
  AND " + condition + @";", p);
        }

        private decimal ReadNonProfitSalesAmount(string salesAliasScope, DynamicParameters p)
        {
            string costColumn = GetFirstExistingColumn("SDetails", "Cost", "CostPrice");
            string salePriceColumn = GetFirstExistingColumn("SDetails", "UnitPrice", "Rate", "SellingPrice");
            string qtyColumn = GetFirstExistingColumn("SDetails", "Qty", "Quantity");
            string lineAmountColumn = GetFirstExistingColumn("SDetails", "TotalAmount", "Amount");

            if (string.IsNullOrWhiteSpace(costColumn) || string.IsNullOrWhiteSpace(salePriceColumn))
                return 0;

            string amountExpression = !string.IsNullOrWhiteSpace(lineAmountColumn)
                ? $"ISNULL(sd.{lineAmountColumn},0)"
                : !string.IsNullOrWhiteSpace(qtyColumn)
                    ? $"ISNULL(sd.{salePriceColumn},0) * ISNULL(sd.{qtyColumn},0)"
                    : $"ISNULL(sd.{salePriceColumn},0)";
            string condition = BuildNonProfitSaleCondition(costColumn, salePriceColumn);

            return Safe<decimal>(@"
IF OBJECT_ID('SDetails','U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(" + amountExpression + @"),0)
FROM SDetails sd
INNER JOIN SMaster sm ON sm.BillNo=sd.BillNo AND sm.BranchId=sd.BranchId AND sm.CompanyId=sd.CompanyId AND sm.FinYearId=sd.FinYearId
WHERE " + salesAliasScope + @"
  AND ISNULL(sm.CancelFlag,0)=0
  AND " + condition + @";", p);
        }

        private string BuildNonProfitSaleCondition(string costColumn, string salePriceColumn)
        {
            return $"ISNULL(sd.{costColumn},0)>0 AND ISNULL(sd.{salePriceColumn},0)<=ISNULL(sd.{costColumn},0)";
        }

        private string GetSalesPaymentTable()
        {
            return TableExists("SPaymentDetails") ? "SPaymentDetails" : TableExists("_POS_SPaymentDetails") ? "_POS_SPaymentDetails" : null;
        }

        private string QualifyColumn(string alias, string columnName)
        {
            return string.IsNullOrWhiteSpace(columnName) ? null : alias + "." + columnName;
        }

        private string BuildSalesPaymentNameExpression(string paymentAlias, string paymentNameColumn, string paymentIdColumn, string payModeTable, string payModeIdColumn, string payModeNameColumn)
        {
            string fallback = "'Payment'";
            if (!string.IsNullOrWhiteSpace(paymentIdColumn) &&
                !string.IsNullOrWhiteSpace(payModeTable) &&
                !string.IsNullOrWhiteSpace(payModeIdColumn) &&
                !string.IsNullOrWhiteSpace(payModeNameColumn))
            {
                fallback = $"ISNULL((SELECT TOP 1 pm.{payModeNameColumn} FROM {payModeTable} pm WHERE pm.{payModeIdColumn}={paymentAlias}.{paymentIdColumn}), 'Payment')";
            }

            if (!string.IsNullOrWhiteSpace(paymentNameColumn))
                return $"ISNULL(NULLIF({paymentAlias}.{paymentNameColumn}, ''), {fallback})";

            return fallback;
        }

        private string BuildModePredicate(string columnName, string mode)
        {
            return BuildPaymentModePredicate(columnName, null, mode);
        }

        private string BuildPaymentModePredicate(string nameColumn, string idColumn, string mode)
        {
            string namePredicate = string.IsNullOrWhiteSpace(nameColumn)
                ? "1=0"
                : $"LOWER(REPLACE(ISNULL({nameColumn},''),' ','')) LIKE '%{GetModeToken(mode)}%'";

            string idPredicate = string.IsNullOrWhiteSpace(idColumn)
                ? "1=0"
                : BuildPaymodeIdPredicate(idColumn, mode);

            if (mode == "bank")
            {
                namePredicate = string.IsNullOrWhiteSpace(nameColumn)
                    ? "1=0"
                    : $"(LOWER(REPLACE(ISNULL({nameColumn},''),' ','')) LIKE '%bank%' OR LOWER(REPLACE(ISNULL({nameColumn},''),' ','')) LIKE '%transfer%')";
            }

            return "(" + namePredicate + " OR " + idPredicate + ")";
        }

        private string BuildPaymodeIdPredicate(string idColumn, string mode)
        {
            string tableName = TableExists("PayMode") ? "PayMode" : TableExists("Paymode") ? "Paymode" : null;
            if (string.IsNullOrWhiteSpace(tableName))
                return "1=0";

            string idName = GetFirstExistingColumn(tableName, "PayModeID", "PaymodeID", "PayModeId");
            string modeName = GetFirstExistingColumn(tableName, "PayModeName", "PaymodeName", "Name");
            if (string.IsNullOrWhiteSpace(idName) || string.IsNullOrWhiteSpace(modeName))
                return "1=0";

            string token = GetModeToken(mode);
            string modePredicate = mode == "bank"
                ? $"(LOWER(REPLACE(ISNULL({modeName},''),' ','')) LIKE '%bank%' OR LOWER(REPLACE(ISNULL({modeName},''),' ','')) LIKE '%transfer%')"
                : $"LOWER(REPLACE(ISNULL({modeName},''),' ','')) LIKE '%{token}%'";

            return $"{idColumn} IN (SELECT {idName} FROM {tableName} WHERE {modePredicate})";
        }

        private string GetModeToken(string mode)
        {
            return mode == "upi" ? "upi" : mode;
        }

        private DynamicParameters BuildBaseParams(DateTime? fromDate = null, DateTime? toDate = null, int userId = 0, int counterId = 0)
        {
            var p = new DynamicParameters();
            p.Add("@BranchId", GetContextValue(SessionContext.BranchId, DataBase.BranchId));
            p.Add("@CompanyId", GetContextValue(SessionContext.CompanyId, DataBase.CompanyId));
            p.Add("@FinYearId", GetContextValue(SessionContext.FinYearId, DataBase.FinyearId));
            p.Add("@UserId", userId);
            p.Add("@CounterId", counterId);
            if (fromDate.HasValue) p.Add("@FromDate", fromDate.Value);
            if (toDate.HasValue) p.Add("@ToDate", toDate.Value);
            return p;
        }

        private string BuildScopeFilter(string tableName, string branchColumn, string alias = null, string dateColumn = null, int userId = 0, int counterId = 0)
        {
            string prefix = string.IsNullOrWhiteSpace(alias) ? string.Empty : alias + ".";
            string filter = $"{prefix}{branchColumn}=@BranchId AND {prefix}CompanyId=@CompanyId AND {prefix}FinYearId=@FinYearId";

            if (!string.IsNullOrWhiteSpace(dateColumn) && !string.IsNullOrWhiteSpace(GetFirstExistingColumn(tableName, dateColumn)))
                filter += $" AND {prefix}{dateColumn}>=@FromDate AND {prefix}{dateColumn}<@ToDate";

            string userColumn = GetFirstExistingColumn(tableName, "UserId", "UserID", "CreatedBy");
            if (!string.IsNullOrWhiteSpace(userColumn) && userId > 0)
                filter += $" AND {prefix}{userColumn}=@UserId";

            string counterColumn = GetFirstExistingColumn(tableName, "CounterId", "CounterID");
            if (!string.IsNullOrWhiteSpace(counterColumn) && counterId > 0)
                filter += $" AND {prefix}{counterColumn}=@CounterId";

            return filter;
        }

        private string GetFirstExistingColumn(string tableName, params string[] columnNames)
        {
            try
            {
                const string sql = @"
SELECT TOP 1 c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@TableName, 'U')
  AND c.name IN @ColumnNames
ORDER BY CASE c.name
    WHEN @PreferredColumn THEN 0
    ELSE 1
END;";

                return DataConnection.QueryFirstOrDefault<string>(
                    sql,
                    new { TableName = tableName, ColumnNames = columnNames, PreferredColumn = columnNames[0] });
            }
            catch
            {
                return null;
            }
        }

        private int GetContextValue(int sessionValue, string legacyValue)
        {
            if (sessionValue > 0)
                return sessionValue;

            int parsed;
            return int.TryParse(legacyValue, out parsed) ? parsed : 0;
        }

        private DataTable ReadLookupFromTable(string tableName, string[] idColumns, string[] nameColumns, string[] branchColumns)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string idColumn = GetFirstExistingColumn(tableName, idColumns);
                string nameColumn = GetFirstExistingColumn(tableName, nameColumns);
                if (string.IsNullOrWhiteSpace(idColumn) || string.IsNullOrWhiteSpace(nameColumn))
                    return table;

                string branchColumn = GetFirstExistingColumn(tableName, branchColumns);
                string branchFilter = string.IsNullOrWhiteSpace(branchColumn) ? string.Empty : $" WHERE ({branchColumn}=@BranchId OR @BranchId=0)";
                string sql = $@"
SELECT CAST({idColumn} AS int) AS Id, CAST({nameColumn} AS nvarchar(150)) AS Name
FROM {tableName}
{branchFilter}
ORDER BY {nameColumn};";

                var rows = DataConnection.Query(sql, BuildBaseParams()).ToList();
                foreach (var row in rows)
                {
                    table.Rows.Add(Convert.ToInt32(row.Id), Convert.ToString(row.Name));
                }
            }
            catch
            {
            }

            return table;
        }

        private string GetUserName(int userId)
        {
            DataRow row = GetUsers().AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == userId);
            return row == null ? "User " + userId : Convert.ToString(row["Name"]);
        }

        private string GetCounterName(int counterId)
        {
            DataRow row = GetCounters().AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == counterId);
            return row == null ? "Counter " + counterId : Convert.ToString(row["Name"]);
        }

        private DataTable BuildDetailTable(FinalAnalysisModel model)
        {
            DataTable table = new DataTable();
            table.Columns.Add("BusinessMetric", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Count", typeof(int));
            table.Columns.Add("FromDate", typeof(DateTime));
            table.Columns.Add("ToDate", typeof(DateTime));
            table.Columns.Add("User", typeof(string));
            table.Columns.Add("Counter", typeof(string));
            table.Columns.Add("Category", typeof(string));

            AddAmount(table, model, "Total Purchase", model.TotalPurchase, "currency");
            table.Rows[table.Rows.Count - 1]["Count"] = model.TotalPurchaseCount;
            AddAmount(table, model, "Total Payment (Cash)", model.TotalPaymentCash, "currency", model.TotalPaymentCashCount);
            AddAmount(table, model, "Total Payment (Bank)", model.TotalPaymentBank, "currency", model.TotalPaymentBankCount);
            AddAmount(table, model, "Total Payment (UPI)", model.TotalPaymentUpi, "currency", model.TotalPaymentUpiCount);
            AddAmount(table, model, "Total Payment (Card)", model.TotalPaymentCard, "currency", model.TotalPaymentCardCount);
            AddAmount(table, model, "Total Payment (Cheque)", model.TotalPaymentCheque, "currency", model.TotalPaymentChequeCount);
            AddAmount(table, model, "Total Outstanding (Vendor)", model.TotalOutstandingVendor, "warn", model.TotalOutstandingVendorCount);
            AddAmount(table, model, "Total Cost", model.TotalCost, "currency", model.TotalCostCount);
            AddAmount(table, model, "Total Sale", model.TotalSale, "currency", model.TotalSaleCount);
            AddAmount(table, model, "Total Profit", model.TotalProfit, model.TotalProfit >= 0 ? "profit" : "loss", model.TotalProfitCount);
            AddAmount(table, model, "Partial Payment", model.PartialPayment, "warn", model.PartialPaymentCount);
            AddAmount(table, model, "Purchase Return", model.PurchaseReturn, "currency", model.PurchaseReturnCount);
            AddAmount(table, model, "Sales Return", model.SalesReturn, "currency", model.SalesReturnCount);
            AddAmount(table, model, "Non-Profit Items", model.NonProfitAmount, "warn", model.NonProfitItems);
            AddCount(table, model, "Excess Stock Items", model.ExcessStockItems, "warn");
            AddCount(table, model, "Out of Stock Items", model.OutOfStockItems, "loss");
            AddCount(table, model, "Discontinued Items", model.DiscontinuedItems, "neutral");

            return table;
        }

        private void AddAmount(DataTable table, FinalAnalysisModel model, string metric, decimal amount, string category)
        {
            table.Rows.Add(metric, amount, DBNull.Value, model.FromDate, model.ToDate, model.UserName, model.CounterName, category);
        }

        private void AddAmount(DataTable table, FinalAnalysisModel model, string metric, decimal amount, string category, int count)
        {
            table.Rows.Add(metric, amount, count, model.FromDate, model.ToDate, model.UserName, model.CounterName, category);
        }

        private void AddCount(DataTable table, FinalAnalysisModel model, string metric, int count, string category)
        {
            table.Rows.Add(metric, DBNull.Value, count, model.FromDate, model.ToDate, model.UserName, model.CounterName, category);
        }

        private bool TableExists(string tableName)
        {
            try
            {
                return DataConnection.QueryFirstOrDefault<int>(
                    "SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NULL THEN 0 ELSE 1 END;",
                    new { TableName = tableName }) == 1;
            }
            catch
            {
                return false;
            }
        }
    }

    public class FinalAnalysisModel
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int UserId { get; set; }
        public int CounterId { get; set; }
        public string UserName { get; set; }
        public string CounterName { get; set; }
        public string BranchName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public decimal TotalPurchase { get; set; }
        public int TotalPurchaseCount { get; set; }
        public decimal TotalPurchaseCash { get; set; }
        public int TotalPurchaseCashCount { get; set; }
        public decimal TotalPurchaseCredit { get; set; }
        public int TotalPurchaseCreditCount { get; set; }
        public decimal TotalPurchaseUpi { get; set; }
        public int TotalPurchaseUpiCount { get; set; }
        public decimal TotalPurchaseBank { get; set; }
        public int TotalPurchaseBankCount { get; set; }
        public decimal TotalPurchaseCheque { get; set; }
        public int TotalPurchaseChequeCount { get; set; }
        public decimal TotalPaymentCash { get; set; }
        public int TotalPaymentCashCount { get; set; }
        public decimal TotalPaymentBank { get; set; }
        public int TotalPaymentBankCount { get; set; }
        public decimal TotalPaymentUpi { get; set; }
        public int TotalPaymentUpiCount { get; set; }
        public decimal TotalPaymentCard { get; set; }
        public int TotalPaymentCardCount { get; set; }
        public decimal TotalPaymentCheque { get; set; }
        public int TotalPaymentChequeCount { get; set; }
        public decimal TotalOutstandingVendor { get; set; }
        public int TotalOutstandingVendorCount { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalCostCount { get; set; }
        public decimal TotalSale { get; set; }
        public int TotalSaleCount { get; set; }
        public decimal TotalSaleCash { get; set; }
        public int TotalSaleCashCount { get; set; }
        public decimal TotalSaleCredit { get; set; }
        public int TotalSaleCreditCount { get; set; }
        public decimal TotalSaleUpi { get; set; }
        public int TotalSaleUpiCount { get; set; }
        public decimal TotalSaleBank { get; set; }
        public int TotalSaleBankCount { get; set; }
        public decimal TotalSaleCheque { get; set; }
        public int TotalSaleChequeCount { get; set; }
        public decimal TotalSaleCard { get; set; }
        public int TotalSaleCardCount { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalProfitCount { get; set; }
        public decimal PartialPayment { get; set; }
        public int PartialPaymentCount { get; set; }
        public decimal PurchaseReturn { get; set; }
        public int PurchaseReturnCount { get; set; }
        public decimal SalesReturn { get; set; }
        public int SalesReturnCount { get; set; }
        public decimal NonProfitAmount { get; set; }
        public int NonProfitItems { get; set; }
        public int ExcessStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int DiscontinuedItems { get; set; }
        public DataTable DetailTable { get; set; }
    }
}
