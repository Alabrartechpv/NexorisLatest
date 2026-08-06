using Dapper;
using ModelClass;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.MasterRepositry
{
   public class BranchRepository:BaseRepostitory
    {
        public string SaveBranch(Branch branch)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Branch> listbranch = DataConnection.Query<Branch>(STOREDPROCEDURE.POS_Branch, branch, trans,
                    commandType: CommandType.StoredProcedure).ToList<Branch>();
                if (string.Equals(branch._Operation, "CREATE", StringComparison.OrdinalIgnoreCase))
                {
                    int branchId = ResolveCreatedBranchId(branch, listbranch);
                    EnsureReturnLedgers(branch.CompanyId, branchId, trans);
                }
                if (listbranch.Count > 0)
                {

                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return "Success";
        }

        private int ResolveCreatedBranchId(Branch branch, List<Branch> createdBranches)
        {
            if (branch.Id > 0)
                return branch.Id;

            if (createdBranches != null && createdBranches.Count > 0)
                return createdBranches[0].Id;

            return 0;
        }

        private void EnsureReturnLedgers(int companyId, int branchId, IDbTransaction trans)
        {
            if (branchId <= 0)
                return;

            EnsureReturnLedger(companyId, branchId, DefaultLedgers.SALESRETURN, (int)AccountGroup.SALES_ACCOUNT, trans);
            EnsureReturnLedger(companyId, branchId, DefaultLedgers.PURCHASERETURN, (int)AccountGroup.PURCHASE_ACCOUNT, trans);

            if (GetLedgerId(DefaultLedgers.BEGINSTOCK, (int)AccountGroup.STOCK_IN_HAND, branchId, trans) == 0 &&
                GetLedgerId("BEGIN STOCK", (int)AccountGroup.STOCK_IN_HAND, branchId, trans) == 0)
            {
                EnsureReturnLedger(companyId, branchId, DefaultLedgers.BEGINSTOCK, (int)AccountGroup.STOCK_IN_HAND, trans);
            }
        }

        private void EnsureReturnLedger(int companyId, int branchId, string ledgerName, int groupId, IDbTransaction trans)
        {
            if (GetLedgerId(ledgerName, groupId, branchId, trans) > 0)
                return;

            int ledgerId = GetNextLedgerId(trans);
            if (ledgerId <= 0)
                return;

            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection, (SqlTransaction)trans))
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

        private int GetLedgerId(string ledgerName, int groupId, int branchId, IDbTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._4GetLedgerIdByLedgerNameAndGroupId, (SqlConnection)DataConnection, (SqlTransaction)trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                cmd.Parameters.AddWithValue("@GroupId", groupId);
                cmd.Parameters.AddWithValue("@BranchId", branchId);

                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        private int GetNextLedgerId(IDbTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection, (SqlTransaction)trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@_Operation", "GETNEXTID");

                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }
        public Branch UpdateBranch(Branch br)
        {
            Branch item = new Branch();

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id ", br.Id);
                    cmd.Parameters.AddWithValue("@BranchName ", br.BranchName);
                    cmd.Parameters.AddWithValue("@CompanyId  ", br.CompanyId);
                    cmd.Parameters.AddWithValue("@Address  ", br.Address);
                    cmd.Parameters.AddWithValue("@Phone  ", br.Phone);
                    cmd.Parameters.AddWithValue("@IsDelete  ", 0);
                    cmd.Parameters.AddWithValue("@IsECommerceAvailable  ", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Update");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Branch>();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;

        }
        public Branch Delete(int selectedId)
        {
            Branch item = new Branch();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id ", selectedId);
                    cmd.Parameters.AddWithValue("@BranchName ", "");
                    cmd.Parameters.AddWithValue("@CompanyId  ", 0);
                    cmd.Parameters.AddWithValue("@Address  ", "");
                    cmd.Parameters.AddWithValue("@Phone  ", "");
                    cmd.Parameters.AddWithValue("@IsDelete  ", 0);
                    cmd.Parameters.AddWithValue("@IsECommerceAvailable  ", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Delete");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Branch>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public Branch GetById(int selectedId)
        {
            Branch item = new Branch();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id ", selectedId);
                    cmd.Parameters.AddWithValue("@BranchName ", "");
                    cmd.Parameters.AddWithValue("@CompanyId  ", 0);
                    cmd.Parameters.AddWithValue("@Address  ", "");
                    cmd.Parameters.AddWithValue("@Phone  ", "");
                    cmd.Parameters.AddWithValue("@IsDelete  ", 0);
                    cmd.Parameters.AddWithValue("@IsECommerceAvailable  ", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Branch>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public BranchDDlGrid SearchBranch(string searchTerm)
        {
            BranchDDlGrid records = new BranchDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchName", searchTerm);
                    cmd.Parameters.AddWithValue("@_Operation", "Search");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            records.List = ds.Tables[0].ToListOfObject<BranchDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return records;
        }



    }
}
