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
