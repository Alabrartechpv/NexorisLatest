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
   public class GroupRepository: BaseRepostitory
    {
        public string SaveGroup(Group group)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Group> listgroup = DataConnection.Query<Group>(STOREDPROCEDURE.POS_Group, group, trans,
                    commandType: CommandType.StoredProcedure).ToList<Group>();
                if (listgroup.Count > 0)
                {
                    trans.Commit();

                }
                else
                {
                    trans.Rollback();
                }
            }
            catch (Exception ex)
            {

                trans.Rollback();
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return "Success";
        }
        public Group GetById1(int selectedId)
        {
            Group item = new Group();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Group, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", selectedId);
                    cmd.Parameters.AddWithValue("@GroupName", "");
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@Photo", item.Photo);
                    //cmd.Parameters.AddWithValue("@PhotoByteArray", "");
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Group>();
                            //item.Photo = (item.PhotoByteArray);
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
        public Group UpdateGroup(Group g)
        {
            Group groupitem = new Group();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Group, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", g.Id);
                    cmd.Parameters.AddWithValue("@GroupName", g.GroupName);
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@Photo", g.Photo);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            groupitem = ds.Tables[0].Rows[0].ToNullableObject<Group>();
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
            return groupitem;
        }
    }
}
