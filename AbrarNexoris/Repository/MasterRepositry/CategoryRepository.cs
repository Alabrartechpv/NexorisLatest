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
  public  class CategoryRepository: BaseRepostitory
    {
        public string SaveCategory(Category category)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Category> listcategory = DataConnection.Query<Category>(STOREDPROCEDURE.POS_Category, category, trans,
                    commandType: CommandType.StoredProcedure).ToList<Category>();
                if (listcategory.Count > 0)
                {

                }
                trans.Commit();
            }
            catch (Exception)
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

        public Category GetByIdCategory(int selectedId)
        {
            Category item = new Category();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Category, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", selectedId);
                    cmd.Parameters.AddWithValue("@CategoryName", item.CategoryName);
                    cmd.Parameters.AddWithValue("@GroupId", item.GroupId);
                    cmd.Parameters.AddWithValue("@Photo", item.Photo);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Category>();
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

        public Category UpdateCategory(Category ct)
        {
            Category catitem = new Category();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Category, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", ct.Id);
                    cmd.Parameters.AddWithValue("@CategoryName", ct.CategoryName);
                    cmd.Parameters.AddWithValue("@GroupId", ct.GroupId);
                    cmd.Parameters.AddWithValue("@Photo", ct.Photo);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            catitem = ds.Tables[0].Rows[0].ToNullableObject<Category>();
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
            return catitem;
        }

        public Category DeleteCategory(int id)
        {
            Category deletedCategory = new Category();
            DataConnection.Open();

            try
            {
                // First get the category details before soft deleting
                using (SqlCommand getCmd = new SqlCommand("SELECT Id, CategoryName, GroupId, cast(Photo as varbinary(max)) as Photo FROM Category WHERE Id = @Id AND IsDelete = 0", (SqlConnection)DataConnection))
                {
                    getCmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(getCmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
                        {
                            deletedCategory = ds.Tables[0].Rows[0].ToNullableObject<Category>();
                        }
                    }
                }

                // Now perform the soft delete by setting IsDelete = 1
                if (deletedCategory != null && deletedCategory.Id > 0)
                {
                    using (SqlCommand deleteCmd = new SqlCommand("UPDATE Category SET IsDelete = 1 WHERE Id = @Id", (SqlConnection)DataConnection))
                    {
                        deleteCmd.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = deleteCmd.ExecuteNonQuery();
                        
                        if (rowsAffected == 0)
                        {
                            // If no rows were affected, the category might not exist or already be deleted
                            deletedCategory = null;
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
            return deletedCategory;
        }
    }
}
