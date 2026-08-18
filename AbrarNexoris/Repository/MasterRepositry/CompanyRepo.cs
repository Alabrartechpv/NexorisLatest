using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Master;

namespace Repository.MasterRepositry
{
    public class CompanyRepo : BaseRepostitory
    {
        public CompanyDDlGrid GetAllCompanies(string companyName = null, int pageIndex = 0, int pageSize = 10, string sortBy = "CompanyID", string sortDirection = "DESC")
        {
            CompanyDDlGrid companyGrid = new CompanyDDlGrid();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyName", companyName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    cmd.Parameters.AddWithValue("@SortBy", sortBy);
                    cmd.Parameters.AddWithValue("@SortByDirection", sortDirection);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);

                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            List<CompanyDDl> companies = new List<CompanyDDl>();
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                companies.Add(new CompanyDDl
                                {
                                    CompanyID = Convert.ToInt32(row["CompanyID"]),
                                    CompanyName = row["CompanyName"].ToString()
                                });
                            }
                            companyGrid.List = companies;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving companies: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return companyGrid;
        }

        public CompanyModel GetCompanyById(int companyId)
        {
            CompanyModel company = null;

            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            company = ReadCompanyFromDataReader(reader);
                        }
                    }
                }

                // Fallback: If SP returned null, query table directly
                if (company == null)
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM _CompanyInfo WHERE CompanyID = @CompanyID", (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                company = ReadCompanyFromDataReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Direct query fallback on exception
                try
                {
                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM _CompanyInfo WHERE CompanyID = @CompanyID", (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                company = ReadCompanyFromDataReader(reader);
                            }
                        }
                    }
                }
                catch { /* Ignore fallback exceptions */ }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return company;
        }

        private CompanyModel ReadCompanyFromDataReader(SqlDataReader reader)
        {
            byte[] logoData = null;
            if (HasColumn(reader, "LogoByteArray") && reader["LogoByteArray"] != DBNull.Value)
            {
                logoData = (byte[])reader["LogoByteArray"];
            }
            else if (HasColumn(reader, "Logo") && reader["Logo"] != DBNull.Value)
            {
                logoData = (byte[])reader["Logo"];
            }

            return new CompanyModel
            {
                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                CompanyName = reader["CompanyName"].ToString(),
                CompanyCaption = HasColumn(reader, "CompanyCaption") && reader["CompanyCaption"] != DBNull.Value ? reader["CompanyCaption"].ToString() : null,
                Address1 = HasColumn(reader, "Address1") && reader["Address1"] != DBNull.Value ? reader["Address1"].ToString() : null,
                Address2 = HasColumn(reader, "Address2") && reader["Address2"] != DBNull.Value ? reader["Address2"].ToString() : null,
                Address3 = HasColumn(reader, "Address3") && reader["Address3"] != DBNull.Value ? reader["Address3"].ToString() : null,
                Address4 = HasColumn(reader, "Address4") && reader["Address4"] != DBNull.Value ? reader["Address4"].ToString() : null,
                Country = HasColumn(reader, "Country") && reader["Country"] != DBNull.Value ? (int?)Convert.ToInt32(reader["Country"]) : null,
                State = HasColumn(reader, "State") && reader["State"] != DBNull.Value ? (int?)Convert.ToInt32(reader["State"]) : null,
                Zipcode = HasColumn(reader, "Zipcode") && reader["Zipcode"] != DBNull.Value ? reader["Zipcode"].ToString() : null,
                Phone = HasColumn(reader, "Phone") && reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                Mobile = HasColumn(reader, "Mobile") && reader["Mobile"] != DBNull.Value ? reader["Mobile"].ToString() : null,
                Email = HasColumn(reader, "Email") && reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                Website = HasColumn(reader, "Website") && reader["Website"] != DBNull.Value ? reader["Website"].ToString() : null,
                BusinessType = HasColumn(reader, "BusinessType") && reader["BusinessType"] != DBNull.Value ? reader["BusinessType"].ToString() : null,
                BackupPath = HasColumn(reader, "BackupPath") && reader["BackupPath"] != DBNull.Value ? reader["BackupPath"].ToString() : null,
                Logo = logoData,
                FinYearFrom = HasColumn(reader, "FinYearFrom") && reader["FinYearFrom"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FinYearFrom"]) : null,
                FinYearTo = HasColumn(reader, "FinYearTo") && reader["FinYearTo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FinYearTo"]) : null,
                BookFrom = HasColumn(reader, "BookFrom") && reader["BookFrom"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["BookFrom"]) : null,
                BookTo = HasColumn(reader, "BookTo") && reader["BookTo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["BookTo"]) : null,
                TaxSystem = HasColumn(reader, "TaxSystem") && reader["TaxSystem"] != DBNull.Value ? (int?)Convert.ToInt32(reader["TaxSystem"]) : null,
                TaxNo = HasColumn(reader, "TaxNo") && reader["TaxNo"] != DBNull.Value ? reader["TaxNo"].ToString() : null,
                LicenseNo = HasColumn(reader, "LicenseNo") && reader["LicenseNo"] != DBNull.Value ? reader["LicenseNo"].ToString() : null,
                DLNO1 = HasColumn(reader, "DLNO1") && reader["DLNO1"] != DBNull.Value ? reader["DLNO1"].ToString() : null,
                DLNO2 = HasColumn(reader, "DLNO2") && reader["DLNO2"] != DBNull.Value ? reader["DLNO2"].ToString() : null,
                FSSAINo = HasColumn(reader, "FSSAINo") && reader["FSSAINo"] != DBNull.Value ? reader["FSSAINo"].ToString() : null,
                Currency = HasColumn(reader, "Currency") && reader["Currency"] != DBNull.Value ? (int?)Convert.ToInt32(reader["Currency"]) : null
            };
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public CompanyModel GetCompanyByUserAndBranch(int userId, int branchId, int companyId)
        {
            CompanyModel company = null;

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyDetails, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            company = new CompanyModel
                            {
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                CompanyName = reader["CompanyName"].ToString(),
                                Address1 = reader["Address1"] != DBNull.Value ? reader["Address1"].ToString() : null,
                                Address2 = reader["Address2"] != DBNull.Value ? reader["Address2"].ToString() : null,
                                Address3 = reader["Address3"] != DBNull.Value ? reader["Address3"].ToString() : null,
                                Address4 = reader["Address4"] != DBNull.Value ? reader["Address4"].ToString() : null,
                                Country = reader["Country"] != DBNull.Value ? (int?)Convert.ToInt32(reader["Country"]) : null,
                                State = reader["State"] != DBNull.Value ? (int?)Convert.ToInt32(reader["State"]) : null,
                                Zipcode = reader["Zipcode"] != DBNull.Value ? reader["Zipcode"].ToString() : null,
                                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                                Mobile = reader["Mobile"] != DBNull.Value ? reader["Mobile"].ToString() : null,
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                                Website = reader["Website"] != DBNull.Value ? reader["Website"].ToString() : null,
                                TaxNo = reader["TaxNo"] != DBNull.Value ? reader["TaxNo"].ToString() : null,
                                LicenseNo = reader["LicenseNo"] != DBNull.Value ? reader["LicenseNo"].ToString() : null,
                                DLNO1 = reader["DLNO1"] != DBNull.Value ? reader["DLNO1"].ToString() : null,
                                DLNO2 = reader["DLNO2"] != DBNull.Value ? reader["DLNO2"].ToString() : null,
                                FSSAINo = reader["FSSAINo"] != DBNull.Value ? reader["FSSAINo"].ToString() : null
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company details: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return company;
        }

        public string CreateCompany(CompanyModel company)
        {
            string result = string.Empty;

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CompanyName", company.CompanyName);
                    cmd.Parameters.AddWithValue("@CompanyCaption", company.CompanyCaption ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address1", company.Address1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address2", company.Address2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address3", company.Address3 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address4", company.Address4 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", company.Country ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", company.State ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Zipcode", company.Zipcode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", company.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Mobile", company.Mobile ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", company.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Website", company.Website ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BusinessType", company.BusinessType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BackupPath", company.BackupPath ?? (object)DBNull.Value);
                    // Explicitly set SqlDbType.VarBinary to prevent type inference issues with null values
                    var logoParam = cmd.Parameters.Add("@Logo", SqlDbType.VarBinary, -1); // -1 for MAX
                    logoParam.Value = company.Logo ?? (object)DBNull.Value;
                    cmd.Parameters.AddWithValue("@FinYearFrom", company.FinYearFrom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FinYearTo", company.FinYearTo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BookFrom", company.BookFrom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BookTo", company.BookTo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxSystem", company.TaxSystem ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxNo", company.TaxNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LicenseNo", company.LicenseNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DLNO1", company.DLNO1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DLNO2", company.DLNO2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FSSAINo", company.FSSAINo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Currency", company.Currency ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                    result = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating company: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public string UpdateCompany(CompanyModel company)
        {
            string result = string.Empty;

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CompanyID", company.CompanyID);
                    cmd.Parameters.AddWithValue("@CompanyName", company.CompanyName);
                    cmd.Parameters.AddWithValue("@CompanyCaption", company.CompanyCaption ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address1", company.Address1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address2", company.Address2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address3", company.Address3 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address4", company.Address4 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", company.Country ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", company.State ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Zipcode", company.Zipcode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", company.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Mobile", company.Mobile ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", company.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Website", company.Website ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BusinessType", company.BusinessType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BackupPath", company.BackupPath ?? (object)DBNull.Value);
                    // Explicitly set SqlDbType.VarBinary to prevent type inference issues with null values
                    var logoParam = cmd.Parameters.Add("@Logo", SqlDbType.VarBinary, -1); // -1 for MAX
                    logoParam.Value = company.Logo ?? (object)DBNull.Value;
                    cmd.Parameters.AddWithValue("@FinYearFrom", company.FinYearFrom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FinYearTo", company.FinYearTo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BookFrom", company.BookFrom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BookTo", company.BookTo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxSystem", company.TaxSystem ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxNo", company.TaxNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LicenseNo", company.LicenseNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DLNO1", company.DLNO1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DLNO2", company.DLNO2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FSSAINo", company.FSSAINo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Currency", company.Currency ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                    result = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating company: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public string DeleteCompany(int companyId)
        {
            string result = string.Empty;

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    result = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting company: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public List<CompanyDDl> GetCompanyDropdownList()
        {
            List<CompanyDDl> companies = new List<CompanyDDl>();

            if (DataConnection.State != ConnectionState.Open)
                DataConnection.Open();

            try
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "DDL");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                companies.Add(new CompanyDDl
                                {
                                    CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                    CompanyName = reader["CompanyName"].ToString()
                                });
                            }
                        }
                    }
                }
                catch { /* SP DDL operation may not exist, fallback below */ }

                // Fallback 1: If DDL returned 0 rows, try @_Operation = 'GETALL'
                if (companies.Count == 0)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CompanyInfo, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    companies.Add(new CompanyDDl
                                    {
                                        CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                        CompanyName = reader["CompanyName"].ToString()
                                    });
                                }
                            }
                        }
                    }
                    catch { /* SP GETALL may fail, fallback below */ }
                }

                // Fallback 2: Direct SQL Query across candidate company tables
                if (companies.Count == 0)
                {
                    string[] candidateTables = new string[] { "_CompanyInfo", "CompanyInfo", "CompanyMaster", "Company" };
                    foreach (string table in candidateTables)
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand($"SELECT CompanyID, CompanyName FROM dbo.{table}", (SqlConnection)DataConnection))
                            {
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        companies.Add(new CompanyDDl
                                        {
                                            CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                            CompanyName = reader["CompanyName"].ToString()
                                        });
                                    }
                                }
                            }
                            if (companies.Count > 0) break;
                        }
                        catch { /* Try next table candidate */ }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving company dropdown list: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return companies;
        }
    }
}
