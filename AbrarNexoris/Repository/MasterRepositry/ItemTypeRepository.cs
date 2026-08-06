using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ModelClass;
using ModelClass.Master;

namespace Repository.MasterRepositry
{
    public class ItemTypeRepository : BaseRepostitory
    {
        private static bool _storageEnsured = false;

        public ItemTypeRepository()
        {
            EnsureStorage();
        }

        public void EnsureStorage()
        {
            if (_storageEnsured) return;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                // 1. Ensure Table and missing columns
                string tableScript = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemTypes') AND EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemType')
BEGIN
    EXEC('SELECT Id, ItemType, CAST(0 AS BIT) AS IsDelete, CAST(0 AS BIT) AS IsDefault INTO ItemTypes FROM ItemType');
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemTypes')
BEGIN
    CREATE TABLE ItemTypes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ItemType NVARCHAR(200) NOT NULL,
        IsDelete BIT NOT NULL DEFAULT 0,
        IsDefault BIT NOT NULL DEFAULT 0
    );
END;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemTypes')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ItemTypes') AND name = 'IsDelete')
    BEGIN
        ALTER TABLE ItemTypes ADD IsDelete BIT NOT NULL DEFAULT 0;
    END;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ItemTypes') AND name = 'IsDefault')
    BEGIN
        ALTER TABLE ItemTypes ADD IsDefault BIT NOT NULL DEFAULT 0;
    END;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ItemTypes') AND name = 'ItemType')
    BEGIN
        ALTER TABLE ItemTypes ADD ItemType NVARCHAR(200) NULL;
    END;
END;
";
                using (SqlCommand cmd = new SqlCommand(tableScript, (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Ensure Procedure Stub Exists
                string stubScript = @"
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'POS_ItemType')
BEGIN
    EXEC('CREATE PROCEDURE POS_ItemType AS BEGIN SET NOCOUNT ON; END');
END;
";
                using (SqlCommand cmd = new SqlCommand(stubScript, (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3. Alter Procedure to full implementation
                string procScript = @"
ALTER PROCEDURE POS_ItemType
    @Id INT = 0,
    @ItemType NVARCHAR(200) = '',
    @IsDelete BIT = 0,
    @IsDefault BIT = 0,
    @_Operation NVARCHAR(50) = 'GETALL'
AS
BEGIN
    SET NOCOUNT ON;

    IF @_Operation = 'CREATE' OR @_Operation = 'INSERT'
    BEGIN
        INSERT INTO ItemTypes (ItemType, IsDelete, IsDefault)
        VALUES (NULLIF(@ItemType, ''), 0, @IsDefault);

        SELECT SCOPE_IDENTITY() AS Id, @ItemType AS ItemTypeName, CAST(0 AS BIT) AS IsDelete, @IsDefault AS IsDefault;
    END
    ELSE IF @_Operation = 'UPDATE'
    BEGIN
        UPDATE ItemTypes
        SET ItemType = ISNULL(NULLIF(@ItemType, ''), ItemType)
        WHERE Id = @Id;

        SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE Id = @Id;
    END
    ELSE IF @_Operation = 'DELETE'
    BEGIN
        UPDATE ItemTypes
        SET IsDelete = 1
        WHERE Id = @Id;

        SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE Id = @Id;
    END
    ELSE IF @_Operation = 'SETDEFAULT'
    BEGIN
        UPDATE ItemTypes SET IsDefault = 0;
        UPDATE ItemTypes SET IsDefault = 1 WHERE Id = @Id;
        SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE Id = @Id;
    END
    ELSE IF @_Operation = 'GETDEFAULT'
    BEGIN
        SELECT TOP 1 Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault
        FROM ItemTypes
        WHERE ISNULL(IsDelete, 0) = 0 AND ISNULL(IsDefault, 0) = 1;
    END
    ELSE IF @_Operation = 'GETBYID'
    BEGIN
        SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault
        FROM ItemTypes
        WHERE Id = @Id AND ISNULL(IsDelete, 0) = 0;
    END
    ELSE IF @_Operation = 'SEARCH'
    BEGIN
        SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault
        FROM ItemTypes
        WHERE ISNULL(IsDelete, 0) = 0 AND ItemType LIKE '%' + ISNULL(@ItemType, '') + '%'
        ORDER BY Id ASC;
    END
    ELSE
    BEGIN
        SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault
        FROM ItemTypes
        WHERE ISNULL(IsDelete, 0) = 0
        ORDER BY Id ASC;
    END
END
";
                using (SqlCommand cmd = new SqlCommand(procScript, (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                _storageEnsured = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring ItemTypes storage: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private void EnsureColumnsExist()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
                string script = @"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemTypes')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ItemTypes') AND name = 'IsDelete')
    BEGIN
        ALTER TABLE ItemTypes ADD IsDelete BIT NOT NULL DEFAULT 0;
    END;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ItemTypes') AND name = 'IsDefault')
    BEGIN
        ALTER TABLE ItemTypes ADD IsDefault BIT NOT NULL DEFAULT 0;
    END;
END;
";
                using (SqlCommand cmd = new SqlCommand(script, (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        public string SaveItemType(ItemType itemType)
        {
            _storageEnsured = false;
            EnsureStorage();
            EnsureColumnsExist();
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemType, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", itemType.Id);
                        cmd.Parameters.AddWithValue("@ItemType", itemType.ItemTypeName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@IsDelete", 0);
                        cmd.Parameters.AddWithValue("@IsDefault", itemType.IsDefault ? 1 : 0);
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                        cmd.ExecuteNonQuery();
                        return "Success";
                    }
                }
                catch (Exception exProc)
                {
                    System.Diagnostics.Debug.WriteLine($"SaveItemType procedure error, executing direct fallback: {exProc.Message}");
                    EnsureColumnsExist();
                    using (SqlCommand fallbackCmd = new SqlCommand("INSERT INTO ItemTypes (ItemType, IsDelete, IsDefault) VALUES (@ItemType, 0, @IsDefault)", (SqlConnection)DataConnection))
                    {
                        fallbackCmd.Parameters.AddWithValue("@ItemType", itemType.ItemTypeName ?? string.Empty);
                        fallbackCmd.Parameters.AddWithValue("@IsDefault", itemType.IsDefault ? 1 : 0);
                        fallbackCmd.ExecuteNonQuery();
                        return "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
        }

        public ItemType UpdateItemType(ItemType itemType)
        {
            EnsureStorage();
            EnsureColumnsExist();
            ItemType result = null;
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemType, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", itemType.Id);
                        cmd.Parameters.AddWithValue("@ItemType", itemType.ItemTypeName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@IsDelete", 0);
                        cmd.Parameters.AddWithValue("@IsDefault", itemType.IsDefault ? 1 : 0);
                        cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapt.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                result = dt.Rows[0].ToNullableObject<ItemType>();
                            }
                        }
                    }
                }
                catch
                {
                    EnsureColumnsExist();
                    using (SqlCommand fallbackCmd = new SqlCommand("UPDATE ItemTypes SET ItemType = @ItemType WHERE Id = @Id", (SqlConnection)DataConnection))
                    {
                        fallbackCmd.Parameters.AddWithValue("@Id", itemType.Id);
                        fallbackCmd.Parameters.AddWithValue("@ItemType", itemType.ItemTypeName ?? string.Empty);
                        fallbackCmd.ExecuteNonQuery();
                        result = itemType;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
            return result;
        }

        public bool SetDefaultItemType(int id)
        {
            EnsureStorage();
            EnsureColumnsExist();
            if (id <= 0) return false;
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemType, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@ItemType", DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsDelete", 0);
                        cmd.Parameters.AddWithValue("@IsDefault", 1);
                        cmd.Parameters.AddWithValue("@_Operation", "SETDEFAULT");

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch
                {
                    EnsureColumnsExist();
                    using (SqlCommand clearCmd = new SqlCommand("UPDATE ItemTypes SET IsDefault = 0", (SqlConnection)DataConnection))
                    {
                        clearCmd.ExecuteNonQuery();
                    }
                    using (SqlCommand setCmd = new SqlCommand("UPDATE ItemTypes SET IsDefault = 1 WHERE Id = @Id", (SqlConnection)DataConnection))
                    {
                        setCmd.Parameters.AddWithValue("@Id", id);
                        setCmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting default ItemType: {ex.Message}");
                return false;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
        }

        public ItemType GetDefaultItemType()
        {
            EnsureStorage();
            EnsureColumnsExist();
            ItemType item = null;
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE ISNULL(IsDelete, 0) = 0 AND ISNULL(IsDefault, 0) = 1 ORDER BY Id ASC", (SqlConnection)DataConnection))
                {
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            item = dt.Rows[0].ToNullableObject<ItemType>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting default ItemType: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
            return item;
        }

        public ItemType DeleteItemType(int id)
        {
            EnsureStorage();
            EnsureColumnsExist();
            ItemType result = null;
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE ItemTypes SET IsDelete = 1 WHERE Id = @Id", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                    result = new ItemType { Id = id, IsDelete = true };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
            return result;
        }

        public ItemType GetItemTypeById(int selectedId)
        {
            EnsureStorage();
            EnsureColumnsExist();
            ItemType item = new ItemType();
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE Id = @Id AND ISNULL(IsDelete, 0) = 0", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", selectedId);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            item = dt.Rows[0].ToNullableObject<ItemType>();
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
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
            return item;
        }

        public List<ItemType> GetAllItemTypes()
        {
            EnsureStorage();
            EnsureColumnsExist();
            List<ItemType> list = new List<ItemType>();
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE ISNULL(IsDelete, 0) = 0 ORDER BY Id ASC", (SqlConnection)DataConnection))
                {
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            list = dt.ToListOfObject<ItemType>().ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllItemTypes: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
            return list;
        }

        public List<ItemType> SearchItemTypes(string search)
        {
            EnsureStorage();
            EnsureColumnsExist();
            List<ItemType> list = new List<ItemType>();
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Id, ItemType AS ItemTypeName, ISNULL(IsDelete, 0) AS IsDelete, ISNULL(IsDefault, 0) AS IsDefault FROM ItemTypes WHERE ISNULL(IsDelete, 0) = 0 AND ItemType LIKE '%' + @Search + '%' ORDER BY Id ASC", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Search", search ?? string.Empty);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            list = dt.ToListOfObject<ItemType>().ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SearchItemTypes: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open) DataConnection.Close();
            }
            return list;
        }
    }
}
