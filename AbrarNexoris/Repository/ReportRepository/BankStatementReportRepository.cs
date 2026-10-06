using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class BankStatementReportRepository : BaseRepostitory
    {
        private static bool _procedureChecked = false;

        private void EnsureBankStatementProcedure()
        {
            if (_procedureChecked) return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.POS_BankStatementReport', 'P') IS NOT NULL DROP PROCEDURE dbo.POS_BankStatementReport;
", (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
CREATE PROCEDURE [dbo].[POS_BankStatementReport]
(
    @CompanyId   INT,
    @BranchId    INT,
    @FinYearId   INT,
    @FromDate    DATETIME,
    @ToDate      DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Sales (Money In)
    SELECT
        CAST(sm.BillDate AS DATE)                           AS TransactionDate,
        'Sales'                                             AS TransactionType,
        ISNULL(sm.CustomerName, 'Walk-in Customer')         AS PartyName,
        CAST(sm.BillNo AS VARCHAR(30))                      AS BillVoucherNo,
        ISNULL(sp.Amount, 0)                                AS MoneyIn,
        CAST(0.00 AS DECIMAL(18,2))                        AS MoneyOut,
        ISNULL(pm.PayModeName, 'Bank')                      AS PaymentMethod,
        ISNULL(NULLIF(sp.Reference, ''), ISNULL(sm.PaymentReference, '')) AS Reference
    FROM SPaymentDetails sp
    INNER JOIN SMaster sm
        ON sp.BillNo = sm.BillNo
       AND sp.CompanyId = sm.CompanyId
       AND sp.BranchId  = sm.BranchId
       AND sp.FinYearId = sm.FinYearId
    LEFT JOIN PayMode pm ON sp.PaymodeId = pm.PayModeID
    WHERE sp.CompanyId  = @CompanyId
      AND sp.BranchId   = @BranchId
      AND (@FinYearId <= 0 OR sp.FinYearId = @FinYearId OR @FinYearId IS NULL)
      AND CAST(sm.BillDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND ISNULL(sp.PaymodeId, 0) NOT IN (1, 2)

    UNION ALL

    -- 2. Purchase (Money Out)
    SELECT
        CAST(p.PurchaseDate AS DATE)                        AS TransactionDate,
        'Purchase'                                          AS TransactionType,
        ISNULL(p.VendorName, '')                            AS PartyName,
        'GRN-' + CAST(p.PurchaseNo AS VARCHAR(30))          AS BillVoucherNo,
        CAST(0.00 AS DECIMAL(18,2))                        AS MoneyIn,
        ISNULL(p.GrandTotal, 0)                             AS MoneyOut,
        ISNULL(p.Paymode, 'Bank')                           AS PaymentMethod,
        ISNULL(NULLIF(p.InvoiceNo, ''), ISNULL(p.Remarks, '')) AS Reference
    FROM PMaster p
    WHERE p.CompanyId  = @CompanyId
      AND p.BranchId   = @BranchId
      AND (@FinYearId <= 0 OR p.FinYearId = @FinYearId OR @FinYearId IS NULL)
      AND CAST(p.PurchaseDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(p.CancelFlag, 0) = 0
      AND ISNULL(p.PaymodeID, 0) NOT IN (1, 2)

    UNION ALL

    -- 3. Vendor Payment (Money Out)
    SELECT
        CAST(vp.VoucherDate AS DATE)                        AS TransactionDate,
        'Vendor Payment'                                    AS TransactionType,
        ISNULL(lm.LedgerName, '')                           AS PartyName,
        'VP-' + CAST(vp.VoucherId AS VARCHAR(30))           AS BillVoucherNo,
        CAST(0.00 AS DECIMAL(18,2))                        AS MoneyIn,
        ISNULL(vp.PaymentAmount, 0)                         AS MoneyOut,
        ISNULL(pm.PayModeName, 'Bank Transfer')             AS PaymentMethod,
        ISNULL(vp.Narration, '')                            AS Reference
    FROM VendorPaymentMaster vp
    LEFT JOIN LedgerMaster lm ON vp.VendorLedgerId = lm.LedgerID AND lm.BranchID = vp.BranchId
    LEFT JOIN PayMode pm ON vp.PaymentMethodLedgerId = pm.PayModeID
    WHERE vp.CompanyId  = @CompanyId
      AND vp.BranchId   = @BranchId
      AND CAST(vp.VoucherDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(vp.CancelFlag, 0) = 0
      AND ISNULL(vp.PaymentMethodLedgerId, 0) NOT IN (1, 2)

    UNION ALL

    -- 4. Customer Receipt (Money In)
    SELECT
        CAST(cr.VoucherDate AS DATE)                        AS TransactionDate,
        'Customer Receipt'                                  AS TransactionType,
        ISNULL(lm.LedgerName, '')                           AS PartyName,
        'CR-' + CAST(cr.VoucherId AS VARCHAR(30))           AS BillVoucherNo,
        ISNULL(cr.ReceiptAmount, 0)                         AS MoneyIn,
        CAST(0.00 AS DECIMAL(18,2))                        AS MoneyOut,
        ISNULL(pm.PayModeName, 'Bank Transfer')             AS PaymentMethod,
        ISNULL(cr.Narration, '')                            AS Reference
    FROM CustomerReceiptMaster cr
    LEFT JOIN LedgerMaster lm ON cr.CustomerLedgerId = lm.LedgerID AND lm.BranchID = cr.BranchId
    LEFT JOIN PayMode pm ON cr.PaymentMethodLedgerId = pm.PayModeID
    WHERE cr.CompanyId  = @CompanyId
      AND cr.BranchId   = @BranchId
      AND CAST(cr.VoucherDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(cr.CancelFlag, 0) = 0
      AND ISNULL(cr.PaymentMethodLedgerId, 0) NOT IN (1, 2)

    UNION ALL

    -- 5. General Receipts, Contra Deposits & Direct Bank Income (Money In from Vouchers)
    SELECT
        CAST(v.VoucherDate AS DATE)                         AS TransactionDate,
        CASE 
            WHEN v.VoucherType = 'GENREC' THEN 'General Receipt'
            WHEN v.VoucherType = 'Contra' THEN 'Bank Contra'
            WHEN v.VoucherType = 'Receipt' THEN 'Receipt'
            WHEN v.VoucherType = 'Credit Note' THEN 'Credit Note'
            WHEN v.VoucherType = 'Debit Note' THEN 'Debit Note'
            ELSE v.VoucherType 
        END                                                 AS TransactionType,
        ISNULL((
            SELECT TOP 1 l2.LedgerName 
            FROM Vouchers v2 
            INNER JOIN LedgerMaster l2 ON v2.LedgerID = l2.LedgerID 
            WHERE v2.VoucherID = v.VoucherID 
              AND v2.VoucherType = v.VoucherType 
              AND v2.LedgerID <> v.LedgerID
        ), 'Bank Account')                                  AS PartyName,
        ISNULL(NULLIF(v.VoucherNumber, ''), 'VR-' + CAST(v.VoucherID AS VARCHAR(30))) AS BillVoucherNo,
        ISNULL(v.Debit, 0)                                  AS MoneyIn,
        CAST(0.00 AS DECIMAL(18,2))                        AS MoneyOut,
        ISNULL(NULLIF(v.Mode, ''), 'Bank Transfer')         AS PaymentMethod,
        ISNULL(v.Narration, '')                             AS Reference
    FROM Vouchers v
    WHERE v.CompanyID = @CompanyId
      AND v.BranchID = @BranchId
      AND (@FinYearId <= 0 OR v.FinYearID = @FinYearId OR @FinYearId IS NULL)
      AND v.LedgerID IN (
          SELECT lm.LedgerID 
          FROM LedgerMaster lm 
          LEFT JOIN AccountGroupMaster ag ON lm.GroupID = ag.GroupID 
          WHERE lm.GroupID IN (15, 25) 
             OR ag.GroupUnder LIKE '%/15/%' 
             OR ag.GroupUnder LIKE '%/25/%' 
             OR ag.GroupName LIKE '%Bank%'
      )
      AND CAST(v.VoucherDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(v.CancelFlag, 0) = 0
      AND ISNULL(v.Debit, 0) > 0
      AND v.VoucherType NOT IN ('Sales', 'CUSTRCPT', 'CustomerReceipt', 'ShiftClosing')

    UNION ALL

    -- 6. General Payments, Contra Withdrawals & Direct Bank Expenses (Money Out from Vouchers)
    SELECT
        CAST(v.VoucherDate AS DATE)                         AS TransactionDate,
        CASE 
            WHEN v.VoucherType = 'GENPAY' THEN 'General Payment'
            WHEN v.VoucherType = 'Contra' THEN 'Bank Contra'
            WHEN v.VoucherType = 'Payment' THEN 'Payment'
            WHEN v.VoucherType = 'Credit Note' THEN 'Credit Note'
            WHEN v.VoucherType = 'Debit Note' THEN 'Debit Note'
            ELSE v.VoucherType 
        END                                                 AS TransactionType,
        ISNULL((
            SELECT TOP 1 l2.LedgerName 
            FROM Vouchers v2 
            INNER JOIN LedgerMaster l2 ON v2.LedgerID = l2.LedgerID 
            WHERE v2.VoucherID = v.VoucherID 
              AND v2.VoucherType = v.VoucherType 
              AND v2.LedgerID <> v.LedgerID
        ), 'Bank Account')                                  AS PartyName,
        ISNULL(NULLIF(v.VoucherNumber, ''), 'VP-' + CAST(v.VoucherID AS VARCHAR(30))) AS BillVoucherNo,
        CAST(0.00 AS DECIMAL(18,2))                        AS MoneyIn,
        ISNULL(v.Credit, 0)                                 AS MoneyOut,
        ISNULL(NULLIF(v.Mode, ''), 'Bank Transfer')         AS PaymentMethod,
        ISNULL(v.Narration, '')                             AS Reference
    FROM Vouchers v
    WHERE v.CompanyID = @CompanyId
      AND v.BranchID = @BranchId
      AND (@FinYearId <= 0 OR v.FinYearID = @FinYearId OR @FinYearId IS NULL)
      AND v.LedgerID IN (
          SELECT lm.LedgerID 
          FROM LedgerMaster lm 
          LEFT JOIN AccountGroupMaster ag ON lm.GroupID = ag.GroupID 
          WHERE lm.GroupID IN (15, 25) 
             OR ag.GroupUnder LIKE '%/15/%' 
             OR ag.GroupUnder LIKE '%/25/%' 
             OR ag.GroupName LIKE '%Bank%'
      )
      AND CAST(v.VoucherDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(v.CancelFlag, 0) = 0
      AND ISNULL(v.Credit, 0) > 0
      AND v.VoucherType NOT IN ('Purchase', 'VENDPAY', 'VendorPayment', 'ShiftClosing')

    ORDER BY TransactionDate, BillVoucherNo;
END
", (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                _procedureChecked = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deploying Bank Statement procedure: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all bank-related transactions (Sales, Purchase, Vendor Payment, Customer Receipt, General Receipt, General Payment, Contra)
        /// for the specified date range.
        /// </summary>
        public BankStatementReportModel GetBankStatementReport(DateTime fromDate, DateTime toDate)
        {
            var report = new BankStatementReportModel();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                EnsureBankStatementProcedure();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_BankStatementReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
                    int branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
                    int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        decimal totalIn = 0, totalOut = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            decimal moneyIn = row["MoneyIn"] != DBNull.Value ? Convert.ToDecimal(row["MoneyIn"]) : 0;
                            decimal moneyOut = row["MoneyOut"] != DBNull.Value ? Convert.ToDecimal(row["MoneyOut"]) : 0;

                            report.Transactions.Add(new BankStatementTransaction
                            {
                                TransactionDate = row["TransactionDate"] != DBNull.Value ? Convert.ToDateTime(row["TransactionDate"]) : DateTime.MinValue,
                                TransactionType = row["TransactionType"]?.ToString() ?? "",
                                PartyName = row["PartyName"]?.ToString() ?? "",
                                BillVoucherNo = row["BillVoucherNo"]?.ToString() ?? "",
                                MoneyIn = moneyIn,
                                MoneyOut = moneyOut,
                                PaymentMethod = row["PaymentMethod"]?.ToString() ?? "",
                                Reference = row["Reference"]?.ToString() ?? ""
                            });

                            totalIn += moneyIn;
                            totalOut += moneyOut;
                        }

                        report.Summary = new BankStatementSummary
                        {
                            TotalMoneyIn = totalIn,
                            TotalMoneyOut = totalOut,
                            NetAmount = totalIn - totalOut
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Bank Statement Report: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return report;
        }
    }
}
