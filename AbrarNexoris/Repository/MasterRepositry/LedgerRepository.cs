using ModelClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.MasterRepositry
{
    public class LedgerRepository : BaseRepostitory
    {
        public int GetLedgerId(string LedgerName, int GroupId, int BranchId)
        {
            int LedgerID = 0;
            bool wasConnectionClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                // Only open if connection was closed
                if (wasConnectionClosed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._4GetLedgerIdByLedgerNameAndGroupId, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LedgerName", LedgerName);
                    cmd.Parameters.AddWithValue("@GroupId", GroupId);
                    cmd.Parameters.AddWithValue("@BranchId", BranchId); // Use the passed BranchId parameter, not hardcoded

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            LedgerID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
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
                // Only close if we opened it
                if (wasConnectionClosed && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return LedgerID;
        }

        public AccountLedgerDDLGrid getAccountLedgerDDL(AccountLedgerDDLRequest request)
        {
            AccountLedgerDDLGrid grid = new AccountLedgerDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._4GetAccountLedgerDDL, (SqlConnection)DataConnection))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", request.BranchId);
                    cmd.Parameters.AddWithValue("@For", request.For);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<AccountLedgerDDL>();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {

                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return grid;
        }

    }
}
