using System;
using System.Data;
using System.Data.SqlClient;
using Repository;

namespace PosBranch_Win.Utilities
{
    public static class InitialSetupHelper
    {
        /// <summary>
        /// Checks if the Branches table is empty (which indicates a cleared database).
        /// </summary>
        public static bool IsDatabaseEmpty()
        {
            BaseRepostitory repo = null;
            try
            {
                repo = new BaseRepostitory();
                if (repo.DataConnection == null)
                    return false;

                if (repo.DataConnection.State != ConnectionState.Open)
                    repo.DataConnection.Open();

                // Check if Branches table exists in the schema
                string checkTableSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Branches'";
                using (SqlCommand checkCmd = new SqlCommand(checkTableSql, (SqlConnection)repo.DataConnection))
                {
                    int tableCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (tableCount == 0)
                    {
                        // Branches table does not exist, database is uninitialized/empty schema
                        return true;
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Branches WHERE IsDelete = 0", (SqlConnection)repo.DataConnection))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if database is empty: {ex.Message}");
                if (ex is SqlException sqlEx && sqlEx.Number == 208)
                {
                    return true;
                }
                return false; // Fallback to normal login flow on connection/database errors
            }
            finally
            {
                if (repo != null)
                {
                    repo.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs initial seeding of Company, Financial Year, User Levels, Admin User, and Branch.
        /// </summary>
        public static bool InitializeDatabase(
            string companyName,
            string companyCaption,
            string branchName,
            string branchAddress,
            string branchPhone,
            string adminPassword)
        {
            BaseRepostitory repo = null;
            SqlTransaction transaction = null;

            try
            {
                repo = new BaseRepostitory();
                if (repo.DataConnection.State != ConnectionState.Open)
                    repo.DataConnection.Open();

                SqlConnection conn = (SqlConnection)repo.DataConnection;

                // Verify core schema tables exist before executing seed scripts
                string checkSchemaSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME IN ('CompanyInfo', 'FinancialYear', 'Userlevels', 'Users', 'Branches')";
                using (SqlCommand checkSchema = new SqlCommand(checkSchemaSql, conn))
                {
                    int tableCount = Convert.ToInt32(checkSchema.ExecuteScalar());
                    if (tableCount < 5)
                    {
                        throw new InvalidOperationException("The database schema tables ('CompanyInfo', 'FinancialYear', etc.) are missing. Please execute the database schema installation scripts on SQL Server first.");
                    }
                }

                transaction = conn.BeginTransaction();

                // 1. Insert CompanyInfo if empty
                SqlCommand checkComp = new SqlCommand("SELECT COUNT(*) FROM CompanyInfo WHERE IsDelete = 0", conn, transaction);
                int companyCount = Convert.ToInt32(checkComp.ExecuteScalar());
                int companyId = 1;

                if (companyCount == 0)
                {
                    string insertCompSql = @"
                        INSERT INTO CompanyInfo (
                            CompanyID, CompanyName, CompanyCaption, Address1, Country, State, 
                            Zipcode, Phone, Mobile, Email, Website, BusinessType, BackupPath, 
                            FinYearFrom, FinYearTo, BookFrom, BookTo, TaxSystem, Currency, IsDelete
                        ) VALUES (
                            @CompanyID, @CompanyName, @CompanyCaption, @Address1, @Country, @State, 
                            @Zipcode, @Phone, @Mobile, @Email, @Website, @BusinessType, @BackupPath, 
                            @FinYearFrom, @FinYearTo, @BookFrom, @BookTo, @TaxSystem, @Currency, @IsDelete
                        )";

                    using (SqlCommand insertComp = new SqlCommand(insertCompSql, conn, transaction))
                    {
                        insertComp.Parameters.AddWithValue("@CompanyID", 1);
                        insertComp.Parameters.AddWithValue("@CompanyName", companyName.Trim());
                        insertComp.Parameters.AddWithValue("@CompanyCaption", string.IsNullOrWhiteSpace(companyCaption) ? "Nexoris Retail" : companyCaption.Trim());
                        insertComp.Parameters.AddWithValue("@Address1", "Main Street");
                        insertComp.Parameters.AddWithValue("@Country", 1);
                        insertComp.Parameters.AddWithValue("@State", 1);
                        insertComp.Parameters.AddWithValue("@Zipcode", "12345");
                        insertComp.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(branchPhone) ? "123456789" : branchPhone.Trim());
                        insertComp.Parameters.AddWithValue("@Mobile", string.IsNullOrWhiteSpace(branchPhone) ? "123456789" : branchPhone.Trim());
                        insertComp.Parameters.AddWithValue("@Email", "admin@nexoris.com");
                        insertComp.Parameters.AddWithValue("@Website", "www.nexoris.com");
                        insertComp.Parameters.AddWithValue("@BusinessType", "Retail");
                        insertComp.Parameters.AddWithValue("@BackupPath", @"C:\Backup\");
                        insertComp.Parameters.AddWithValue("@FinYearFrom", new DateTime(DateTime.Now.Year, 1, 1));
                        insertComp.Parameters.AddWithValue("@FinYearTo", new DateTime(DateTime.Now.Year, 12, 31));
                        insertComp.Parameters.AddWithValue("@BookFrom", new DateTime(DateTime.Now.Year, 1, 1));
                        insertComp.Parameters.AddWithValue("@BookTo", new DateTime(DateTime.Now.Year, 12, 31));
                        insertComp.Parameters.AddWithValue("@TaxSystem", 1);
                        insertComp.Parameters.AddWithValue("@Currency", 1);
                        insertComp.Parameters.AddWithValue("@IsDelete", 0);

                        insertComp.ExecuteNonQuery();
                    }
                }
                else
                {
                    SqlCommand getCompId = new SqlCommand("SELECT MIN(CompanyID) FROM CompanyInfo WHERE IsDelete = 0", conn, transaction);
                    companyId = Convert.ToInt32(getCompId.ExecuteScalar());
                }

                // 2. Insert FinancialYear if empty
                SqlCommand checkFin = new SqlCommand("SELECT COUNT(*) FROM FinancialYear WHERE CompanyID = @CompanyID", conn, transaction);
                checkFin.Parameters.AddWithValue("@CompanyID", companyId);
                int finYearCount = Convert.ToInt32(checkFin.ExecuteScalar());

                if (finYearCount == 0)
                {
                    string insertFinSql = @"
                        INSERT INTO FinancialYear (CompanyID, FinYearFrom, FinYearTo, FinYearID, CurFinYear)
                        VALUES (@CompanyID, @FinYearFrom, @FinYearTo, @FinYearID, @CurFinYear)";

                    using (SqlCommand insertFin = new SqlCommand(insertFinSql, conn, transaction))
                    {
                        insertFin.Parameters.AddWithValue("@CompanyID", companyId);
                        insertFin.Parameters.AddWithValue("@FinYearFrom", new DateTime(DateTime.Now.Year, 1, 1));
                        insertFin.Parameters.AddWithValue("@FinYearTo", new DateTime(DateTime.Now.Year, 12, 31));
                        insertFin.Parameters.AddWithValue("@FinYearID", 1);
                        insertFin.Parameters.AddWithValue("@CurFinYear", 1);

                        insertFin.ExecuteNonQuery();
                    }
                }

                // 3. Insert Userlevels if empty for this company
                SqlCommand checkLevels = new SqlCommand("SELECT COUNT(*) FROM Userlevels WHERE CompanyId = @CompanyID", conn, transaction);
                checkLevels.Parameters.AddWithValue("@CompanyID", companyId);
                int levelCount = Convert.ToInt32(checkLevels.ExecuteScalar());

                if (levelCount == 0)
                {
                    string insertLevelSql = @"
                        INSERT INTO Userlevels (CompanyId, BranchID, UserLevelID, UserLevel) VALUES
                        (@CompanyID, 1, 1, 'Administrator'),
                        (@CompanyID, 1, 6, 'Cashier'),
                        (@CompanyID, 1, 7, 'Accountant'),
                        (@CompanyID, 1, 8, 'Sales Man'),
                        (@CompanyID, 1, 9, 'Purchase Manager'),
                        (@CompanyID, 1, 10, 'Transporter'),
                        (@CompanyID, 1, 11, 'StockTaker')";

                    using (SqlCommand insertLevels = new SqlCommand(insertLevelSql, conn, transaction))
                    {
                        insertLevels.Parameters.AddWithValue("@CompanyID", companyId);
                        insertLevels.ExecuteNonQuery();
                    }
                }

                // 4. Encrypt password and Insert Admin User if empty
                SqlCommand checkUsers = new SqlCommand("SELECT COUNT(*) FROM Users WHERE IsDelete = 0 OR IsDelete IS NULL", conn, transaction);
                int userCount = Convert.ToInt32(checkUsers.ExecuteScalar());

                if (userCount == 0)
                {
                    EncryptionAndDecryptionHelper enc = new EncryptionAndDecryptionHelper();
                    string encryptedPassword = enc.Encrypt(adminPassword, true);

                    string insertUserSql = @"
                        INSERT INTO Users (CompanyID, BranchID, UserLevelID, UserName, Password, IsDelete)
                        VALUES (@CompanyID, @BranchID, @UserLevelID, @UserName, @Password, @IsDelete)";

                    using (SqlCommand insertUser = new SqlCommand(insertUserSql, conn, transaction))
                    {
                        insertUser.Parameters.AddWithValue("@CompanyID", companyId);
                        insertUser.Parameters.AddWithValue("@BranchID", 1);
                        insertUser.Parameters.AddWithValue("@UserLevelID", 1); // Administrator
                        insertUser.Parameters.AddWithValue("@UserName", "admin");
                        insertUser.Parameters.AddWithValue("@Password", encryptedPassword);
                        insertUser.Parameters.AddWithValue("@IsDelete", 0);

                        insertUser.ExecuteNonQuery();
                    }
                }

                // Commit initial setup tables so that POS_Branch can access them if needed
                transaction.Commit();

                // 5. Call POS_Branch CREATE SP (creates branch, 29 groups, default ledgers, and TrackTrans record)
                using (SqlCommand cmdBranch = new SqlCommand("POS_Branch", conn))
                {
                    cmdBranch.CommandType = CommandType.StoredProcedure;
                    cmdBranch.Parameters.AddWithValue("@BranchName", branchName.Trim());
                    cmdBranch.Parameters.AddWithValue("@IsDelete", 0);
                    cmdBranch.Parameters.AddWithValue("@CompanyId", companyId);
                    cmdBranch.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(branchAddress) ? "Main Branch Office" : branchAddress.Trim());
                    cmdBranch.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(branchPhone) ? "123456789" : branchPhone.Trim());
                    cmdBranch.Parameters.AddWithValue("@FinYearId", 1);
                    cmdBranch.Parameters.AddWithValue("@IsECommerceAvailable", 0);
                    cmdBranch.Parameters.AddWithValue("@_Operation", "CREATE");

                    object spResult = cmdBranch.ExecuteScalar();
                    System.Diagnostics.Debug.WriteLine($"POS_Branch CREATE result: {spResult}");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during InitialSetupHelper.InitializeDatabase: {ex.Message}");
                if (transaction != null && transaction.Connection != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch { }
                }
                throw;
            }
            finally
            {
                if (repo != null)
                {
                    repo.Dispose();
                }
            }
        }
    }
}
