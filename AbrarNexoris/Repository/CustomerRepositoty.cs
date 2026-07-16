using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using Dapper;


namespace Repository
{
    public class CustomerRepositoty : BaseRepostitory
    {
        public string SaveCustomer(ClsCustomers ObjCust, CustomerAddress ObjCustAddr)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                // Create a clean object with only the fields that POS_Customer stored procedure expects
                var customerForLedger = new
                {
                    CompanyId = ObjCust.CompanyId,
                    BranchId = ObjCust.BranchId,
                    LedgerId = ObjCust.LedgerId,
                    LedgerName = ObjCust.LedgerName,
                    AliasName = ObjCust.AliasName,
                    Description = ObjCust.Description,
                    Notes = ObjCust.Notes,
                    PriceLevel = ObjCust.PriceLevel,
                    OpenDebit = ObjCust.OpenDebit,
                    OpenCredit = ObjCust.OpenCredit,
                    _Operation = "GENERATELEDGER"
                };

                List<ClsCustomers> GetGenLedgerNO = DataConnection.Query<ClsCustomers>(STOREDPROCEDURE.POS_Customer, customerForLedger, trans, commandType: CommandType.StoredProcedure).ToList<ClsCustomers>();

                if (GetGenLedgerNO.Count > 0)
                {
                    foreach (ClsCustomers LedgerNo in GetGenLedgerNO)
                    {
                        ObjCust.LedgerId = LedgerNo.LedgerId;
                        ObjCustAddr.LedgerId = LedgerNo.LedgerId;
                        ObjCustAddr.LedgerName = ObjCust.LedgerName;
                        ObjCustAddr.CompanyId = ObjCust.CompanyId;
                        ObjCustAddr.BranchId = ObjCust.BranchId;
                    }

                }

                // Create a clean object for CREATE operation
                var customerForCreate = new
                {
                    CompanyId = ObjCust.CompanyId,
                    BranchId = ObjCust.BranchId,
                    LedgerId = ObjCust.LedgerId,
                    LedgerName = ObjCust.LedgerName,
                    AliasName = ObjCust.AliasName,
                    Description = ObjCust.Description,
                    Notes = ObjCust.Notes,
                    PriceLevel = ObjCust.PriceLevel,
                    OpenDebit = ObjCust.OpenDebit,
                    OpenCredit = ObjCust.OpenCredit,
                    _Operation = "CREATE"
                };

                List<ClsCustomers> ListCust = DataConnection.Query<ClsCustomers>(STOREDPROCEDURE.POS_Customer, customerForCreate, trans, commandType: CommandType.StoredProcedure).ToList<ClsCustomers>();
                if (ListCust.Count > 0)
                {

                }
                ObjCustAddr._Operation = "CREATE";
                DataConnection.Query<CustomerAddress>(STOREDPROCEDURE.POS_Customer_ContactDetails, ObjCustAddr, trans, commandType: CommandType.StoredProcedure).ToList<CustomerAddress>();
                // Always commit — INSERTs don't return rows, so there's no count to check
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;

            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return "Success";
        }



        public CustomerDDLGrids GetCustomerDDL()
        {
            CustomerDDLGrids ObjCustomerDDLGrid = new CustomerDDLGrids();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Customer, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ObjCustomerDDLGrid.List = ds.Tables[0].ToListOfObject<CustomerGridList>();
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

            return ObjCustomerDDLGrid;
        }

        public CustAddressDDLGrids getCustAddress(int LedgerId)
        {
            CustAddressDDLGrids ObjCustAddrss = new CustAddressDDLGrids();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Customer, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@LedgerId", LedgerId);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ObjCustAddrss.ListCustomer = ds.Tables[0].ToListOfObject<ClsCustomers>();
                        }

                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            ObjCustAddrss.ListCustAddress = ds.Tables[1].ToListOfObject<CustomerAddress>();
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

            return ObjCustAddrss;
        }

        public string UpdateCstomerAddress(ClsCustomers objCust, CustomerAddress objCustAddress)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                // Create a clean object with only the fields that POS_Customer stored procedure expects
                var customerForUpdate = new
                {
                    CompanyId = objCust.CompanyId,
                    BranchId = objCust.BranchId,
                    LedgerId = objCust.LedgerId,
                    LedgerName = objCust.LedgerName,
                    AliasName = objCust.AliasName,
                    Description = objCust.Description,
                    Notes = objCust.Notes,
                    PriceLevel = objCust.PriceLevel,
                    OpenDebit = objCust.OpenDebit,
                    OpenCredit = objCust.OpenCredit,
                    _Operation = "Update"
                };

                List<ClsCustomers> ListObjCust = DataConnection.Query<ClsCustomers>(STOREDPROCEDURE.POS_Customer, customerForUpdate, trans, commandType: CommandType.StoredProcedure).ToList<ClsCustomers>();

                if (ListObjCust.Count > 0)
                {

                }
                objCustAddress._Operation = "Update";
                List<CustomerAddress> ListObjCustAddress = DataConnection.Query<CustomerAddress>(STOREDPROCEDURE.POS_Customer_ContactDetails, objCustAddress, trans, commandType: CommandType.StoredProcedure).ToList<CustomerAddress>();

                if (ListObjCustAddress.Count > 0)
                {

                }

                trans.Commit();


            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;

            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return "SUCCESS";
        }

        public CustomerDDLGrids SearchCustomers(string searchText)
        {
            CustomerDDLGrids ObjCustomerDDLGrid = new CustomerDDLGrids();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Customer, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@LedgerName", searchText);
                    cmd.Parameters.AddWithValue("@_Operation", "Search");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ObjCustomerDDLGrid.List = ds.Tables[0].ToListOfObject<CustomerGridList>();
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

            return ObjCustomerDDLGrid;
        }

        // Debug method to test stored procedure output directly
        public DataTable TestGetCustomerDDL()
        {
            DataTable resultTable = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Customer, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null))
                        {
                            resultTable = ds.Tables[0];

                            // Debug: Log the actual data structure
                            System.Diagnostics.Debug.WriteLine("=== STORED PROCEDURE OUTPUT DEBUG ===");
                            System.Diagnostics.Debug.WriteLine($"Table has {resultTable.Rows.Count} rows and {resultTable.Columns.Count} columns");

                            // Log column names
                            System.Diagnostics.Debug.WriteLine("Columns:");
                            foreach (DataColumn col in resultTable.Columns)
                            {
                                System.Diagnostics.Debug.WriteLine($"  {col.ColumnName} ({col.DataType.Name})");
                            }

                            // Log first row data
                            if (resultTable.Rows.Count > 0)
                            {
                                System.Diagnostics.Debug.WriteLine("First row data:");
                                var firstRow = resultTable.Rows[0];
                                foreach (DataColumn col in resultTable.Columns)
                                {
                                    var value = firstRow[col.ColumnName];
                                    System.Diagnostics.Debug.WriteLine($"  {col.ColumnName}: {value}");
                                }
                            }
                            System.Diagnostics.Debug.WriteLine("=====================================");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TestGetCustomerDDL: {ex.Message}");
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return resultTable;
        }

        public ClsCustomers GetCustomerById(int customerId)
        {
            DataConnection.Open();
            try
            {
                string query = @"
                    SELECT l.LedgerID, l.LedgerName, l.Alias as AliasName, l.Description, l.Notes, 
                           l.PriceLevel, l.OpnDebit as OpenDebit, l.OpnCredit as OpenCredit, 
                           l.CompanyID as CompanyId, l.BranchID as BranchId,
                           ISNULL(cd.SSMNumber, '') as SSMNumber, 
                           ISNULL(cd.TINNumber, '') as TINNumber, 
                           ISNULL(cd.CompanyName, '') as CompanyName, 
                           ISNULL(cd.CompanyTIN, '') as CompanyTIN, 
                           ISNULL(cd.CompanyMSICCode, '') as CompanyMSICCode, 
                           ISNULL(cd.CompanyEmail, '') as CompanyEmail
                    FROM LedgerMaster l 
                    LEFT JOIN ContactDetails cd ON l.LedgerID = cd.LedgerID
                    WHERE l.LedgerID = @CustomerId AND l.GroupID = 16";

                var customer = DataConnection.Query<ClsCustomers>(query, new { CustomerId = customerId }).FirstOrDefault();
                return customer;
            }
            finally
            {
                DataConnection.Close();
            }
        }
    }


}
