using Dapper;
using ModelClass;
using ModelClass.Accounts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.Accounts
{
    public class ManualPartyBalanceRepository : BaseRepostitory
    {
        public ManualPartyBalanceRepository()
        {
            EnsureSchema();
        }

        public List<ManualPartyBalanceEntry> GetEntries(
            string partyType = null,
            string balanceType = null,
            string searchText = null,
            bool openOnly = false,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                return conn.Query<ManualPartyBalanceEntry>(
                    "_ManualPartyBalanceMaster",
                    new
                    {
                        CompanyId = SessionContext.CompanyId,
                        BranchId = SessionContext.BranchId,
                        PartyType = string.IsNullOrWhiteSpace(partyType) ? null : partyType,
                        BalanceType = string.IsNullOrWhiteSpace(balanceType) ? null : balanceType,
                        SearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim(),
                        FromDate = fromDate?.Date,
                        ToDate = toDate?.Date,
                        OpenOnly = openOnly,
                        _Operation = "GETLIST"
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public ManualPartyBalanceEntry GetEntryById(int id)
        {
            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                return conn.QueryFirstOrDefault<ManualPartyBalanceEntry>(
                    "_ManualPartyBalanceMaster",
                    new
                    {
                        Id = id,
                        CompanyId = SessionContext.CompanyId,
                        BranchId = SessionContext.BranchId,
                        _Operation = "GETBYID"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public int SaveEntry(ManualPartyBalanceEntry entry)
        {
            ValidateEntry(entry);

            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                var param = new DynamicParameters();
                param.Add("@Id", entry.Id);
                param.Add("@CompanyId", SessionContext.CompanyId);
                param.Add("@BranchId", SessionContext.BranchId);
                param.Add("@FinYearId", SessionContext.FinYearId);
                param.Add("@PartyType", entry.PartyType.Trim());
                param.Add("@PartyName", entry.PartyName.Trim());
                param.Add("@BalanceType", entry.BalanceType.Trim());
                param.Add("@Amount", entry.Amount);
                param.Add("@EntryDate", entry.EntryDate);
                param.Add("@Remarks", NormalizeRemarks(entry.Remarks));
                param.Add("@UserId", SessionContext.UserId);
                param.Add("@_Operation", entry.Id <= 0 ? "CREATE" : "UPDATE");

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "_ManualPartyBalanceMaster",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                if (result != null && result.Id != null)
                {
                    return (int)result.Id;
                }
                return entry.Id;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public void DeleteEntry(int id)
        {
            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                conn.Execute(
                    "_ManualPartyBalanceMaster",
                    new
                    {
                        Id = id,
                        CompanyId = SessionContext.CompanyId,
                        BranchId = SessionContext.BranchId,
                        UserId = SessionContext.UserId,
                        _Operation = "DELETE"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public List<ManualPartyBalanceSettlement> GetSettlements(int entryId)
        {
            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                return conn.Query<ManualPartyBalanceSettlement>(
                    "_ManualPartyBalanceSettlement",
                    new
                    {
                        ManualPartyBalanceId = entryId,
                        _Operation = "GETBYENTRYID"
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public void AddSettlement(ManualPartyBalanceSettlement settlement)
        {
            if (settlement == null)
                throw new ArgumentNullException(nameof(settlement));
            if (settlement.ManualPartyBalanceId <= 0)
                throw new InvalidOperationException("Select a balance entry before adding a settlement.");
            if (settlement.SettlementAmount <= 0)
                throw new InvalidOperationException("Settlement amount must be greater than zero.");

            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                conn.Execute(
                    "_ManualPartyBalanceSettlement",
                    new
                    {
                        ManualPartyBalanceId = settlement.ManualPartyBalanceId,
                        SettlementAmount = settlement.SettlementAmount,
                        SettlementDate = settlement.SettlementDate,
                        Remarks = NormalizeRemarks(settlement.Remarks),
                        UserId = SessionContext.UserId,
                        _Operation = "CREATE"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public void DeleteSettlement(int settlementId)
        {
            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                conn.Execute(
                    "_ManualPartyBalanceSettlement",
                    new
                    {
                        Id = settlementId,
                        UserId = SessionContext.UserId,
                        _Operation = "DELETE"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private void EnsureSchema()
        {
            SqlConnection conn = (SqlConnection)DataConnection;
            try
            {
                EnsureConnectionOpen(conn);

                const string sql = @"
IF OBJECT_ID('dbo.ManualPartyBalance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ManualPartyBalance
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CompanyId INT NOT NULL,
        BranchId INT NOT NULL,
        FinYearId INT NOT NULL,
        PartyType NVARCHAR(20) NOT NULL,
        PartyName NVARCHAR(200) NOT NULL,
        BalanceType NVARCHAR(20) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        EntryDate DATETIME NOT NULL,
        Remarks NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT ('Open'),
        CreatedBy INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT (GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL,
        IsDeleted BIT NOT NULL DEFAULT ((0))
    );

    CREATE INDEX IX_ManualPartyBalance_BranchDate
        ON dbo.ManualPartyBalance(CompanyId, BranchId, EntryDate DESC);
END;

IF OBJECT_ID('dbo.ManualPartyBalanceSettlement', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ManualPartyBalanceSettlement
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ManualPartyBalanceId INT NOT NULL,
        SettlementAmount DECIMAL(18,2) NOT NULL,
        SettlementDate DATETIME NOT NULL,
        Remarks NVARCHAR(500) NULL,
        CreatedBy INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT (GETDATE()),
        IsDeleted BIT NOT NULL DEFAULT ((0)),
        CONSTRAINT FK_ManualPartyBalanceSettlement_ManualPartyBalance
            FOREIGN KEY (ManualPartyBalanceId)
            REFERENCES dbo.ManualPartyBalance(Id)
    );

    CREATE INDEX IX_ManualPartyBalanceSettlement_Master
        ON dbo.ManualPartyBalanceSettlement(ManualPartyBalanceId, SettlementDate DESC);
END;";

                conn.Execute(sql);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private static void EnsureConnectionOpen(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }

        private static string NormalizeRemarks(string remarks)
        {
            return string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
        }

        private static void ValidateEntry(ManualPartyBalanceEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            if (string.IsNullOrWhiteSpace(entry.PartyType))
                throw new InvalidOperationException("Party type is required.");
            if (string.IsNullOrWhiteSpace(entry.PartyName))
                throw new InvalidOperationException("Party name is required.");
            if (string.IsNullOrWhiteSpace(entry.BalanceType))
                throw new InvalidOperationException("Balance type is required.");
            if (entry.Amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");
        }
    }
}