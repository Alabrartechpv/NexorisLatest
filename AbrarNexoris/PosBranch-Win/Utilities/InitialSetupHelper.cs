using System;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using Repository;
using Repository.MasterRepositry;

namespace PosBranch_Win.Utilities
{
    public static class InitialSetupHelper
    {
        /// <summary>
        /// Checks if the Branches table is empty (which indicates a cleared database).
        /// </summary>
        public static bool IsDatabaseEmpty()
        {
            BaseRepostitory repo = null;
            try
            {
                repo = new BaseRepostitory();
                if (repo.DataConnection == null)
                    return false;

                if (repo.DataConnection.State != ConnectionState.Open)
                    repo.DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)repo.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds == null || ds.Tables.Count == 0 || ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if database is empty: {ex.Message}");
                if (ex is SqlException sqlEx && sqlEx.Number == 208)
                {
                    return true;
                }
                return false;
            }
            finally
            {
                if (repo != null)
                {
                    repo.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs initial seeding of Company, Financial Year, User Levels, Admin User, and Branch.
        /// </summary>
        public static bool InitializeDatabase(
            string companyName,
            string companyCaption,
            string branchName,
            string branchAddress,
            string branchPhone,
            string adminPassword)
        {
            BaseRepostitory repo = null;
            SqlTransaction transaction = null;

            try
            {
                repo = new BaseRepostitory();
                SqlConnection conn = (SqlConnection)repo.DataConnection;

                int companyId = 1;
                EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();
                string encryptedPassword = enc.Encrypt(adminPassword, true);

                // Call _POS_Initialsetup Stored Procedure for clean initial database seeding
                using (SqlCommand cmdInit = new SqlCommand(STOREDPROCEDURE.POS_Initialsetup, conn))
                {
                    cmdInit.CommandType = CommandType.StoredProcedure;
                    cmdInit.Parameters.AddWithValue("@CompanyName", companyName.Trim());
                    cmdInit.Parameters.AddWithValue("@CompanyCaption", string.IsNullOrWhiteSpace(companyCaption) ? "Nexoris Retail" : companyCaption.Trim());
                    cmdInit.Parameters.AddWithValue("@BranchName", branchName.Trim());
                    cmdInit.Parameters.AddWithValue("@BranchAddress", string.IsNullOrWhiteSpace(branchAddress) ? "Main Branch Office" : branchAddress.Trim());
                    cmdInit.Parameters.AddWithValue("@BranchPhone", string.IsNullOrWhiteSpace(branchPhone) ? "123456789" : branchPhone.Trim());
                    cmdInit.Parameters.AddWithValue("@AdminPassword", encryptedPassword);
                    cmdInit.ExecuteNonQuery();
                }

                // 5. Call POS_Branch CREATE SP (creates branch, 29 groups, default ledgers, and TrackTrans record)
                object spResult;
                using (SqlCommand cmdBranch = new SqlCommand(STOREDPROCEDURE.POS_Branch, conn))
                {
                    cmdBranch.CommandType = CommandType.StoredProcedure;
                    cmdBranch.Parameters.AddWithValue("@BranchName", branchName.Trim());
                    cmdBranch.Parameters.AddWithValue("@IsDelete", 0);
                    cmdBranch.Parameters.AddWithValue("@CompanyId", companyId);
                    cmdBranch.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(branchAddress) ? "Main Branch Office" : branchAddress.Trim());
                    cmdBranch.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(branchPhone) ? "123456789" : branchPhone.Trim());
                    cmdBranch.Parameters.AddWithValue("@FinYearId", 1);
                    cmdBranch.Parameters.AddWithValue("@IsECommerceAvailable", 0);
                    cmdBranch.Parameters.AddWithValue("@_Operation", "CREATE");

                    spResult = cmdBranch.ExecuteScalar();
                    System.Diagnostics.Debug.WriteLine($"POS_Branch CREATE result: {spResult}");
                }

                int branchId = ToInt(spResult);
                if (branchId <= 0)
                    branchId = ResolveCreatedBranchId(conn, branchName.Trim());

                EnsureReturnLedgers(conn, companyId, branchId);

                // Seed default Item Types and Paymodes for a fresh install
                EnsureItemTypeSeedData();
                PaymodeRepository.EnsurePaymodeSeedData(conn);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during InitialSetupHelper.InitializeDatabase: {ex.Message}");
                if (transaction != null && transaction.Connection != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch { }
                }
                throw;
            }
            finally
            {
                if (repo != null)
                {
                    repo.Dispose();
                }
            }
        }

        private static int ResolveCreatedBranchId(SqlConnection conn, string branchName)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchName", branchName);
                cmd.Parameters.AddWithValue("@_Operation", "Search");

                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    adapt.Fill(ds);
                    if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0 || !ds.Tables[0].Columns.Contains("Id"))
                        return 0;

                    return ToInt(ds.Tables[0].Rows[0]["Id"]);
                }
            }
        }

        private static void EnsureReturnLedgers(SqlConnection conn, int companyId, int branchId)
        {
            if (branchId <= 0)
                return;

            EnsureReturnLedger(conn, companyId, branchId, DefaultLedgers.SALESRETURN, (int)AccountGroup.SALES_ACCOUNT);
            EnsureReturnLedger(conn, companyId, branchId, DefaultLedgers.PURCHASERETURN, (int)AccountGroup.PURCHASE_ACCOUNT);
        }

        private static void EnsureReturnLedger(SqlConnection conn, int companyId, int branchId, string ledgerName, int groupId)
        {
            if (GetLedgerId(conn, ledgerName, groupId, branchId) > 0)
                return;

            int ledgerId = GetNextLedgerId(conn);
            if (ledgerId <= 0)
                return;

            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                cmd.Parameters.AddWithValue("@BranchID", branchId);
                cmd.Parameters.AddWithValue("@LedgerID", ledgerId);
                cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                cmd.Parameters.AddWithValue("@Alias", DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", DBNull.Value);
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@OpnDebit", 0);
                cmd.Parameters.AddWithValue("@OpnCredit", 0);
                cmd.Parameters.AddWithValue("@ProvideBankDetails", false);
                cmd.Parameters.AddWithValue("@GstApplicable", false);
                cmd.Parameters.AddWithValue("@VatApplicable", false);
                cmd.Parameters.AddWithValue("@InventoryValuesAffected", false);
                cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", false);
                cmd.Parameters.AddWithValue("@PriceLevelApplicable", false);
                cmd.ExecuteScalar();
            }
        }

        private static int GetLedgerId(SqlConnection conn, string ledgerName, int groupId, int branchId)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._4GetLedgerIdByLedgerNameAndGroupId, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                cmd.Parameters.AddWithValue("@GroupId", groupId);
                cmd.Parameters.AddWithValue("@BranchId", branchId);

                return ToInt(cmd.ExecuteScalar());
            }
        }

        private static int GetNextLedgerId(SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@_Operation", "GETNEXTID");

                return ToInt(cmd.ExecuteScalar());
            }
        }

        public static void EnsureItemTypeSeedData()
        {
            BaseRepostitory repo = null;
            try
            {
                repo = new BaseRepostitory();
                if (repo.DataConnection == null) return;
                if (repo.DataConnection.State != ConnectionState.Open)
                    repo.DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemType, (SqlConnection)repo.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SEED");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // SEED operation may not exist on older SP versions — log but do not block startup
                System.Diagnostics.Debug.WriteLine($"EnsureItemTypeSeedData warning: {ex.Message}");
            }
            finally
            {
                if (repo != null) repo.Dispose();
            }
        }

        private static int ToInt(object value)
        {
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }
    }
}
