using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Repository
{
    public class DatabaseRepository : BaseRepostitory
    {
        /// <summary>
        /// Backs up the active database to the specified folder.
        /// </summary>
        /// <param name="backupFolder">The directory on the database server to write the backup file to.</param>
        /// <param name="backupFilePath">Outputs the generated absolute path of the backup file.</param>
        /// <param name="errorMessage">Outputs any SQL or system error message encountered.</param>
        /// <returns>True if the backup succeeded, false otherwise.</returns>
        public bool BackupDatabase(string backupFolder, out string backupFilePath, out string errorMessage)
        {
            backupFilePath = string.Empty;
            errorMessage = string.Empty;

            try
            {
                // Ensure directory path is absolute and clean
                backupFolder = backupFolder.Trim();
                
                // If it's a local path relative to SQL Server, we try to ensure it exists.
                // Note: If SQL Server is remote, this directory check executes on the client PC.
                // It is still a useful check for local development and common single-PC setups.
                if (!Directory.Exists(backupFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(backupFolder);
                    }
                    catch { /* Might fail if remote network folder, let SQL Server handle it */ }
                }

                // Get database name from the connection string
                SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(DataConnection.ConnectionString);
                string dbName = connBuilder.InitialCatalog;

                if (string.IsNullOrWhiteSpace(dbName))
                {
                    errorMessage = "Database name could not be parsed from the connection configuration.";
                    return false;
                }

                string fileName = $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                backupFilePath = Path.Combine(backupFolder, fileName);

                // SQL command to perform native backup
                string query = "BACKUP DATABASE [" + dbName + "] TO DISK = @BackupPath WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using (SqlConnection conn = new SqlConnection(DataConnection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout for large databases
                        cmd.Parameters.AddWithValue("@BackupPath", backupFilePath);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Restores the database from a backup file (.bak).
        /// </summary>
        /// <param name="backupFilePath">The absolute path to the backup file on the server.</param>
        /// <param name="errorMessage">Outputs any SQL or system error message encountered.</param>
        /// <returns>True if the restore succeeded, false otherwise.</returns>
        public bool RestoreDatabase(string backupFilePath, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(backupFilePath))
                {
                    errorMessage = "Backup file path cannot be empty.";
                    return false;
                }

                // Get database name
                SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(DataConnection.ConnectionString);
                string dbName = connBuilder.InitialCatalog;

                if (string.IsNullOrWhiteSpace(dbName))
                {
                    errorMessage = "Database name could not be parsed from the connection configuration.";
                    return false;
                }

                // Connect to master database catalog to perform the restore
                connBuilder.InitialCatalog = "master";
                string masterConnectionString = connBuilder.ConnectionString;

                // T-SQL commands to kill active sessions and restore
                string sql = $@"
                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{dbName}] FROM DISK = @BackupPath WITH REPLACE;
                    ALTER DATABASE [{dbName}] SET MULTI_USER;";

                using (SqlConnection conn = new SqlConnection(masterConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 600; // 10 minutes timeout for restore operations
                        cmd.Parameters.AddWithValue("@BackupPath", backupFilePath);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
