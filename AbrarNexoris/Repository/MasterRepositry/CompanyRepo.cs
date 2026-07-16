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
                            company = new CompanyModel
                            {
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                CompanyName = reader["CompanyName"].ToString(),
                                CompanyCaption = reader["CompanyCaption"] != DBNull.Value ? reader["CompanyCaption"].ToString() : null,
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
                                BusinessType = reader["BusinessType"] != DBNull.Value ? reader["BusinessType"].ToString() : null,
                                BackupPath = reader["BackupPath"] != DBNull.Value ? reader["BackupPath"].ToString() : null,
                                Logo = reader["LogoByteArray"] != DBNull.Value ? (byte[])reader["LogoByteArray"] : null,
                                FinYearFrom = reader["FinYearFrom"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FinYearFrom"]) : null,
                                FinYearTo = reader["FinYearTo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FinYearTo"]) : null,
                                BookFrom = reader["BookFrom"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["BookFrom"]) : null,
                                BookTo = reader["BookTo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["BookTo"]) : null,
                                TaxSystem = reader["TaxSystem"] != DBNull.Value ? (int?)Convert.ToInt32(reader["TaxSystem"]) : null,
                                TaxNo = reader["TaxNo"] != DBNull.Value ? reader["TaxNo"].ToString() : null,
                                LicenseNo = reader["LicenseNo"] != DBNull.Value ? reader["LicenseNo"].ToString() : null,
                                DLNO1 = reader["DLNO1"] != DBNull.Value ? reader["DLNO1"].ToString() : null,
                                DLNO2 = reader["DLNO2"] != DBNull.Value ? reader["DLNO2"].ToString() : null,
                                FSSAINo = reader["FSSAINo"] != DBNull.Value ? reader["FSSAINo"].ToString() : null,
                                Currency = reader["Currency"] != DBNull.Value ? (int?)Convert.ToInt32(reader["Currency"]) : null
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return company;
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

            DataConnection.Open();
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
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company dropdown list: {ex.Message}", ex);
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
