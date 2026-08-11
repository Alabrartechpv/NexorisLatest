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

                list = FetchPaymodesFromConn(conn);
                if (list == null || list.Count == 0)
                {
                    // Table was cleared or empty: auto-seed required default paymodes
                    EnsurePaymodeSeedData(conn);
                    list = FetchPaymodesFromConn(conn);
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

            return list ?? new List<PaymodeModel>();
        }

        private List<PaymodeModel> FetchPaymodesFromConn(SqlConnection conn)
        {
            List<PaymodeModel> list = new List<PaymodeModel>();
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
            return list;
        }

        public static void EnsurePaymodeSeedData(SqlConnection conn)
        {
            if (conn == null) return;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                // Check if records already exist via Stored Procedure
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows) return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"EnsurePaymodeSeedData GETALL check warning: {ex.Message}");
                }

                var defaultPaymodes = new List<PaymodeModel>
                {
                    new PaymodeModel { PayModeID = 1, PayModeName = "Credit", Description = "Credit Sales / Customer Account", FunctionKey = "", PaymodeType = "Credit", Category = "Credit Account", RequireFillInReference = false, IsHide = false, DontOpenDrawer = true },
                    new PaymodeModel { PayModeID = 2, PayModeName = "Cash", Description = "Cash Payment", FunctionKey = "F1", PaymodeType = "Cash", Category = "General", RequireFillInReference = false, IsHide = false, DontOpenDrawer = false },
                    new PaymodeModel { PayModeID = 3, PayModeName = "Card", Description = "Credit / Debit Card", FunctionKey = "F2", PaymodeType = "Card", Category = "Card Gateway", RequireFillInReference = true, IsHide = false, DontOpenDrawer = true },
                    new PaymodeModel { PayModeID = 4, PayModeName = "BankTransfer", Description = "Direct Bank Transfer", FunctionKey = "F3", PaymodeType = "Bank Transfer", Category = "Banking", RequireFillInReference = true, IsHide = false, DontOpenDrawer = true },
                    new PaymodeModel { PayModeID = 5, PayModeName = "UPI", Description = "UPI / QR Digital Payment", FunctionKey = "F4", PaymodeType = "UPI / QR", Category = "Digital Payment", RequireFillInReference = true, IsHide = false, DontOpenDrawer = true },
                    new PaymodeModel { PayModeID = 6, PayModeName = "Cheque", Description = "Cheque / Draft Payment", FunctionKey = "F5", PaymodeType = "Cheque", Category = "Banking", RequireFillInReference = true, IsHide = false, DontOpenDrawer = true },
                    new PaymodeModel { PayModeID = 7, PayModeName = "Gift Voucher", Description = "Gift Voucher / Coupon Payment", FunctionKey = "F6", PaymodeType = "Gift Voucher", Category = "General", RequireFillInReference = true, IsHide = false, DontOpenDrawer = true }
                };

                foreach (var pm in defaultPaymodes)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GeneralPaymodeSetup, conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@_Operation", "INSERT");
                            cmd.Parameters.AddWithValue("@PayModeID", pm.PayModeID);
                            cmd.Parameters.AddWithValue("@PayModeName", pm.PayModeName);
                            cmd.Parameters.AddWithValue("@Description", (object)pm.Description ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@FunctionKey", (object)pm.FunctionKey ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PaymodeType", (object)pm.PaymodeType ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Category", (object)pm.Category ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@FileName", DBNull.Value);

                            SqlParameter photoParam = new SqlParameter("@Photo", SqlDbType.VarBinary, -1);
                            photoParam.Value = DBNull.Value;
                            cmd.Parameters.Add(photoParam);

                            cmd.Parameters.AddWithValue("@RequireFillInReference", pm.RequireFillInReference);
                            cmd.Parameters.AddWithValue("@IsHide", pm.IsHide);
                            cmd.Parameters.AddWithValue("@DontOpenDrawer", pm.DontOpenDrawer);
                            cmd.Parameters.AddWithValue("@LedgerID", DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SP seed insertion warning for {pm.PayModeName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsurePaymodeSeedData exception: {ex.Message}");
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
            }
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
