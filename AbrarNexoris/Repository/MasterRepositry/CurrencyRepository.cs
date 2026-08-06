using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ModelClass;
using ModelClass.Master;

namespace Repository.MasterRepositry
{
    public class CurrencyRepository : BaseRepostitory
    {
        private void AddImageParameter(SqlCommand cmd, byte[] imageBytes)
        {
            var p = cmd.Parameters.Add("@CurrencyImage", SqlDbType.VarBinary);
            if (imageBytes != null && imageBytes.Length > 0)
                p.Value = imageBytes;
            else
                p.Value = DBNull.Value;
        }

        public List<CurrencyModel> GetAllCurrencies()
        {
            List<CurrencyModel> list = new List<CurrencyModel>();
            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Currency, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            list = ds.Tables[0].ToListOfObject<CurrencyModel>();
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
            return list;
        }

        public CurrencyModel GetByIdCurrency(int selectedId)
        {
            CurrencyModel item = new CurrencyModel();
            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Currency, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CurrencyID", selectedId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<CurrencyModel>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                List<CurrencyModel> all = GetAllCurrencies();
                item = all.FirstOrDefault(c => c.CurrencyID == selectedId) ?? new CurrencyModel();
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public CurrencyModel SaveCurrency(CurrencyModel model)
        {
            CurrencyModel result = new CurrencyModel();
            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Currency, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CurrencyID", model.CurrencyID);
                    cmd.Parameters.AddWithValue("@CurrencyName", model.CurrencyName ?? "");
                    cmd.Parameters.AddWithValue("@CurrencyCode", model.CurrencyCode ?? "");
                    cmd.Parameters.AddWithValue("@CurrencySymbol", model.CurrencySymbol ?? "");
                    cmd.Parameters.AddWithValue("@CurrencyUnit", model.CurrencyUnit ?? "");
                    cmd.Parameters.AddWithValue("@DecimalPlace", model.DecimalPlace);
                    cmd.Parameters.AddWithValue("@AmntInMillions", model.AmntInMillions);
                    cmd.Parameters.AddWithValue("@ExchangeRate", model.ExchangeRate);
                    cmd.Parameters.AddWithValue("@CountryID", model.CountryID);
                    AddImageParameter(cmd, model.CurrencyImage);
                    cmd.Parameters.AddWithValue("@_Operation", "INSERT");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            result = ds.Tables[0].Rows[0].ToNullableObject<CurrencyModel>();
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
            return result;
        }

        public CurrencyModel UpdateCurrency(CurrencyModel model)
        {
            CurrencyModel result = new CurrencyModel();
            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Currency, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CurrencyID", model.CurrencyID);
                    cmd.Parameters.AddWithValue("@CurrencyName", model.CurrencyName ?? "");
                    cmd.Parameters.AddWithValue("@CurrencyCode", model.CurrencyCode ?? "");
                    cmd.Parameters.AddWithValue("@CurrencySymbol", model.CurrencySymbol ?? "");
                    cmd.Parameters.AddWithValue("@CurrencyUnit", model.CurrencyUnit ?? "");
                    cmd.Parameters.AddWithValue("@DecimalPlace", model.DecimalPlace);
                    cmd.Parameters.AddWithValue("@AmntInMillions", model.AmntInMillions);
                    cmd.Parameters.AddWithValue("@ExchangeRate", model.ExchangeRate);
                    cmd.Parameters.AddWithValue("@CountryID", model.CountryID);
                    AddImageParameter(cmd, model.CurrencyImage);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            result = ds.Tables[0].Rows[0].ToNullableObject<CurrencyModel>();
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
            return result;
        }

        public bool DeleteCurrency(int selectedId)
        {
            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Currency, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CurrencyID", selectedId);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    cmd.ExecuteNonQuery();
                    return true;
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
        }
    }
}
