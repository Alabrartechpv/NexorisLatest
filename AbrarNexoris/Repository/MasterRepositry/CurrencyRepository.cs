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
        private void EnsureCurrencyTableExists(SqlConnection conn)
        {
            try
            {
                string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Currency' OR name = 'tblCurrency' OR name = 'CurrencyMaster')
BEGIN
    CREATE TABLE tbl_Currency (
        CurrencyID INT IDENTITY(1,1) PRIMARY KEY,
        CurrencyName NVARCHAR(100) NULL,
        CurrencyCode NVARCHAR(50) NULL,
        CurrencySymbol NVARCHAR(50) NULL,
        CurrencyUnit NVARCHAR(50) NULL,
        DecimalPlace INT NULL DEFAULT 2,
        AmntInMillions BIT NULL DEFAULT 0,
        ExchangeRate DECIMAL(18,4) NULL DEFAULT 1.0000,
        CountryID INT NULL DEFAULT 1,
        CurrencyImage VARBINARY(MAX) NULL
    );
END";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        public List<CurrencyModel> GetAllCurrencies()
        {
            List<CurrencyModel> list = new List<CurrencyModel>();
            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId > 0 ? SessionContext.BranchId : 11);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId > 0 ? SessionContext.CompanyId : 1);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1));
                    cmd.Parameters.AddWithValue("@Operation", "Currency");

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
            catch
            {
                // Direct SQL fallback if procedure fails
                try
                {
                    EnsureCurrencyTableExists((SqlConnection)DataConnection);
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_Currency ORDER BY CurrencyID", (SqlConnection)DataConnection))
                    {
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
                catch { }
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
                string[] procedures = new string[] { STOREDPROCEDURE.POS_Currency, "POS_Currency", "USP_POS_Currency" };
                bool executed = false;

                foreach (var procName in procedures)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(procName, (SqlConnection)DataConnection))
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
                        executed = true;
                        break;
                    }
                    catch (SqlException exSql) when (exSql.Number == 2812)
                    {
                        continue;
                    }
                }

                if (!executed)
                {
                    List<CurrencyModel> all = GetAllCurrencies();
                    item = all.FirstOrDefault(c => c.CurrencyID == selectedId) ?? new CurrencyModel();
                }
            }
            catch
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
                string[] procedures = new string[] { STOREDPROCEDURE.POS_Currency, "POS_Currency", "USP_POS_Currency" };
                bool executed = false;

                foreach (var procName in procedures)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(procName, (SqlConnection)DataConnection))
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
                            cmd.Parameters.AddWithValue("@CurrencyImage", (object)model.CurrencyImage ?? DBNull.Value);
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
                        executed = true;
                        break;
                    }
                    catch (SqlException exSql) when (exSql.Number == 2812)
                    {
                        continue;
                    }
                }

                if (!executed)
                {
                    EnsureCurrencyTableExists((SqlConnection)DataConnection);
                    string insertSql = @"
INSERT INTO tbl_Currency (CurrencyName, CurrencyCode, CurrencySymbol, CurrencyUnit, DecimalPlace, AmntInMillions, ExchangeRate, CountryID, CurrencyImage)
VALUES (@CurrencyName, @CurrencyCode, @CurrencySymbol, @CurrencyUnit, @DecimalPlace, @AmntInMillions, @ExchangeRate, @CountryID, @CurrencyImage);
SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(insertSql, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CurrencyName", model.CurrencyName ?? "");
                        cmd.Parameters.AddWithValue("@CurrencyCode", model.CurrencyCode ?? "");
                        cmd.Parameters.AddWithValue("@CurrencySymbol", model.CurrencySymbol ?? "");
                        cmd.Parameters.AddWithValue("@CurrencyUnit", model.CurrencyUnit ?? "");
                        cmd.Parameters.AddWithValue("@DecimalPlace", model.DecimalPlace);
                        cmd.Parameters.AddWithValue("@AmntInMillions", model.AmntInMillions);
                        cmd.Parameters.AddWithValue("@ExchangeRate", model.ExchangeRate);
                        cmd.Parameters.AddWithValue("@CountryID", model.CountryID);
                        cmd.Parameters.AddWithValue("@CurrencyImage", (object)model.CurrencyImage ?? DBNull.Value);

                        var newIdObj = cmd.ExecuteScalar();
                        if (newIdObj != null && int.TryParse(newIdObj.ToString(), out int newId))
                        {
                            model.CurrencyID = newId;
                        }
                    }
                    result = model;
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
                string[] procedures = new string[] { STOREDPROCEDURE.POS_Currency, "POS_Currency", "USP_POS_Currency" };
                bool executed = false;

                foreach (var procName in procedures)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(procName, (SqlConnection)DataConnection))
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
                            cmd.Parameters.AddWithValue("@CurrencyImage", (object)model.CurrencyImage ?? DBNull.Value);
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
                        executed = true;
                        break;
                    }
                    catch (SqlException exSql) when (exSql.Number == 2812)
                    {
                        continue;
                    }
                }

                if (!executed)
                {
                    EnsureCurrencyTableExists((SqlConnection)DataConnection);
                    string updateSql = @"
UPDATE tbl_Currency SET
    CurrencyName = @CurrencyName,
    CurrencyCode = @CurrencyCode,
    CurrencySymbol = @CurrencySymbol,
    CurrencyUnit = @CurrencyUnit,
    DecimalPlace = @DecimalPlace,
    AmntInMillions = @AmntInMillions,
    ExchangeRate = @ExchangeRate,
    CountryID = @CountryID,
    CurrencyImage = @CurrencyImage
WHERE CurrencyID = @CurrencyID;";

                    using (SqlCommand cmd = new SqlCommand(updateSql, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CurrencyID", model.CurrencyID);
                        cmd.Parameters.AddWithValue("@CurrencyName", model.CurrencyName ?? "");
                        cmd.Parameters.AddWithValue("@CurrencyCode", model.CurrencyCode ?? "");
                        cmd.Parameters.AddWithValue("@CurrencySymbol", model.CurrencySymbol ?? "");
                        cmd.Parameters.AddWithValue("@CurrencyUnit", model.CurrencyUnit ?? "");
                        cmd.Parameters.AddWithValue("@DecimalPlace", model.DecimalPlace);
                        cmd.Parameters.AddWithValue("@AmntInMillions", model.AmntInMillions);
                        cmd.Parameters.AddWithValue("@ExchangeRate", model.ExchangeRate);
                        cmd.Parameters.AddWithValue("@CountryID", model.CountryID);
                        cmd.Parameters.AddWithValue("@CurrencyImage", (object)model.CurrencyImage ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                    result = model;
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
                string[] procedures = new string[] { STOREDPROCEDURE.POS_Currency, "POS_Currency", "USP_POS_Currency" };
                bool executed = false;

                foreach (var procName in procedures)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(procName, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CurrencyID", selectedId);
                            cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                            cmd.ExecuteNonQuery();
                        }
                        executed = true;
                        break;
                    }
                    catch (SqlException exSql) when (exSql.Number == 2812)
                    {
                        continue;
                    }
                }

                if (!executed)
                {
                    EnsureCurrencyTableExists((SqlConnection)DataConnection);
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM tbl_Currency WHERE CurrencyID = @CurrencyID", (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CurrencyID", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
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
