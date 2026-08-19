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
   public class UsersRepository:BaseRepostitory
    {
        public string SaveUser(Users user)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection, (SqlTransaction)trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);
                    cmd.Parameters.AddWithValue("@CompanyID", user.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", user.BranchID);
                    cmd.Parameters.AddWithValue("@UserLevelID", user.UserLevelID);
                    cmd.Parameters.AddWithValue("@UserName", user.UserName ?? "");
                    cmd.Parameters.AddWithValue("@Password", user.Password ?? "");
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", string.IsNullOrEmpty(user._Operation) ? "CREATE" : user._Operation);
                    cmd.ExecuteNonQuery();
                }
                
                // Safe T-SQL update for UserLevelID and optional UserLevel column using sp_executesql
                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.Users SET UserLevelID = @UserLevelID WHERE UserName = @UserName;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'UserLevel')
    BEGIN
        EXEC sp_executesql N'UPDATE dbo.Users SET UserLevel = @ul WHERE UserName = @un', N'@ul NVARCHAR(100), @un NVARCHAR(100)', @ul = @UserLevel, @un = @UserName;
    END
END", (SqlConnection)DataConnection, (SqlTransaction)trans))
                {
                    cmd.Parameters.AddWithValue("@UserLevelID", user.UserLevelID);
                    cmd.Parameters.AddWithValue("@UserLevel", (object)user.UserLevel ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserName", user.UserName ?? "");
                    cmd.ExecuteNonQuery();
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
        public Users GetById(int selectedId)
        {
            Users item = new Users();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", selectedId);
                    cmd.Parameters.AddWithValue("@CompanyID", 0);
                    cmd.Parameters.AddWithValue("@BranchID", 0);
                    cmd.Parameters.AddWithValue("@UserLevelID", 0);
                    cmd.Parameters.AddWithValue("@UserName", "");
                    cmd.Parameters.AddWithValue("@Password", "");
                    cmd.Parameters.AddWithValue("@Email", "");
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Users>();
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
        public Users Update(Users us)
        {
            Users user = new Users();

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", us.UserID);
                    cmd.Parameters.AddWithValue("@CompanyID", us.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", us.BranchID);
                    cmd.Parameters.AddWithValue("@UserLevelID", us.UserLevelID);
                    cmd.Parameters.AddWithValue("@UserName", us.UserName);
                    cmd.Parameters.AddWithValue("@Password", us.Password);
                    cmd.Parameters.AddWithValue("@Email", us.Email);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            user = ds.Tables[0].Rows[0].ToNullableObject<Users>();
                        }
                    }

                    using (SqlCommand directCmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.Users SET UserLevelID = @UserLevelID WHERE UserID = @UserID;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'UserLevel')
    BEGIN
        EXEC sp_executesql N'UPDATE dbo.Users SET UserLevel = @ul WHERE UserID = @uid', N'@ul NVARCHAR(100), @uid INT', @ul = @UserLevel, @uid = @UserID;
    END
END", (SqlConnection)DataConnection))
                    {
                        directCmd.Parameters.AddWithValue("@UserLevelID", us.UserLevelID);
                        directCmd.Parameters.AddWithValue("@UserLevel", (object)us.UserLevel ?? DBNull.Value);
                        directCmd.Parameters.AddWithValue("@UserID", us.UserID);
                        directCmd.ExecuteNonQuery();
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
            return user;

        }
        public Users Delete(int selectedId)
        {
            Users item = new Users();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", selectedId);
                    cmd.Parameters.AddWithValue("@CompanyID", 0);
                    cmd.Parameters.AddWithValue("@BranchID", 0);
                    cmd.Parameters.AddWithValue("@UserLevelID", 0);
                    cmd.Parameters.AddWithValue("@UserName", "");
                    cmd.Parameters.AddWithValue("@Password", "");
                    cmd.Parameters.AddWithValue("@Email", "");
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Users>();
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
        public UserDDlGrid Search(string searchTerm)
        {
            UserDDlGrid records = new UserDDlGrid();
            DataConnection.Open();

            try
            {
                using(SqlCommand cmd =new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchID", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@UserName", searchTerm);
                    cmd.Parameters.AddWithValue("@_Operation", "SEARCH");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            records.List = ds.Tables[0].ToListOfObject<UsersDDl>();
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
