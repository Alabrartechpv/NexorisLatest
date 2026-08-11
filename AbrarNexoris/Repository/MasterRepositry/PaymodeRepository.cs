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
    public class PaymodeRepository : BaseRepostitory
    {
        private static bool isStorageEnsured = false;

        public bool EnsureStorage()
        {
            if (isStorageEnsured) return true;

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return false;

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                isStorageEnsured = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureStorage check warning: {ex.Message}");
                return false;
            }
        }

        public List<PaymodeModel> GetAllPaymodes()
        {
            List<PaymodeModel> list = new List<PaymodeModel>();
            EnsureStorage();

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return list;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PaymodeModel item = new PaymodeModel
                            {
                                PayModeID = reader["PayModeID"] != DBNull.Value ? Convert.ToInt32(reader["PayModeID"]) : 0,
                                PayModeName = reader["PayModeName"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                FunctionKey = reader["FunctionKey"]?.ToString(),
                                PaymodeType = reader["PaymodeType"]?.ToString(),
                                Category = reader["Category"]?.ToString(),
                                FileName = reader["FileName"]?.ToString(),
                                Photo = reader["Photo"] != DBNull.Value ? (byte[])reader["Photo"] : null,
                                RequireFillInReference = reader["RequireFillInReference"] != DBNull.Value && Convert.ToBoolean(reader["RequireFillInReference"]),
                                IsHide = reader["IsHide"] != DBNull.Value && Convert.ToBoolean(reader["IsHide"]),
                                DontOpenDrawer = reader["DontOpenDrawer"] != DBNull.Value && Convert.ToBoolean(reader["DontOpenDrawer"]),
                                LedgerID = reader["LedgerID"] != DBNull.Value ? Convert.ToInt32(reader["LedgerID"]) : 0,
                                LedgerName = reader["LedgerName"]?.ToString()
                            };
                            list.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting paymodes: {ex.Message}");
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return list;
        }

        public PaymodeModel GetPaymodeById(int paymodeId)
        {
            if (paymodeId <= 0) return null;
            EnsureStorage();

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return null;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@PayModeID", paymodeId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PaymodeModel
                            {
                                PayModeID = reader["PayModeID"] != DBNull.Value ? Convert.ToInt32(reader["PayModeID"]) : 0,
                                PayModeName = reader["PayModeName"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                FunctionKey = reader["FunctionKey"]?.ToString(),
                                PaymodeType = reader["PaymodeType"]?.ToString(),
                                Category = reader["Category"]?.ToString(),
                                FileName = reader["FileName"]?.ToString(),
                                Photo = reader["Photo"] != DBNull.Value ? (byte[])reader["Photo"] : null,
                                RequireFillInReference = reader["RequireFillInReference"] != DBNull.Value && Convert.ToBoolean(reader["RequireFillInReference"]),
                                IsHide = reader["IsHide"] != DBNull.Value && Convert.ToBoolean(reader["IsHide"]),
                                DontOpenDrawer = reader["DontOpenDrawer"] != DBNull.Value && Convert.ToBoolean(reader["DontOpenDrawer"]),
                                LedgerID = reader["LedgerID"] != DBNull.Value ? Convert.ToInt32(reader["LedgerID"]) : 0,
                                LedgerName = reader["LedgerName"]?.ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting paymode by id: {ex.Message}");
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return null;
        }

        public int SavePaymode(PaymodeModel model)
        {
            if (model == null) return 0;
            EnsureStorage();

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return 0;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", model.PayModeID > 0 ? "UPDATE" : "INSERT");
                    cmd.Parameters.AddWithValue("@PayModeID", model.PayModeID);
                    cmd.Parameters.AddWithValue("@PayModeName", (object)model.PayModeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FunctionKey", (object)model.FunctionKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymodeType", (object)model.PaymodeType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Category", (object)model.Category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileName", (object)model.FileName ?? DBNull.Value);

                    SqlParameter photoParam = new SqlParameter("@Photo", SqlDbType.VarBinary, -1);
                    photoParam.Value = (model.Photo != null && model.Photo.Length > 0) ? (object)model.Photo : DBNull.Value;
                    cmd.Parameters.Add(photoParam);

                    cmd.Parameters.AddWithValue("@RequireFillInReference", model.RequireFillInReference);
                    cmd.Parameters.AddWithValue("@IsHide", model.IsHide);
                    cmd.Parameters.AddWithValue("@DontOpenDrawer", model.DontOpenDrawer);
                    cmd.Parameters.AddWithValue("@LedgerID", model.LedgerID > 0 ? (object)model.LedgerID : DBNull.Value);

                    object res = cmd.ExecuteScalar();
                    return res != null && res != DBNull.Value ? Convert.ToInt32(res) : model.PayModeID;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving paymode: {ex.Message}");
                throw;
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public bool DeletePaymode(int paymodeId)
        {
            if (paymodeId <= 0) return false;
            EnsureStorage();

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return false;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    cmd.Parameters.AddWithValue("@PayModeID", paymodeId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting paymode: {ex.Message}");
                throw;
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public bool RemovePhoto(int paymodeId)
        {
            if (paymodeId <= 0) return false;
            EnsureStorage();

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return false;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "REMOVE_PHOTO");
                    cmd.Parameters.AddWithValue("@PayModeID", paymodeId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing photo: {ex.Message}");
                return false;
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}
