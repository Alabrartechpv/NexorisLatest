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

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                // 1. Ensure Table Columns exist in PayMode
                string tableScript = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayMode')
                    BEGIN
                        CREATE TABLE [dbo].[PayMode](
                            [PayModeID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [PayModeName] [nvarchar](100) NOT NULL,
                            [Description] [nvarchar](250) NULL,
                            [FunctionKey] [nvarchar](50) NULL,
                            [PaymodeType] [nvarchar](50) NULL,
                            [Category] [nvarchar](50) NULL,
                            [FileName] [nvarchar](100) NULL,
                            [Photo] [varbinary](max) NULL,
                            [RequireFillInReference] [bit] NULL DEFAULT 0,
                            [IsHide] [bit] NULL DEFAULT 0,
                            [DontOpenDrawer] [bit] NULL DEFAULT 0,
                            [LedgerID] [int] NULL,
                            [IsDelete] [bit] NULL DEFAULT 0
                        );
                    END
                    ELSE
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'Description')
                            ALTER TABLE PayMode ADD [Description] [nvarchar](250) NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'FunctionKey')
                            ALTER TABLE PayMode ADD [FunctionKey] [nvarchar](50) NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'PaymodeType')
                            ALTER TABLE PayMode ADD [PaymodeType] [nvarchar](50) NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'Category')
                            ALTER TABLE PayMode ADD [Category] [nvarchar](50) NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'FileName')
                            ALTER TABLE PayMode ADD [FileName] [nvarchar](100) NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'Photo')
                            ALTER TABLE PayMode ADD [Photo] [varbinary](max) NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'RequireFillInReference')
                            ALTER TABLE PayMode ADD [RequireFillInReference] [bit] NULL DEFAULT 0;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'IsHide')
                            ALTER TABLE PayMode ADD [IsHide] [bit] NULL DEFAULT 0;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'DontOpenDrawer')
                            ALTER TABLE PayMode ADD [DontOpenDrawer] [bit] NULL DEFAULT 0;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'LedgerID')
                            ALTER TABLE PayMode ADD [LedgerID] [int] NULL;
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PayMode') AND name = 'IsDelete')
                            ALTER TABLE PayMode ADD [IsDelete] [bit] NULL DEFAULT 0;
                    END";

                using (SqlCommand cmd = new SqlCommand(tableScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Ensure Stored Procedure _POS_PayMode_Setup exists
                string spScript = @"
                    IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = '_POS_PayMode_Setup')
                        DROP PROCEDURE [dbo].[_POS_PayMode_Setup];
                ";

                using (SqlCommand cmd = new SqlCommand(spScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string createSpScript = @"
                    CREATE PROCEDURE [dbo].[_POS_PayMode_Setup]
                        @_Operation nvarchar(50) = 'GETALL',
                        @PayModeID int = 0,
                        @PayModeName nvarchar(100) = NULL,
                        @Description nvarchar(250) = NULL,
                        @FunctionKey nvarchar(50) = NULL,
                        @PaymodeType nvarchar(50) = NULL,
                        @Category nvarchar(50) = NULL,
                        @FileName nvarchar(100) = NULL,
                        @Photo varbinary(max) = NULL,
                        @RequireFillInReference bit = 0,
                        @IsHide bit = 0,
                        @DontOpenDrawer bit = 0,
                        @LedgerID int = NULL
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        IF @_Operation = 'GETALL'
                        BEGIN
                            SELECT 
                                p.PayModeID,
                                ISNULL(p.PayModeName, '') AS PayModeName,
                                ISNULL(p.Description, '') AS Description,
                                ISNULL(p.FunctionKey, '') AS FunctionKey,
                                ISNULL(p.PaymodeType, '') AS PaymodeType,
                                ISNULL(p.Category, '') AS Category,
                                ISNULL(p.FileName, '') AS FileName,
                                p.Photo,
                                ISNULL(p.RequireFillInReference, 0) AS RequireFillInReference,
                                ISNULL(p.IsHide, 0) AS IsHide,
                                ISNULL(p.DontOpenDrawer, 0) AS DontOpenDrawer,
                                ISNULL(p.LedgerID, 0) AS LedgerID,
                                ISNULL(l.LedgerName, '') AS LedgerName
                            FROM PayMode p
                            LEFT JOIN LedgerMaster l ON p.LedgerID = l.LedgerID
                            WHERE ISNULL(p.IsDelete, 0) = 0
                            ORDER BY p.PayModeID;
                        END
                        ELSE IF @_Operation = 'GETBYID'
                        BEGIN
                            SELECT 
                                p.PayModeID,
                                ISNULL(p.PayModeName, '') AS PayModeName,
                                ISNULL(p.Description, '') AS Description,
                                ISNULL(p.FunctionKey, '') AS FunctionKey,
                                ISNULL(p.PaymodeType, '') AS PaymodeType,
                                ISNULL(p.Category, '') AS Category,
                                ISNULL(p.FileName, '') AS FileName,
                                p.Photo,
                                ISNULL(p.RequireFillInReference, 0) AS RequireFillInReference,
                                ISNULL(p.IsHide, 0) AS IsHide,
                                ISNULL(p.DontOpenDrawer, 0) AS DontOpenDrawer,
                                ISNULL(p.LedgerID, 0) AS LedgerID,
                                ISNULL(l.LedgerName, '') AS LedgerName
                            FROM PayMode p
                            LEFT JOIN LedgerMaster l ON p.LedgerID = l.LedgerID
                            WHERE p.PayModeID = @PayModeID AND ISNULL(p.IsDelete, 0) = 0;
                        END
                        ELSE IF @_Operation = 'INSERT'
                        BEGIN
                            INSERT INTO PayMode (
                                PayModeName, Description, FunctionKey, PaymodeType, Category,
                                FileName, Photo, RequireFillInReference, IsHide, DontOpenDrawer, LedgerID, IsDelete
                            )
                            VALUES (
                                @PayModeName, @Description, @FunctionKey, @PaymodeType, @Category,
                                @FileName, @Photo, @RequireFillInReference, @IsHide, @DontOpenDrawer, @LedgerID, 0
                            );

                            SELECT SCOPE_IDENTITY() AS PayModeID;
                        END
                        ELSE IF @_Operation = 'UPDATE'
                        BEGIN
                            UPDATE PayMode
                            SET PayModeName = @PayModeName,
                                Description = @Description,
                                FunctionKey = @FunctionKey,
                                PaymodeType = @PaymodeType,
                                Category = @Category,
                                FileName = @FileName,
                                Photo = CASE WHEN @Photo IS NOT NULL THEN @Photo ELSE Photo END,
                                RequireFillInReference = @RequireFillInReference,
                                IsHide = @IsHide,
                                DontOpenDrawer = @DontOpenDrawer,
                                LedgerID = @LedgerID
                            WHERE PayModeID = @PayModeID;

                            SELECT @PayModeID AS PayModeID;
                        END
                        ELSE IF @_Operation = 'DELETE'
                        BEGIN
                            UPDATE PayMode SET IsDelete = 1 WHERE PayModeID = @PayModeID;
                            SELECT @PayModeID AS PayModeID;
                        END
                        ELSE IF @_Operation = 'REMOVE_PHOTO'
                        BEGIN
                            UPDATE PayMode SET Photo = NULL WHERE PayModeID = @PayModeID;
                            SELECT @PayModeID AS PayModeID;
                        END
                    END";

                using (SqlCommand cmd = new SqlCommand(createSpScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                isStorageEnsured = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring Paymode storage: {ex.Message}");
                return false;
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                    conn.Close();
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

                if (model.PayModeID > 0)
                {
                    // --- UPDATE existing record ---
                    string updateSql = @"
                        UPDATE PayMode
                        SET PayModeName = @PayModeName,
                            Description = @Description,
                            FunctionKey = @FunctionKey,
                            PaymodeType = @PaymodeType,
                            Category = @Category,
                            FileName = @FileName,
                            Photo = CASE WHEN @Photo IS NOT NULL THEN @Photo ELSE Photo END,
                            RequireFillInReference = @RequireFillInReference,
                            IsHide = @IsHide,
                            DontOpenDrawer = @DontOpenDrawer,
                            LedgerID = @LedgerID
                        WHERE PayModeID = @PayModeID;";

                    using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                    {
                        AddDataParameters(cmd, model);
                        cmd.ExecuteNonQuery();
                        return model.PayModeID;
                    }
                }
                else
                {
                    // --- INSERT new record ---
                    // The existing PayMode table uses a non-IDENTITY int PK.
                    // We must manually compute the next safe ID before inserting.
                    int nextId = 1;
                    using (SqlCommand cmdNext = new SqlCommand(
                        "SELECT ISNULL(MAX(PayModeID), 0) + 1 FROM PayMode", conn))
                    {
                        object nextRes = cmdNext.ExecuteScalar();
                        if (nextRes != null && nextRes != DBNull.Value)
                            nextId = Convert.ToInt32(nextRes);
                    }

                    model.PayModeID = nextId;

                    string insertSql = @"
                        INSERT INTO PayMode (
                            PayModeID, PayModeName, Description, FunctionKey, PaymodeType, Category,
                            FileName, Photo, RequireFillInReference, IsHide, DontOpenDrawer, LedgerID, IsDelete
                        )
                        VALUES (
                            @PayModeID, @PayModeName, @Description, @FunctionKey, @PaymodeType, @Category,
                            @FileName, @Photo, @RequireFillInReference, @IsHide, @DontOpenDrawer, @LedgerID, 0
                        );";

                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        AddDataParameters(cmd, model);
                        cmd.ExecuteNonQuery();
                        return model.PayModeID;
                    }
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

        private void AddDataParameters(SqlCommand cmd, PaymodeModel model)
        {
            if (cmd == null || model == null) return;
            cmd.Parameters.Add("@PayModeID", SqlDbType.Int).Value = model.PayModeID;
            cmd.Parameters.Add("@PayModeName", SqlDbType.NVarChar, 100).Value = (object)model.PayModeName ?? string.Empty;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = (object)model.Description ?? string.Empty;
            cmd.Parameters.Add("@FunctionKey", SqlDbType.NVarChar, 50).Value = (object)model.FunctionKey ?? string.Empty;
            cmd.Parameters.Add("@PaymodeType", SqlDbType.NVarChar, 50).Value = (object)model.PaymodeType ?? string.Empty;
            cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = (object)model.Category ?? string.Empty;
            cmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 100).Value = (object)model.FileName ?? string.Empty;

            SqlParameter photoParam = new SqlParameter("@Photo", SqlDbType.VarBinary, -1);
            if (model.Photo != null && model.Photo.Length > 0)
                photoParam.Value = model.Photo;
            else
                photoParam.Value = DBNull.Value;
            cmd.Parameters.Add(photoParam);

            cmd.Parameters.Add("@RequireFillInReference", SqlDbType.Bit).Value = model.RequireFillInReference;
            cmd.Parameters.Add("@IsHide", SqlDbType.Bit).Value = model.IsHide;
            cmd.Parameters.Add("@DontOpenDrawer", SqlDbType.Bit).Value = model.DontOpenDrawer;

            if (model.LedgerID > 0)
                cmd.Parameters.Add("@LedgerID", SqlDbType.Int).Value = model.LedgerID;
            else
                cmd.Parameters.Add("@LedgerID", SqlDbType.Int).Value = DBNull.Value;
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

                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE PayMode SET IsDelete = 1 WHERE PayModeID = @PayModeID", conn))
                {
                    cmd.Parameters.Add("@PayModeID", SqlDbType.Int).Value = paymodeId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting paymode: {ex.Message}");
                throw ex;
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

                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE PayMode SET Photo = NULL, FileName = NULL WHERE PayModeID = @PayModeID", conn))
                {
                    cmd.Parameters.Add("@PayModeID", SqlDbType.Int).Value = paymodeId;
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
