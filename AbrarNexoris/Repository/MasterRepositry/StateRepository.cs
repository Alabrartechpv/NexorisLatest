using Dapper;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModelClass.Master.StateModel;

namespace Repository.MasterRepositry
{
  public  class StateRepository:BaseRepostitory
    {
        public string SaveState(State state)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<State> liststate = DataConnection.Query<State>(STOREDPROCEDURE.POS_State, state, trans,
                    commandType: CommandType.StoredProcedure).ToList<State>();
                if (liststate.Count > 0)
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

        public State GetByIdState(int selectedId)
        {
            State item = new State();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_State, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StateID ", selectedId);
                    cmd.Parameters.AddWithValue("@StateName ", "");
                    cmd.Parameters.AddWithValue("@StateCode  ", 0);
                    cmd.Parameters.AddWithValue("@CountryID  ", 0);
                    cmd.Parameters.AddWithValue("@IsDelete  ", 0);
                    cmd.Parameters.AddWithValue("@_Operation  ", "GetByID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<State>();
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
        public State UpdateState(State se)
        {
            State item = new State();

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_State, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StateID ", se.StateID);
                    cmd.Parameters.AddWithValue("@StateName ", se.StateName);
                    cmd.Parameters.AddWithValue("@CountryID  ", se.CountryID);
                    cmd.Parameters.AddWithValue("@StateCode  ", se.StateCode);
                    cmd.Parameters.AddWithValue("@IsDelete  ", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Update");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<State>();

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
        public State DeleteState(int selectedId)
        {
            State item = new State();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_State, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StateID ", selectedId);
                    cmd.Parameters.AddWithValue("@StateName ", "");
                    cmd.Parameters.AddWithValue("@CountryID  ", 0);
                    cmd.Parameters.AddWithValue("@StateCode  ", 0);
                    cmd.Parameters.AddWithValue("@IsDelete  ", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Delete");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<State>();
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
        public StateDDlGrid SearchRecords(string searchTerm)
        {
            StateDDlGrid records = new StateDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_State, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StateName", searchTerm);
                    cmd.Parameters.AddWithValue("@_Operation", "Search");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            records.List = ds.Tables[0].ToListOfObject<StateDDL>();
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
