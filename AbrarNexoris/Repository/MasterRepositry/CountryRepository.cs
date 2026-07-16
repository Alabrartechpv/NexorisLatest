using Dapper;
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
  public  class CountryRepository: BaseRepostitory
    {
        public string SaveCountry(Country country)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Country> listCountry = DataConnection.Query<Country>(STOREDPROCEDURE.POS_Country, country, trans,
                    commandType: CommandType.StoredProcedure).ToList<Country>();
                if (listCountry.Count > 0)
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
        public Country GetByIdCountry(int selectedId)
        {
            Country item = new Country();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CountryID", selectedId);
                    cmd.Parameters.AddWithValue("@CountryName", "");
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@FinYearFrom", "");
                    cmd.Parameters.AddWithValue("@FinYearTo", "");
                    cmd.Parameters.AddWithValue("@BookFrom", "");
                    cmd.Parameters.AddWithValue("@BookTo", "");
                    cmd.Parameters.AddWithValue("@TaxTypeId", "");
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Country>();
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
        public Country UpdateCountry(Country coun)
        {
            Country item = new Country();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CountryID", coun.CountryID);
                    cmd.Parameters.AddWithValue("@CountryName", coun.CountryName);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@FinYearFrom", coun.FinYearFrom);
                    cmd.Parameters.AddWithValue("@FinYearTo", coun.FinYearTo);
                    cmd.Parameters.AddWithValue("@BookFrom", coun.BookFrom);
                    cmd.Parameters.AddWithValue("@BookTo", coun.BookTo);
                    cmd.Parameters.AddWithValue("@TaxTypeId", coun.TaxTypeId);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Country>();
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
        public Country DeleteCountry(int selectedId)
        {
            Country item = new Country();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CountryID", selectedId);
                    cmd.Parameters.AddWithValue("@CountryName", "");
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@FinYearFrom", "");
                    cmd.Parameters.AddWithValue("@FinYearTo", "");
                    cmd.Parameters.AddWithValue("@BookFrom", "");
                    cmd.Parameters.AddWithValue("@BookTo", "");
                    cmd.Parameters.AddWithValue("@TaxTypeId", "");
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<Country>();
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
    }
}
